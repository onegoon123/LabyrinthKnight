using UnityEngine;
using TMPro;

[System.Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string description;
    public bool isCompleted;
    public int progress;
    public int target;
    public string reward;
}

public class AchievementsPanel : MonoBehaviour
{
    [Header("업적 UI")]
    public Transform achievementListParent;
    public GameObject achievementItemPrefab;
    public TextMeshProUGUI totalAchievementsText;
    public TextMeshProUGUI completedAchievementsText;
    
    [Header("업적 데이터")]
    public Achievement[] achievements;
    
    private int totalAchievements;
    private int completedAchievements;
    
    private void Start()
    {
        InitializeAchievements();
        UpdateUI();
        CreateAchievementItems();
    }
    
    private void InitializeAchievements()
    {
        // 기본 업적들 초기화
        if (achievements == null || achievements.Length == 0)
        {
            achievements = new Achievement[]
            {
                new Achievement
                {
                    id = "first_kill",
                    title = "첫 번째 처치",
                    description = "적을 1마리 처치하세요",
                    isCompleted = false,
                    progress = 0,
                    target = 1,
                    reward = "골드 100"
                },
                new Achievement
                {
                    id = "killer_10",
                    title = "킬러",
                    description = "적을 10마리 처치하세요",
                    isCompleted = false,
                    progress = 0,
                    target = 10,
                    reward = "골드 500"
                },
                new Achievement
                {
                    id = "killer_100",
                    title = "전투의 달인",
                    description = "적을 100마리 처치하세요",
                    isCompleted = false,
                    progress = 0,
                    target = 100,
                    reward = "골드 2000"
                },
                new Achievement
                {
                    id = "level_10",
                    title = "성장",
                    description = "레벨 10에 도달하세요",
                    isCompleted = false,
                    progress = 0,
                    target = 10,
                    reward = "골드 1000"
                },
                new Achievement
                {
                    id = "rich_1000",
                    title = "부자",
                    description = "골드 1000개를 모으세요",
                    isCompleted = false,
                    progress = 0,
                    target = 1000,
                    reward = "골드 500"
                }
            };
        }
        
        totalAchievements = achievements.Length;
        LoadAchievementProgress();
    }
    
    private void CreateAchievementItems()
    {
        if (achievementListParent == null || achievementItemPrefab == null) return;
        
        // 기존 아이템들 제거
        foreach (Transform child in achievementListParent)
        {
            Destroy(child.gameObject);
        }
        
        // 업적 아이템들 생성
        foreach (Achievement achievement in achievements)
        {
            GameObject item = Instantiate(achievementItemPrefab, achievementListParent);
            AchievementItemUI itemUI = item.GetComponent<AchievementItemUI>();
            
            if (itemUI != null)
            {
                itemUI.SetupAchievement(achievement);
            }
        }
    }
    
    private void UpdateUI()
    {
        completedAchievements = 0;
        
        foreach (Achievement achievement in achievements)
        {
            if (achievement.isCompleted)
            {
                completedAchievements++;
            }
        }
        
        if (totalAchievementsText != null)
            totalAchievementsText.text = $"총 업적: {totalAchievements}";
        
        if (completedAchievementsText != null)
            completedAchievementsText.text = $"완료: {completedAchievements}";
    }
    
    public void UpdateAchievementProgress(string achievementId, int progress)
    {
        foreach (Achievement achievement in achievements)
        {
            if (achievement.id == achievementId)
            {
                achievement.progress = progress;
                
                if (achievement.progress >= achievement.target && !achievement.isCompleted)
                {
                    achievement.isCompleted = true;
                    CompleteAchievement(achievement);
                }
                
                break;
            }
        }
        
        SaveAchievementProgress();
        UpdateUI();
    }
    
    private void CompleteAchievement(Achievement achievement)
    {
        Debug.Log($"업적 완료: {achievement.title} - {achievement.reward}");
        // 보상 지급 로직 추가
    }
    
    private void LoadAchievementProgress()
    {
        foreach (Achievement achievement in achievements)
        {
            achievement.progress = PlayerPrefs.GetInt($"Achievement_{achievement.id}_Progress", 0);
            achievement.isCompleted = PlayerPrefs.GetInt($"Achievement_{achievement.id}_Completed", 0) == 1;
        }
    }
    
    private void SaveAchievementProgress()
    {
        foreach (Achievement achievement in achievements)
        {
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_Progress", achievement.progress);
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_Completed", achievement.isCompleted ? 1 : 0);
        }
        PlayerPrefs.Save();
    }
}
