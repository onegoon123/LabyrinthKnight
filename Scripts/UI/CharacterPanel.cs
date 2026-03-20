using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterPanel : MonoBehaviour
{
    [Header("캐릭터 정보 UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI attackSpeedText;
    public TextMeshProUGUI expText;
    
    [Header("스탯 바")]
    public Slider healthBar;
    public Slider expBar;
    
    private PlayerStats playerStats;
    
    private void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        
        if (playerStats != null)
        {
            playerStats.OnStatsChanged += UpdateUI;
            playerStats.OnLevelUp += OnLevelUp;
        }
        
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateUI;
            playerStats.OnLevelUp -= OnLevelUp;
        }
    }
    
    private void UpdateUI()
    {
        if (playerStats == null) return;
        
        // 기본 정보 업데이트
        if (levelText != null)
            levelText.text = $"Lv. {playerStats.level}";
        
        if (healthText != null)
            healthText.text = $"{playerStats.currentHealth} / {playerStats.maxHealth}";
        
        if (attackText != null)
            attackText.text = $"공격력: {playerStats.attackPower}";
        
        if (defenseText != null)
            defenseText.text = $"방어력: {playerStats.defense}";
        
        if (attackSpeedText != null)
            attackSpeedText.text = $"공격속도: {playerStats.attackSpeed:F1}";
        
        if (expText != null)
            expText.text = $"{playerStats.experience} / {playerStats.experienceToNextLevel}";
        
        // 바 업데이트
        if (healthBar != null)
            healthBar.value = playerStats.GetHealthPercentage();
        
        if (expBar != null)
        {
            float expProgress = (float)playerStats.experience / playerStats.experienceToNextLevel;
            expBar.value = expProgress;
        }
    }
    
    private void OnLevelUp()
    {
        UpdateUI();
        Debug.Log("레벨업!");
    }
}
