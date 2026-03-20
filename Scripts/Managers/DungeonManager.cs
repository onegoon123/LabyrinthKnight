using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkNaku.Number;
using DG.Tweening;

/// <summary>
/// [활성 던전 매니저]
/// 플레이어가 직접 입장하는 특정 던전(웨이브 방식, 보스전 등)의 진행을 담당합니다.
/// 방치형 모드(DungeonSystem)와는 별개로 동작하며, 던전 입장 시 방치 모드를 일시정지시킵니다.
/// </summary>
public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("상태")]
    public bool isDungeonActive = false; // 현재 던전 진행 중 여부
    public DungeonData currentDungeon; // 현재 진행 중인 던전 데이터
    public int currentWaveIndex = 0; // 현재 웨이브 인덱스
    
    [Header("참조")]
    [Header("참조")]
    // public Transform[] spawnPoints; // Removed: 모든 스폰은 EnemySpawner를 통함
    public DungeonSystem idleDungeonSystem; // 기존 방치형 시스템 참조 (일시정지용)
    
    [Header("전투 상태")]
    private List<Enemy> activeEnemies = new List<Enemy>(); // 현재 활성화된 적 목록
    private int enemiesKilledInWave = 0; // 현재 웨이브에서 처치한 적 수
    private int totalEnemiesInWave = 0; // 현재 웨이브의 총 적 수
    private bool isWaveSpawning = false; // 웨이브 스폰 진행 중 여부
    
    private Coroutine waveCoroutine; // 현재 실행 중인 웨이브 코루틴 (개별 중지용)
    
    // 이벤트
    public event System.Action<DungeonData> OnDungeonEnter; // 던전 입장 시
    public event System.Action<bool> OnDungeonExit; // 던전 퇴장 시 (true: 클리어, false: 실패/포기)
    public event System.Action<int, int> OnWaveProgress; // 웨이브 진행 상황 (현재, 총 웨이브)
    
    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (idleDungeonSystem == null)
            idleDungeonSystem = FindFirstObjectByType<DungeonSystem>();
            
        // 플레이어 사망 이벤트 처리가 필요하다면 여기서 연결하거나 GameManager를 통해 처리
    }
    
    [ContextMenu("Quick Start Test Dungeon")]
    public void QuickStartTestDungeon()
    {
        if (currentDungeon != null)
        {
            EnterDungeon(currentDungeon);
        }
        else
        {
            Debug.LogWarning("테스트할 DungeonData가 할당되지 않았습니다!");
        }
    }

    /// <summary>
    /// 특정 던전에 입장합니다.
    /// </summary>
    public void EnterDungeon(DungeonData dungeon)
    {
        if (isDungeonActive) return;
        StartCoroutine(EnterDungeonRoutine(dungeon));
    }
    
    private IEnumerator EnterDungeonRoutine(DungeonData dungeon)
    {
        // 페이드 아웃
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeOut(); // 인자 제거
            yield return new WaitForSeconds(SceneFadeManager.Instance.fadeDuration);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        currentDungeon = dungeon;
        isDungeonActive = true;
        currentWaveIndex = 0;
        
        // 방치형 시스템(무한 스테이지) 일시정지 및 필드 정리
        // 방치형 시스템(무한 스테이지) 일시정지 및 필드 정리
        if (idleDungeonSystem == null)
            idleDungeonSystem = FindFirstObjectByType<DungeonSystem>();

        if (idleDungeonSystem != null)
        {
            idleDungeonSystem.StopDungeon();
            idleDungeonSystem.CleanupAllEnemies();
            
            // 던전 테마 적용 (맵 재생성)
            if (dungeon.theme != null && idleDungeonSystem.mapManager != null)
            {
                idleDungeonSystem.mapManager.GenerateMap(dungeon.theme);
            }
        }
        
        // 잔존 몬스터 강제 정리 (리스트 누락 대비, 전체 검색)
        Enemy[] remainingEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var enemy in remainingEnemies)
        {
             if (enemy != null)
             {
                 EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
                 if (spawner != null) spawner.ReleaseEnemy(enemy);
                 else Destroy(enemy.gameObject);
             }
        }
        
        // 플레이어/동료 위치 초기화 (중앙)
        ResetPositions();

        OnDungeonEnter?.Invoke(dungeon);
        
        // 페이드 인
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeIn(); // 인자 제거
            yield return new WaitForSeconds(SceneFadeManager.Instance.fadeDuration);
        }
        
        // 첫 웨이브 시작
        if (waveCoroutine != null) StopCoroutine(waveCoroutine);
        waveCoroutine = StartCoroutine(StartWaveRoutine());
        
        // 플레이어 사망 이벤트 구독
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player != null)
        {
            player.OnDeath -= HandlePlayerDeath; // 중복 방지
            player.OnDeath += HandlePlayerDeath;
        }
    }
    
    private void ResetPositions()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null) player.transform.position = Vector3.zero;
        
        if (CompanionSystem.Instance != null && CompanionSystem.Instance.HasActiveCompanion())
        {
            var companion = CompanionSystem.Instance.GetActiveCompanion();
            if (companion != null) companion.transform.position = Vector3.zero;
        }
    }
    
    /// <summary>
    /// 던전에서 퇴장합니다. (클리어 또는 실패)
    /// </summary>
    public void ExitDungeon(bool isClear)
    {
        if (!isDungeonActive) return;
        StartCoroutine(ExitDungeonRoutine(isClear));
    }

    private IEnumerator ExitDungeonRoutine(bool isClear)
    {
        isDungeonActive = false;
        
        // 웨이브 진행 중단 (자기 자신인 ExitDungeonRoutine은 멈추면 안 됨)
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }
        
        // 페이드 아웃
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeOut(); // 인자 제거
            yield return new WaitForSeconds(SceneFadeManager.Instance.fadeDuration);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 남은 적 제거
        ClearActiveEnemies();
        
        // 클리어 보상 지급
        if (isClear && currentDungeon != null)
        {
            GiveRewards();
        }
        
        OnDungeonExit?.Invoke(isClear);
        currentDungeon = null;
        
        // 플레이어/동료 위치 초기화 (메인 복귀 시)
        ResetPositions();
        
        // 방치형 시스템(무한 스테이지) 재개
        if (idleDungeonSystem != null)
        {
            idleDungeonSystem.StartDungeon();
        }
        
        // 플레이어 사망 이벤트 구독 해제
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player != null)
        {
            player.OnDeath -= HandlePlayerDeath;
            // 던전 실패 시 체력 회복 (선택 사항: 메인 복귀 시 풀피)
            if (!isClear)
            {
                player.Heal(player.maxHealth); 
                
                // 플레이어 투명도/애니메이션 복구
                var sprite = player.GetComponent<SpriteRenderer>();
                if (sprite != null)
                {
                    sprite.color = Color.white;
                    sprite.DOFade(1f, 0.5f);
                }
                
                // 플레이어 컨트롤러 활성화 (사망 시 비활성화되었을 수 있음)
                var controller = player.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.enabled = true;
                    // 애니메이션 리셋?
                }
            }
        }

        // 페이드 인
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeIn(); // 인자 제거
        }
    }
    
    private void GiveRewards()
    {
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player != null && currentDungeon != null)
        {
            player.gold += currentDungeon.rewards.goldReward;
            player.AddExperience(currentDungeon.rewards.expReward);
            // 추후 아이템 보상 로직 추가 가능
        }
    }
    
    /// <summary>
    /// 웨이브 진행 코루틴
    /// </summary>
    private IEnumerator StartWaveRoutine()
    {
        // 모든 웨이브를 클리어했는지 확인
        if (currentDungeon == null || currentWaveIndex >= currentDungeon.waves.Count)
        {
            ExitDungeon(true); // 던전 클리어
            yield break;
        }
        
        DungeonWave wave = currentDungeon.waves[currentWaveIndex];
        enemiesKilledInWave = 0;
        
        // 이번 웨이브의 총 적 수 계산 (일반 몹 + 보스)
        totalEnemiesInWave = wave.spawnCount;
        if (wave.bossData != null) totalEnemiesInWave++;
        
        OnWaveProgress?.Invoke(currentWaveIndex + 1, currentDungeon.waves.Count);
        
        // 적 스폰 시작
        yield return StartCoroutine(SpawnWaveEnemies(wave));
    }
    
    private IEnumerator SpawnWaveEnemies(DungeonWave wave)
    {
        isWaveSpawning = true;
        
        // 일반 몬스터 스폰 루프
        for (int i = 0; i < wave.spawnCount; i++)
        {
            if (!isDungeonActive) yield break;
            
            // 스폰할 적 종류 랜덤 선택
            if (wave.enemiesToSpawn != null && wave.enemiesToSpawn.Count > 0)
            {
                EnemyData randomEnemy = wave.enemiesToSpawn[Random.Range(0, wave.enemiesToSpawn.Count)];
                SpawnEnemy(randomEnemy, false);
            }
            
            yield return new WaitForSeconds(wave.spawnInterval);
        }
        
        // 보스 스폰 (해당 웨이브에 보스가 있다면)
        if (wave.bossData != null)
        {
            SpawnBoss(wave.bossData);
        }
        
        isWaveSpawning = false;
    }
    
    private void SpawnEnemy(EnemyData data, bool isBoss)
    {
        if (data == null) return;
        
        // EnemySpawner(풀링 시스템)를 통해 스폰
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            Vector3 spawnPos = spawner.GetRandomSpawnPosition();
            
            // 주의: EnemyData는 ScriptableObject이므로 원본을 수정하면 안 됩니다.
            // 런타임에 레벨 보정이 필요하다면 Instantiate로 복제해서 사용해야 합니다.
            EnemyData instanceData = Instantiate(data); 
            
            Enemy enemy = spawner.SpawnEnemy(instanceData, spawnPos);
            
            if (enemy != null)
            {
                enemy.OnEnemyDefeated += OnEnemyDefeated;
                activeEnemies.Add(enemy);
                
                if (isBoss)
                {
                    // 보스는 크기를 키우는 등 추가 설정
                    enemy.transform.localScale = Vector3.one * 1.5f;
                }
                else
                {
                    // 일반 몹은 크기 초기화 (풀링 재사용 시 중요)
                    enemy.transform.localScale = Vector3.one;
                }
            }
        }
    }
    
    private void SpawnBoss(EnemyData bossData)
    {
        SpawnEnemy(bossData, true);
    }
    
    /// <summary>
    /// 적 처치 시 호출되는 콜백
    /// </summary>
    private void OnEnemyDefeated(Enemy enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            enemiesKilledInWave++;
            
            // 웨이브 클리어 조건 체크:
            // 1. 더 이상 스폰할 적이 없고 (isWaveSpawning == false)
            // 2. 현재 살아있는 적이 없을 때 (activeEnemies.Count == 0)
            if (!isWaveSpawning && activeEnemies.Count == 0)
            {
                currentWaveIndex++;
                if (waveCoroutine != null) StopCoroutine(waveCoroutine);
                waveCoroutine = StartCoroutine(StartWaveRoutine()); // 다음 웨이브 진행
            }
        }
    }
    
    public void HandlePlayerDeath()
    {
        if (isDungeonActive)
        {
            ExitDungeon(false); // 던전 실패
        }
    }
    
    private void ClearActiveEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject); // 풀링 시스템 사용 시 Release로 변경 권장
            }
        }
        activeEnemies.Clear();
    }
}
