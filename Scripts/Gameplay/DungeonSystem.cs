using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 던전 시스템을 관리하는 클래스입니다.
/// 스테이지 진행, 적 스폰, 전투 승패 처리 등을 담당합니다.
/// </summary>
public class DungeonSystem : MonoBehaviour
{
    [Header("던전 설정")]
    public bool isDungeonActive = true; // 던전 진행 활성화 여부
    
    [Header("무한 스테이지 설정")]
    public List<StageThemeData> stageThemes; // 스테이지별 테마 데이터 (배경, 적 종류 등)
    public int currentStage = 1; // 현재 진행 중인 스테이지 번호
    public int enemiesPerStage = 5; // 스테이지 당 처치해야 할 적의 수
    private int remainingEnemiesInStage = 0; // 현재 스테이지에 남은 적 수
    
    [Header("적 스폰 설정")]
    public GameObject enemyPrefab; // 적 프리팹 (EnemySpawner가 없을 때 사용)
    
    [Header("환경 설정")]
    public DungeonMapManager mapManager; // 맵(타일맵) 생성 관리자
    
    [Header("던전 통계")]
    public int totalBattles = 0;
    public int totalVictories = 0;
    public int totalDefeats = 0;
    
    [Header("시스템 참조")]
    public PlayerStats playerStats;
    public PlayerController playerController;
    public EnemySpawner enemySpawner; // 적 생성 및 풀링 담당
    public ProgressionSystem progressionSystem;
    
    // 현재 활성화된 적들의 목록 (관리 및 정리용)
    private List<Enemy> activeEnemies = new List<Enemy>();
    
    [Header("성능 최적화")]
    public float enemyUpdateInterval = 0.1f; // 적 목록 업데이트 간격
    private float lastEnemyUpdateTime = 0f;

    [Header("이펙트")]
    public GameObject respawnEffectPrefab;

    private Coroutine spawnCoroutine;
    
    // 외부에서 구독할 수 있는 이벤트들
    public event System.Action<int> OnStageStart; // 스테이지 시작 시 발생 (인자: 스테이지 번호)
    public event System.Action<int> OnStageClear; // 스테이지 클리어 시 발생
    public event System.Action OnPlayerDefeated; // 플레이어 사망 시 발생

