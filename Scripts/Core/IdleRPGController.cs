using UnityEngine;

public class IdleRPGController : MonoBehaviour
{
    [Header("게임 설정")]
    public bool enableAutoSave = true;
    public float autoSaveInterval = 30f;
    public bool enableAutoBattle = true;
    public float battleSpeed = 1f;
    
    
    [Header("시스템 참조")]
    public DungeonSystem dungeonSystem;
    public GameSaveSystem saveSystem;
    
    [Header("적 초기 설정")]
    public int startingEnemyLevel = 1;
    public float enemySpawnInterval = 2f;
    public int maxEnemiesOnScreen = 3;
    
    private void Start()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        // 게임 설정만 적용 (다른 시스템들은 Inspector에서 참조하거나 자체적으로 초기화)
        ApplyGameSettings();
    }
    
    
    private void ApplyGameSettings()
    {
        // 던전 시스템 설정
        // 던전 시스템 설정
        if (dungeonSystem != null)
        {
            // dungeonSystem.enemySpawnInterval = enemySpawnInterval; // Removed
            // dungeonSystem.maxEnemiesInDungeon = maxEnemiesOnScreen; // Removed
            dungeonSystem.currentStage = startingEnemyLevel;
        }
        
        // 저장 시스템 설정
        if (saveSystem != null)
        {
            saveSystem.autoSaveInterval = autoSaveInterval;
        }
        
        // 게임 속도 설정
        Time.timeScale = battleSpeed;
    }
    
    public void ResetGame()
    {
        // 게임 매니저를 통해 게임 재시작
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
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
}
