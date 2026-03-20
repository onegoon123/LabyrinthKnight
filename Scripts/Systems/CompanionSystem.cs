using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 동료 시스템을 관리하는 매니저
/// 한 번에 하나의 동료만 활성화할 수 있습니다.
/// </summary>
public class CompanionSystem : MonoBehaviour
{
    public static CompanionSystem Instance { get; private set; }
    
    [Header("동료 설정")]
    public GameObject companionPrefab; // 동료 프리팹
    public List<CompanionData> allCompanions; // 모든 동료 데이터
    
    [Header("현재 상태")]
    private CompanionController activeCompanion;
    private string currentCompanionId = ""; // 현재 선택된 동료 ID
    private Transform playerTransform;
    
    private void Awake()
    {
        // Singleton 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    // 동료 상태 관리 (해금, 호감도 등)
    private Dictionary<string, CompanionSaveData> companionStates = new Dictionary<string, CompanionSaveData>();

    // UI 이벤트를 위한 델리게이트
    public event System.Action<CompanionData> OnCompanionChanged;
    public event System.Action<string> OnCompanionUnlocked;
    public event System.Action<string, int> OnAffinityChanged;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeReferences();
        
        // 초기 상태 설정 (로드된 데이터가 없을 경우를 대비)
        InitializeCompanionStates();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeReferences();
        
        // 스토리 씬 등 전투가 아닌 씬에서는 동료 소환하지 않음
        // "Story"가 포함된 씬 이름이나 특정 비전투 씬 이름 체크
        string sceneName = scene.name.ToLower();
        if (sceneName.Contains("story") || sceneName == "title" || sceneName == "lobby") 
        {
            return;
        }
        
        // 씬 로드 시 활성화된 동료가 있다면 재소환
        if (!string.IsNullOrEmpty(currentCompanionId))
        {
            SpawnCompanionById(currentCompanionId);
        }
    }

    private void InitializeReferences()
    {
        // 플레이어 찾기 및 이벤트 연결
        var playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerTransform = playerStats.transform;
            playerStats.OnLevelUp -= HandlePlayerLevelUp; // 중복 방지
            playerStats.OnLevelUp += HandlePlayerLevelUp;
        }

