using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("플레이어 스탯 UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI goldText;
    public Slider expBar;
    
    [Header("던전 UI")]
    public TextMeshProUGUI stageText;
    
    [Header("시스템 참조")]
    public PlayerStats playerStats;
    public DungeonSystem dungeonSystem;
    public DungeonManager dungeonManager;
    public NavigationController navigationController;
    
    private void Start()
    {
        // Inspector에서 참조가 설정되지 않은 경우에만 자동으로 찾기
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
            
        if (dungeonSystem == null)
            dungeonSystem = FindFirstObjectByType<DungeonSystem>();
            
        if (dungeonManager == null)
            dungeonManager = FindFirstObjectByType<DungeonManager>();

        if (navigationController == null)
            navigationController = FindFirstObjectByType<NavigationController>();
        
        if (playerStats != null)
        {
            playerStats.OnStatsChanged += UpdateUI;
            playerStats.OnLevelUp += UpdateUI;
        }
        
        if (dungeonSystem != null)
        {
            dungeonSystem.OnStageStart += UpdateStageUI;
            UpdateStageUI(dungeonSystem.currentStage);
        }
        
        if (dungeonManager != null)
        {
            dungeonManager.OnDungeonEnter += UpdateDungeonNameUI;
            dungeonManager.OnDungeonExit += UpdateMainStageUI;
        }
        
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateUI;
            playerStats.OnLevelUp -= UpdateUI;
        }
        
        if (dungeonSystem != null)
        {
            dungeonSystem.OnStageStart -= UpdateStageUI;
        }
        
        if (dungeonManager != null)
        {
            dungeonManager.OnDungeonEnter -= UpdateDungeonNameUI;
            dungeonManager.OnDungeonExit -= UpdateMainStageUI;
        }
    }
    
    private void UpdateUI()
    {
        if (playerStats == null) return;
        
        // 기본 정보 업데이트
        if (levelText != null)
            levelText.text = $"{playerStats.level}";
        
        if (expText != null)
            expText.text = $"{playerStats.experience} / {playerStats.experienceToNextLevel}";
        
        if (goldText != null)
            goldText.text = playerStats.gold.ToString();
        
        if (expBar != null)
        {
            float expProgress = (float)playerStats.experience / playerStats.experienceToNextLevel;
            expBar.value = expProgress;
        }
    }

    private void UpdateStageUI(int stage)
    {
        if (stageText != null)
        {
            stageText.text = $"STAGE {stage}";
        }
    }
    
    private void UpdateDungeonNameUI(DungeonData data)
    {
        if (stageText != null)
        {
            stageText.text = data.dungeonName;
        }
        
        // 던전 입장 시 하단 네비게이션 선택 해제
        if (navigationController != null)
        {
            navigationController.DeselectAll();
        }
    }
    
    private void UpdateMainStageUI(bool isClear)
    {
        if (dungeonSystem != null)
        {
            UpdateStageUI(dungeonSystem.currentStage);
        }
    }
}
