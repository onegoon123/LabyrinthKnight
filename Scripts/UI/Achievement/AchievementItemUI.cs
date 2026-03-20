using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementItemUI : MonoBehaviour
{
    [Header("업적 아이템 UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI rewardText;
    public Slider progressBar;
    public Image completedIcon;
    public Image backgroundImage;
    
    [Header("색상 설정")]
    public Color normalColor = Color.white;
    public Color completedColor = Color.green;
    
    private Achievement achievement;
    
    public void SetupAchievement(Achievement achievement)
    {
        this.achievement = achievement;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (achievement == null) return;
        
        // 텍스트 설정
        if (titleText != null)
            titleText.text = achievement.title;
        
        if (descriptionText != null)
            descriptionText.text = achievement.description;
        
        if (progressText != null)
            progressText.text = $"{achievement.progress} / {achievement.target}";
        
        if (rewardText != null)
            rewardText.text = achievement.reward;
        
        // 진행률 바 설정
        if (progressBar != null)
        {
            progressBar.value = (float)achievement.progress / achievement.target;
        }
        
        // 완료 상태 표시
        if (completedIcon != null)
            completedIcon.gameObject.SetActive(achievement.isCompleted);
        
        // 배경 색상 설정
        if (backgroundImage != null)
        {
            backgroundImage.color = achievement.isCompleted ? completedColor : normalColor;
        }
    }
}