        // 던전 시스템 이벤트 구독
        var dungeonSystem = FindFirstObjectByType<DungeonSystem>();
        if (dungeonSystem != null)
        {
            dungeonSystem.OnStageClear -= HandleStageClear; // 중복 방지
            dungeonSystem.OnStageClear += HandleStageClear;
        }
    }

    private float affinityTimer;
    public float affinityInterval = 20f; // 20초마다 호감도 상승

    private void Update()
    {
        // 활성화된 동료가 있을 때 시간 경과에 따른 호감도 상승
        if (activeCompanion != null && activeCompanion.isAlive && activeCompanion.companionData != null)
        {
            affinityTimer += Time.deltaTime;
            if (affinityTimer >= affinityInterval)
            {
                affinityTimer = 0f;
                IncreaseAffinity(activeCompanion.companionData.companionId, 1);
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        var playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.OnLevelUp -= HandlePlayerLevelUp;
        }

        var dungeonSystem = FindFirstObjectByType<DungeonSystem>();
        if (dungeonSystem != null)
        {
            dungeonSystem.OnStageClear -= HandleStageClear;
        }
    }

    private void InitializeCompanionStates()
    {
        if (allCompanions == null) return;

        foreach (var data in allCompanions)
        {
            if (data == null) continue;

            // 이미 데이터가 있다면 덮어쓰지 않음 (로드된 데이터 보존)
            if (!companionStates.ContainsKey(data.companionId))
            {
                companionStates[data.companionId] = new CompanionSaveData
                {
                    companionId = data.companionId,
                    isUnlocked = false, // 기본적으로 잠김
                    currentAffinity = 0,
                    isStoryUnlocked = false
                };
            }
        }
    }

    private void HandleStageClear(int stage)
    {
        // 스테이지 클리어 시 호감도 상승 로직은 시간 기반으로 변경되어 삭제됨.
        /*
        // 활성화된 동료가 있으면 호감도 상승
        if (activeCompanion != null && activeCompanion.isAlive && activeCompanion.companionData != null)
        {
            IncreaseAffinity(activeCompanion.companionData.companionId, 1);
        }
        */
    }

    public void IncreaseAffinity(string companionId, int amount)
    {
        if (companionStates.TryGetValue(companionId, out var state))
        {
            state.currentAffinity += amount;
            // 최대치 제한 (선택 사항, 사용자 요청에 따라 100이 최대라면 여기서 Clamp 가능)
            if (state.currentAffinity > 100) state.currentAffinity = 100;

            Debug.Log($"[CompanionSystem] {companionId} affinity increased to {state.currentAffinity}");
            OnAffinityChanged?.Invoke(companionId, state.currentAffinity);
            
        }
    }

    /// <summary>
    /// [치트용] 호감도를 특정 값으로 설정합니다.
    /// </summary>
    public void SetAffinity(string companionId, int value)
    {
        if (companionStates.TryGetValue(companionId, out var state))
        {
            state.currentAffinity = Mathf.Clamp(value, 0, 100);
            Debug.Log($"[CompanionSystem] {companionId} affinity set to {state.currentAffinity}");
            OnAffinityChanged?.Invoke(companionId, state.currentAffinity);
        }
    }
    public bool UnlockCompanion(string companionId, bool force = false)
    {
        if (companionStates.TryGetValue(companionId, out var state))
        {
            if (state.isUnlocked) return true; // 이미 해금됨

            if (force)
            {
                state.isUnlocked = true;
                OnCompanionUnlocked?.Invoke(companionId);
                return true;
            }

            // 비용 체크 및 차감 로직
            var data = GetCompanionDataById(companionId);
            var playerStats = FindFirstObjectByType<PlayerStats>();
            
            if (data != null && playerStats != null)
            {
                if (playerStats.gold >= data.unlockCost)
                {
                    playerStats.gold -= data.unlockCost;
                    state.isUnlocked = true;
                    Debug.Log($"[CompanionSystem] Unlocked {companionId} for {data.unlockCost} gold.");
                    OnCompanionUnlocked?.Invoke(companionId);
                    return true;
                }
                else
                {
                    Debug.LogWarning("[CompanionSystem] Not enough gold to unlock companion.");
                }
            }
        }
        return false;
    }

    /// <summary>
    /// [치트용] 모든 동료를 해금하고 호감도를 최대로 설정합니다.
    /// </summary>
    public void UnlockAllCompanions()
    {
        if (allCompanions == null) return;

        foreach (var data in allCompanions)
        {
            if (data == null) continue;

            // 강제 해금
            UnlockCompanion(data.companionId, true);
            
            // 호감도 최대 설정
            SetAffinity(data.companionId, 100);
        }
        Debug.Log("[CompanionSystem] All companions unlocked and affinity set to max (Cheat).");
    }

    public bool IsCompanionUnlocked(string companionId)
    {
        if (companionStates.TryGetValue(companionId, out var state))
        {
            return state.isUnlocked;
        }
        return false;
    }
    
    public int GetCompanionAffinity(string companionId)
    {
        if (companionStates.TryGetValue(companionId, out var state))
        {
            return state.currentAffinity;
        }
        return 0;
    }
    
    public bool IsStoryUnlocked(string companionId)
    {
        if (companionStates.TryGetValue(companionId, out var state))
        {
            return state.isStoryUnlocked;
        }
        return false;
    }

    // 저장용 데이터 반환
    public List<CompanionSaveData> GetCompanionSaveDataList()
    {
        return new List<CompanionSaveData>(companionStates.Values);
    }

    // 데이터 로드
    public void LoadCompanionSaveData(List<CompanionSaveData> loadedData)
    {
        if (loadedData == null) return;

        foreach (var data in loadedData)
        {
            if (data != null)
            {
                companionStates[data.companionId] = data;
            }
        }
    }

    private void HandlePlayerLevelUp()
    {
        if (activeCompanion != null && activeCompanion.isAlive)
        {
            var playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats != null)
            {
                Debug.Log($"[CompanionSystem] Updating companion stats to level {playerStats.level}");
                activeCompanion.UpdateStats(playerStats.level);
            }
        }
    }
    
    /// <summary>
    /// 동료를 활성화합니다.
    /// </summary>
    /// <param name="companionData">동료 데이터</param>
    public void SetActiveCompanion(CompanionData companionData)
    {
        if (companionData == null)
        {
            Debug.LogWarning("[CompanionSystem] Companion data is null!");
            return;
        }
        
        // 해금 여부 확인
        if (!IsCompanionUnlocked(companionData.companionId))
        {
            Debug.LogWarning($"[CompanionSystem] Companion {companionData.characterName} is locked!");
            return;
        }
        
        // 기존 동료 제거 (참조된 동료)
        RemoveCompanion();
        
        // [안전장치] 씬에 남아있는 모든 동료 컨트롤러 검색 후 제거 (중복 방지)
        var existingCompanions = FindObjectsByType<CompanionController>(FindObjectsSortMode.None);
        foreach (var existing in existingCompanions)
        {
            if (existing != null)
            {
                Debug.LogWarning($"[CompanionSystem] Found leftover companion {existing.name}, destroying...");
                Destroy(existing.gameObject);
            }
        }
        
        // 새 동료 생성
        if (companionPrefab == null)
        {
            Debug.LogError("[CompanionSystem] Companion prefab is not assigned!");
            return;
        }
        
        // 플레이어 근처에 생성
        Vector3 spawnPosition = playerTransform != null 
            ? playerTransform.position + Vector3.right * 2f 
            : Vector3.zero;
        
        GameObject companionObj = Instantiate(companionPrefab, spawnPosition, Quaternion.identity);
        activeCompanion = companionObj.GetComponent<CompanionController>();
        
        if (activeCompanion != null)
        {
            // 플레이어 레벨 가져오기
            int playerLevel = 1;
            var playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats != null)
            {
                playerLevel = playerStats.level;
            }

            activeCompanion.Initialize(companionData, playerLevel);
            activeCompanion.OnCompanionDefeated += HandleCompanionDefeated;
            currentCompanionId = companionData.companionId; // ID 저장
            Debug.Log($"[CompanionSystem] Spawned companion: {companionData.characterName} (Level: {playerLevel})");
            
            OnCompanionChanged?.Invoke(companionData);
        }
        else
        {
            Debug.LogError("[CompanionSystem] Companion prefab does not have CompanionController component!");
            Destroy(companionObj);
        }
    }
    
    /// <summary>
    /// ID로 동료를 소환합니다 (저장 파일에서 로드할 때 사용).
    /// </summary>
    /// <param name="companionId">동료 ID</param>
    public void SpawnCompanionById(string companionId)
    {
        if (string.IsNullOrEmpty(companionId))
        {
            Debug.Log("[CompanionSystem] No companion ID provided.");
            return;
        }
        
        CompanionData data = GetCompanionDataById(companionId);
        if (data != null)
        {
            // 로드 시에는 해금 여부 체크를 건너뛰거나, 로드된 상태를 신뢰해야 함
            // 하지만 SetActiveCompanion에서 체크하므로, 상태가 올바르게 로드되어 있어야 함
            SetActiveCompanion(data);
        }
        else
        {
            Debug.LogWarning($"[CompanionSystem] Companion with ID '{companionId}' not found!");
        }
    }
    
    /// <summary>
    /// ID로 CompanionData를 찾습니다.
    /// </summary>
    private CompanionData GetCompanionDataById(string companionId)
    {
        if (allCompanions == null) return null;
        
        foreach (var companion in allCompanions)
        {
            if (companion != null && companion.companionId == companionId)
            {
                return companion;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 현재 동료를 제거합니다.
    /// </summary>
    public void RemoveCompanion()
    {
        // 1. 현재 참조하고 있는 동료 제거
        if (activeCompanion != null)
        {
            activeCompanion.OnCompanionDefeated -= HandleCompanionDefeated;
            if (activeCompanion.gameObject != null)
            {
                Destroy(activeCompanion.gameObject);
            }
            activeCompanion = null;
        }

        // 2. [강력한 안전장치] 씬에 남아있는 모든 동료 오브젝트 강제 제거
        // 참조가 끊겼거나, 중복 생성된 좀비 동료들을 모두 정리합니다.
        var allCompanions = FindObjectsByType<CompanionController>(FindObjectsSortMode.None);
        foreach (var companion in allCompanions)
        {
            if (companion != null && companion.gameObject != null)
            {
                // 이미 Destroy 예약된 객체인지 확인은 어렵지만, 다시 호출해도 안전함
                Destroy(companion.gameObject);
            }
        }

        // 3. 상태 초기화
        currentCompanionId = ""; // ID 초기화
        Debug.Log("[CompanionSystem] All companions removed");
        
        OnCompanionChanged?.Invoke(null);
    }
    
    /// <summary>
    /// 현재 활성화된 동료를 반환합니다.
    /// </summary>
    public CompanionController GetActiveCompanion()
    {
        return activeCompanion;
    }
    
    /// <summary>
    /// 동료가 활성화되어 있는지 확인합니다.
    /// </summary>
    public bool HasActiveCompanion()
    {
        return activeCompanion != null && activeCompanion.isAlive;
    }
    
    /// <summary>
    /// 현재 선택된 동료 ID를 반환합니다 (저장용).
    /// </summary>
    public string GetCurrentCompanionId()
    {
        return currentCompanionId;
    }
    
    /// <summary>
    /// 모든 동료 목록을 반환합니다.
    /// </summary>
    public List<CompanionData> GetAllCompanions()
    {
        return allCompanions;
    }
    
    private void HandleCompanionDefeated(CompanionController companion)
    {
        Debug.Log($"[CompanionSystem] Companion defeated: {companion.GetName()}");
        
        // 사망한 동료의 데이터 저장
        CompanionData deadCompanionData = companion.companionData;
        
        // 동료 제거
        RemoveCompanion();
        
        // 10초 후 리스폰
        if (deadCompanionData != null)
        {
            StartCoroutine(RespawnCompanionRoutine(deadCompanionData, 10f));
        }
    }
    
    [Header("이펙트")]
    public GameObject respawnEffectPrefab;

    private System.Collections.IEnumerator RespawnCompanionRoutine(CompanionData data, float delay)
    {
        Debug.Log($"[CompanionSystem] Companion will respawn in {delay} seconds...");
        yield return new WaitForSeconds(delay);
        
        SetActiveCompanion(data);
        
        if (activeCompanion != null && respawnEffectPrefab != null)
        {
            Instantiate(respawnEffectPrefab, activeCompanion.transform.position, Quaternion.identity);
        }
        
        Debug.Log($"[CompanionSystem] Companion respawned!");
    }
}
