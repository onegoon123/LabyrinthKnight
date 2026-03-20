using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonListItem : MonoBehaviour
{
    [Header("UI 요소")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public Image iconImage;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI expText;
    public Button enterButton; // 복구됨

    private DungeonData dungeonData;
    private System.Action<DungeonData> onEnterCallback;
    
    public void Initialize(DungeonData data, System.Action<DungeonData> onEnter)
    {
        dungeonData = data;
        onEnterCallback = onEnter;
        
        if (nameText != null) nameText.text = data.dungeonName;
        if (levelText != null) levelText.text = $"Lv. {data.recommendedLevel}";
        if (iconImage != null && data.icon != null) iconImage.sprite = data.icon;
        
        // 숫자만 출력 (디자인 유연성 확보)
        if (goldText != null) goldText.text = $"{data.rewards.goldReward:N0}";
        if (expText != null) expText.text = $"{data.rewards.expReward:N0}";
        
        if (enterButton != null)
        {
            enterButton.onClick.RemoveAllListeners();
            enterButton.onClick.AddListener(OnEnterClicked);
        }
    }
    
    private void OnEnterClicked()
    {
        onEnterCallback?.Invoke(dungeonData);
    }
}