    private void Start()
    {
        // 필수 컴포넌트들을 자동으로 찾아서 연결합니다. (Inspector 누락 방지)
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
                playerStats = FindFirstObjectByType<PlayerStats>();
        }
        
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
                playerController = FindFirstObjectByType<PlayerController>();
        }
        
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (progressionSystem == null)
            progressionSystem = FindFirstObjectByType<ProgressionSystem>();
            
        // 주의: GameManager가 StartDungeon을 호출하므로 여기서는 호출하지 않습니다. (중복 실행 방지)
    }
    
    private void Update()
    {
        // 던전이 활성화되어 있고 플레이어가 살아있을 때만 로직 수행
        if (isDungeonActive && playerStats != null && playerStats.IsAlive())
        {
            // 주기적으로 죽은 적(null이거나 isAlive가 false인 적)을 목록에서 제거하여 메모리/성능 관리
            if (Time.time - lastEnemyUpdateTime >= enemyUpdateInterval)
            {
                CleanupDeadEnemies();
                lastEnemyUpdateTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// 던전(방치 모드)을 시작합니다.
    /// </summary>
    public void StartDungeon()
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }
        
        if (enemySpawner == null)
        {
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
            if (enemySpawner == null)
            {
                Debug.LogError("[DungeonSystem] EnemySpawner가 없습니다! 적을 스폰할 수 없습니다.");
                return;
            }
        }
        
        isDungeonActive = true;

        // 이어하기 여부 판단
        int totalGoalCount;
        int enemiesToSpawn;
        bool regenerateMap;

        if (remainingEnemiesInStage > 0 && remainingEnemiesInStage < enemiesPerStage)
        {
            // 이어하기: 남은 적 수만큼만 스폰하고 맵은 유지
            // 이미 소환된 적이 있다면 그만큼 제외하고 추가 소환
            totalGoalCount = remainingEnemiesInStage;
            enemiesToSpawn = remainingEnemiesInStage - activeEnemies.Count;
            if (enemiesToSpawn < 0) enemiesToSpawn = 0;
            
            regenerateMap = false;
        }
        else
        {
            // 새 시작: 전체 적 스폰하고 맵 새로 생성
            CleanupAllEnemies(); // 이전 적이 있다면 정리
            
            totalGoalCount = enemiesPerStage;
            enemiesToSpawn = enemiesPerStage;
            regenerateMap = true;
        }

        StartStage(currentStage, totalGoalCount, enemiesToSpawn, regenerateMap);
    }
    
    /// <summary>
    /// 던전을 중지합니다. (다른 콘텐츠 진입 시 호출)
    /// </summary>
    public void StopDungeon(bool clearEnemies = true)
    {
        isDungeonActive = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        if (clearEnemies)
        {
            CleanupAllEnemies(); // 모든 적 제거
        }
    }

    /// <summary>
    /// 현재 필드에 있는 모든 적을 제거합니다.
    /// </summary>
    public void CleanupAllEnemies()
    {
        // 안전한 제거를 위해 리스트 복사 후 처리
        var enemiesToCleanup = new List<Enemy>(activeEnemies);
        foreach (Enemy enemy in enemiesToCleanup)
        {
            if (enemy != null)
            {
                // 이벤트 구독 해제
                enemy.OnEnemyDefeated -= OnEnemyDefeated;
                
                if (enemySpawner != null)
                {
                    enemySpawner.ReleaseEnemy(enemy);
                }
                else
                {
                    Destroy(enemy.gameObject);
                }
            }
        }
        activeEnemies.Clear();
    }
    
    /// <summary>
    /// 특정 스테이지를 시작합니다.
    /// </summary>
    private void StartStage(int stage, int totalGoalCount, int enemiesToSpawn, bool regenerateMap)
    {
        currentStage = stage;
        remainingEnemiesInStage = totalGoalCount; // 목표 처치 수 설정
        
        // 테마 결정 (예: 10스테이지마다 테마 변경)
        StageThemeData currentTheme = null;
        if (stageThemes != null && stageThemes.Count > 0)
        {
            int themeIndex = ((stage - 1) / 10) % stageThemes.Count;
            currentTheme = stageThemes[themeIndex];
        }
        else
        {
            Debug.LogWarning($"[DungeonSystem] StageThemes 목록이 비어있습니다! 배경 맵을 생성할 수 없습니다.");
        }
        
        if (currentTheme == null)
        {
             Debug.LogWarning($"[DungeonSystem] 현재 스테이지({stage})의 테마가 null입니다. 기본 맵이 없으면 적이 추락할 수 있습니다.");
        }
        
        // 맵 생성 (타일맵 갱신) - regenerateMap이 true일 때만 생성
        if (regenerateMap && currentTheme != null && mapManager != null)
        {
            mapManager.GenerateMap(currentTheme);
        }
        
        // 적 스폰 코루틴 시작
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnStageEnemies(currentTheme, stage, enemiesToSpawn));
        
        OnStageStart?.Invoke(stage); // 이벤트 발생
        Debug.Log($"[DungeonSystem] 스테이지 {stage} 시작 (목표 {totalGoalCount}, 추가 스폰 {enemiesToSpawn})");
    }
    
    /// <summary>
    /// 스테이지의 적들을 일정 간격으로 스폰하는 코루틴
    /// </summary>
    private IEnumerator SpawnStageEnemies(StageThemeData theme, int stage, int count)
    {
        Debug.Log($"[DungeonSystem] Spawning loop started. Count: {count}, TimeScale: {Time.timeScale}");
        for (int i = 0; i < count; i++)
        {
            if (!isDungeonActive) 
            {
                Debug.Log("[DungeonSystem] Spawning stopped because dungeon is inactive.");
                yield break; // 던전 중지 시 스폰 중단
            }
            
            try
            {
                Debug.Log($"[DungeonSystem] Spawning enemy {i + 1}/{count}");
                SpawnEnemy(theme, stage);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DungeonSystem] Error spawning enemy {i}: {e}");
            }
            
            yield return new WaitForSeconds(0.5f); // 스폰 간격 (0.5초)
        }
        Debug.Log("[DungeonSystem] Spawning loop finished.");
        spawnCoroutine = null;
    }
    
    /// <summary>
    /// 개별 적을 스폰하고 설정합니다.
    /// </summary>
    private void SpawnEnemy(StageThemeData theme, int stage)
    {
        if (enemySpawner == null) return;

        Vector3 spawnPos = enemySpawner.GetRandomSpawnPosition();
        
        // 적 데이터 생성 (레벨에 따른 스탯 계산 포함)
        EnemyData data = CreateEnemyDataForStage(theme, stage);
        
        // 적 생성 (풀링 사용)
        Enemy enemy = enemySpawner.SpawnEnemy(data, spawnPos);
        
        // 적이 성공적으로 생성되었다면 관리 목록에 추가하고 이벤트 구독
        if (enemy != null)
        {
            enemy.OnEnemyDefeated += OnEnemyDefeated;
            activeEnemies.Add(enemy);
            Debug.Log($"[DungeonSystem] Enemy spawned. Active: {activeEnemies.Count}, Remaining: {remainingEnemiesInStage}");
        }
        else
        {
            Debug.LogWarning($"[DungeonSystem] 적 스폰 실패! 진행을 위해 남은 적 수를 감소시킵니다. (Stage: {stage})");
            remainingEnemiesInStage--;
            CheckStageClear();
        }
    }
    
    /// <summary>
    /// 적이 처치되었을 때 호출되는 콜백
    /// </summary>
    private void OnEnemyDefeated(Enemy enemy)
    {
        if (enemy == null) return;

        enemy.OnEnemyDefeated -= OnEnemyDefeated; // 중복 호출 방지를 위해 구독 해제
        activeEnemies.Remove(enemy);
        remainingEnemiesInStage--;
        
        Debug.Log($"[DungeonSystem] Enemy defeated. Active: {activeEnemies.Count}, Remaining: {remainingEnemiesInStage}");
        
        CheckStageClear();
    }

    private void CheckStageClear()
    {
        // 목표 적을 모두 처치했으면 스테이지 클리어
        if (remainingEnemiesInStage <= 0)
        {
            OnStageClear?.Invoke(currentStage);
            Debug.Log($"[DungeonSystem] 스테이지 {currentStage} 클리어!");

            // 다음 스테이지 시작
            NextStage();
        }
    }

    private void NextStage()
    {
        if (isDungeonActive)
        {
            // 다음 스테이지는 항상 풀 스폰 및 맵 재생성
            StartStage(currentStage + 1, enemiesPerStage, enemiesPerStage, true);
            
            // 다음 스테이지 시작 시 중앙에 이펙트 재생
            if (respawnEffectPrefab != null)
            {
                Instantiate(respawnEffectPrefab, Vector3.zero, Quaternion.identity);
            }

            // 플레이어와 동료를 중앙으로 이동
            if (playerController != null)
            {
                playerController.transform.position = Vector3.zero;
            }

            if (CompanionSystem.Instance != null && CompanionSystem.Instance.HasActiveCompanion())
            {
                var companion = CompanionSystem.Instance.GetActiveCompanion();
                if (companion != null)
                {
                    companion.transform.position = Vector3.zero;
                }
            }
        }
    }
    
    public void HandlePlayerDefeated()
    {
        totalDefeats++;
        StopDungeon(false); // 적을 제거하지 않고 던전 중지 (일시 정지 개념)
        OnPlayerDefeated?.Invoke();
        
        // 1초 후 플레이어 부활
        StartCoroutine(RespawnPlayerRoutine(1f));
    }

    private IEnumerator RespawnPlayerRoutine(float delay)
    {
        Debug.Log($"[DungeonSystem] Player will respawn in {delay} seconds...");
        yield return new WaitForSeconds(delay);
        
        // 플레이어 부활 처리
        if (playerStats != null)
        {
            playerStats.Heal(playerStats.maxHealth); // 체력 회복
            
            // 플레이어 컨트롤러 초기화 (애니메이션 등)
            if (playerController != null)
            {
                // 위치를 중앙으로 이동
                playerController.transform.position = Vector3.zero;

                // 스프라이트 투명도 복구
                var spriteRenderer = playerController.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                    spriteRenderer.DOFade(1f, 0.5f);
                }
                
                // 자동 전투 재개
                playerController.SetAutoMode(true);
                
                // 리스폰 이펙트 (중앙)
                if (respawnEffectPrefab != null)
                {
                    Instantiate(respawnEffectPrefab, Vector3.zero, Quaternion.identity);
                }
            }
        }
        // 동료도 중앙으로 이동 (CompanionSystem이 있다면)
        if (CompanionSystem.Instance != null && CompanionSystem.Instance.HasActiveCompanion())
        {
            var companion = CompanionSystem.Instance.GetActiveCompanion();
            if (companion != null)
            {
                // 플레이어와 동일한 위치
                companion.transform.position = Vector3.zero;
            }
        }
        
        // 던전 재개
        StartDungeon();
        Debug.Log("[DungeonSystem] Player respawned!");
    }
    
    public void ToggleDungeon()
    {
        if (isDungeonActive)
        {
            StopDungeon();
        }
        else
        {
            StartDungeon();
        }
    }
    
    // 외부에서 적을 수동으로 추가할 때 사용
    public void AddEnemyToList(Enemy enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }
    
    public void RemoveEnemyFromList(Enemy enemy)
    {
        if (enemy != null && activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }
    
    // 죽은 적 정리 (Update에서 호출)
    private void CleanupDeadEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null || !activeEnemies[i].isAlive)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 스테이지 레벨에 맞는 적 데이터를 생성합니다.
    /// </summary>
    private EnemyData CreateEnemyDataForStage(StageThemeData theme, int stage)
    {
        EnemyData data = ScriptableObject.CreateInstance<EnemyData>();
        data.level = stage;
        
        // 테마에 설정된 템플릿 중 하나를 랜덤 선택
        EnemyData template = null;
        if (theme != null && theme.enemyTemplates != null && theme.enemyTemplates.Count > 0)
        {
            template = theme.enemyTemplates[Random.Range(0, theme.enemyTemplates.Count)];
        }
        
        if (template != null)
        {
            // 템플릿 데이터 복사
            data.characterName = template.characterName;
            data.sprite = template.sprite;
            
            // 기본 스탯 복사
            data.baseMaxHealth = template.baseMaxHealth;
            data.baseAttackPower = template.baseAttackPower;
            data.baseDefense = template.baseDefense;
            data.baseAttackSpeed = template.baseAttackSpeed;
            data.baseCritChance = template.baseCritChance;
            data.baseCritDamage = template.baseCritDamage;
            
            // 성장 계수 복사
            data.growthHealth = template.growthHealth;
            data.growthAttack = template.growthAttack;
            data.growthDefense = template.growthDefense;
            data.growthAttackSpeed = template.growthAttackSpeed;
            data.growthCritChance = template.growthCritChance;
            data.growthCritDamage = template.growthCritDamage;
            
            // 기타 설정
            data.moveSpeed = template.moveSpeed;
            data.attackRange = template.attackRange;
            data.detectionRange = template.detectionRange;
            
            // 보상 계산 (레벨 비례 - 지수 증가)
            // 골드: 기본값 * (1.08 ^ (스테이지-1)) + (스테이지 * 10)
            float goldMultiplier = Mathf.Pow(1.08f, stage - 1);
            data.goldReward = Mathf.RoundToInt(template.goldReward * goldMultiplier) + (stage * 10);
            
            // 경험치: 기본값 * (1.08 ^ (스테이지-1)) + (스테이지 * 5)
            float expMultiplier = Mathf.Pow(1.08f, stage - 1);
            data.experienceReward = Mathf.RoundToInt(template.experienceReward * expMultiplier) + (stage * 5);
        }
        else
        {
            // 템플릿이 없으면 기본값 사용
            SetDefaultEnemyData(data, stage);
        }
        
        return data;
    }
    
    private void SetDefaultEnemyData(EnemyData data, int level)
    {
        data.characterName = $"몬스터 Lv.{level}";
        data.baseMaxHealth = 50;
        data.baseAttackPower = 8;
        data.baseDefense = 2;
        data.baseAttackSpeed = 0.8f;
        
        data.growthHealth = 1.1f;
        data.growthAttack = 1.05f;
        data.growthDefense = 1.05f;
        
        data.experienceReward = 10 + (level * 5);
        data.goldReward = 5 + (level * 2);
        data.attackRange = 1.5f;
        data.moveSpeed = 3f;
        data.detectionRange = 10f;
    }
}