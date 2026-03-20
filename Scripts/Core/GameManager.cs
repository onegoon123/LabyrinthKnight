using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// [게임 매니저]
/// 게임의 전반적인 상태(시작, 일시정지, 종료 등)를 관리하는 싱글톤 클래스입니다.
/// 주요 시스템(플레이어, 던전, UI, 저장)에 대한 참조를 중앙에서 관리합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("게임 설정")]
    public bool isGamePaused = false; // 게임 일시정지 여부
    public float gameSpeed = 1f; // 게임 진행 속도 (기본 1.0)
    
    [Header("시스템 참조")]
    public PlayerStats playerStats;
    public DungeonSystem dungeonSystem; // 방치형 던전 시스템
    public GameUI gameUI;
    public GameSaveSystem saveSystem;
    public DungeonManager dungeonManager; // 활성 던전 매니저
    public EquipmentCraftingSystem equipmentCraftingSystem; // 장비 제작 시스템
    
    [Header("게임 상태")]
    public bool isGameStarted = false;
    public bool isGameOver = false;

    [Header("치트 설정")]
    public bool enableCheats = false;
    public bool isTestMode = false; // 테스트 모드 (닉네임 "테스트"로 활성화)
    public int cheatGoldAmount = 10000;
    
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }
    
    // 글로벌 이벤트
    public event System.Action OnGameStart;
    public event System.Action OnGamePause;
    public event System.Action OnGameResume;
    public event System.Action OnGameOver;
    
    private void Awake()
    {
        // 싱글톤 패턴 구현: 중복 생성 방지 및 씬 전환 시 유지
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
    
    private void Start()
    {
        InitializeGame();
        Application.targetFrameRate = 60;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] Scene Loaded: {scene.name}");
        isGameStarted = false;

        // 참조 초기화 (InitializeGame에서 다시 찾도록 유도)
        playerStats = null;
        dungeonSystem = null;
        gameUI = null;
        dungeonManager = null;
        equipmentCraftingSystem = null;
        
        // saveSystem은 GameManager와 같은 씬에 있거나 DontDestroyOnLoad라고 가정하지만,
        // 만약 씬마다 새로 생긴다면 이것도 null로.
        // 여기선 안전하게 그대로 두거나 null (GameSaveSystem이 DontDestroyOnLoad면 유지해야 함)
        // 사용자의 saveSystem은 씬 객체일 확률이 높음 (GameManager처럼).
        
        InitializeGame();
    }
    
    /// <summary>
    /// 게임 초기화 및 주요 컴포넌트 연결
    /// </summary>
    private void InitializeGame()
    {
        // 컴포넌트 자동 찾기 (Inspector 연결 누락 대비)
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
        
        if (dungeonSystem == null)
            dungeonSystem = FindFirstObjectByType<DungeonSystem>();
        
        if (gameUI == null)
            gameUI = FindFirstObjectByType<GameUI>();
        
        if (saveSystem == null)
            saveSystem = FindFirstObjectByType<GameSaveSystem>();

        if (dungeonManager == null)
        {
            dungeonManager = FindFirstObjectByType<DungeonManager>();
            
            // 없으면 자동 생성하여 기능 보장
            if (dungeonManager == null)
            {
                GameObject dmObj = new GameObject("DungeonManager");
                dungeonManager = dmObj.AddComponent<DungeonManager>();
                Debug.Log("[GameManager] DungeonManager를 찾을 수 없어 자동으로 생성했습니다.");
            }
        }

        if (equipmentCraftingSystem == null)
            equipmentCraftingSystem = FindFirstObjectByType<EquipmentCraftingSystem>();
        
        // 에디터에서만 디버그 콘솔 추가
        if (GetComponent<DebugConsole>() == null)
        {
            gameObject.AddComponent<DebugConsole>();
        }

        // 테스트 모드 확인 (PlayerPrefs)
        if (PlayerPrefs.GetInt("IsTestMode", 0) == 1)
        {
            isTestMode = true;
            Debug.Log("[GameManager] Test Mode detected via PlayerPrefs.");
        }

        // 게임 시작
        StartGame();
    }
    
    public void StartGame()
    {
        if (isGameStarted) return;
        
        isGameStarted = true;
        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f; // 시간 흐름 초기화
        
        // 테스트 모드 치트 적용 (데이터 로드 후 적용을 위해 지연)
        if (isTestMode)
        {
            StartCoroutine(ApplyTestModeCheatsDelayed());
        }
        
        // 던전(방치 모드) 시작
        if (dungeonSystem != null)
        {
            dungeonSystem.StartDungeon();
        }
        
        OnGameStart?.Invoke();
        Debug.Log("게임 시작!");
    }

    private System.Collections.IEnumerator ApplyTestModeCheatsDelayed()
    {
        // GameSaveSystem의 로드가 완료될 때까지 대기 (안전하게 0.5초 대기)
        yield return new WaitForSeconds(0.5f);
        
        ApplyTestModeCheats();
    }

    private void ApplyTestModeCheats()
    {
        Debug.Log("[GameManager] Applying Test Mode Cheats...");

        // 1. 골드 1억 추가
        if (playerStats != null)
        {
            playerStats.gold += 100000000;
            playerStats.NotifyStatsChanged();
            Debug.Log("[Cheat] Gold set to +100,000,000");
        }

        // 2. 아이템 생성 쿨타임 0
        if (equipmentCraftingSystem != null)
        {
            equipmentCraftingSystem.regenInterval = 0f;
            Debug.Log("[Cheat] Item generation cooldown set to 0");
        }

        // 3. 모든 동료 해금 및 호감도 최대
        if (CompanionSystem.Instance != null)
        {
            CompanionSystem.Instance.UnlockAllCompanions();
        }
    }
    
    /// <summary>
    /// 게임을 일시정지합니다. (시간 정지)
    /// </summary>
    public void PauseGame()
    {
        if (isGamePaused || isGameOver) return;
        
        isGamePaused = true;
        Time.timeScale = 0f; // 시간 정지
        
        // 던전 시스템도 중지
        if (dungeonSystem != null)
        {
            dungeonSystem.StopDungeon();
        }
        
        OnGamePause?.Invoke();
        Debug.Log("게임 일시정지");
    }
    
    /// <summary>
    /// 게임을 재개합니다. (시간 흐름 복구)
    /// </summary>
    public void ResumeGame()
    {
        if (!isGamePaused || isGameOver) return;
        
        isGamePaused = false;
        Time.timeScale = gameSpeed; // 설정된 속도로 복구
        
        // 던전 시스템 재개
        if (dungeonSystem != null)
        {
            dungeonSystem.StartDungeon();
        }
        
        OnGameResume?.Invoke();
        Debug.Log("게임 재개");
    }
    
    public void TogglePause()
    {
        if (isGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public void GameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        isGamePaused = true;
        Time.timeScale = 0f;
        
        if (dungeonSystem != null)
        {
            dungeonSystem.StopDungeon();
        }
        
        OnGameOver?.Invoke();
        Debug.Log("게임 오버!");
    }
    
    /// <summary>
    /// 게임을 처음부터 다시 시작합니다. (데이터 초기화 포함)
    /// </summary>
    public void RestartGame()
    {
        // 게임 상태 초기화
        isGameStarted = false;
        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f;
        
        // 저장 파일 삭제 (완전 초기화)
        if (saveSystem != null)
        {
            saveSystem.DeleteSave();
        }
        
        // 현재 씬 재로드 (페이드 효과 사용)
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneFadeManager.Instance.LoadSceneWithFade(currentSceneName);
    }
    
    public void QuitGame()
    {
        // 게임 종료 전 자동 저장
        if (saveSystem != null)
        {
            saveSystem.SaveGame();
        }
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void SetGameSpeed(float speed)
    {
        gameSpeed = Mathf.Clamp(speed, 0.1f, 5f);
        
        if (!isGamePaused)
        {
            Time.timeScale = gameSpeed;
        }
    }
    
    public void SaveGame()
    {
        if (saveSystem != null)
        {
            saveSystem.SaveGame();
        }
    }
    
    public void LoadGame()
    {
        if (saveSystem != null)
        {
            saveSystem.LoadGame();
        }
    }
    
    private void Update()
    {
        // 입력 처리 (단축키)
        
        // ESC 키로 일시정지/재개
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
        
        // F5 키로 저장
        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            SaveGame();
        }
        
        // F9 키로 로드
        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            LoadGame();
        }

        // F10 키로 골드 치트
        if (enableCheats && Keyboard.current.f10Key.wasPressedThisFrame)
        {
            AddCheatGold();
        }
    }

    private void AddCheatGold()
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogWarning("골드 치트를 적용할 PlayerStats를 찾을 수 없습니다.");
                return;
            }
        }

        int amount = Mathf.Max(0, cheatGoldAmount);
        playerStats.gold += amount;
        playerStats.NotifyStatsChanged();
        Debug.Log($"[Cheat] 골드 {amount}이(가) 추가되었습니다. 현재 골드: {playerStats.gold.ToString()}");
    }
}
