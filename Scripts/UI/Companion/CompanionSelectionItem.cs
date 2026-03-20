using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 동료 목록의 개별 아이템 UI
/// </summary>
public class CompanionSelectionItem : MonoBehaviour
{
    [Header("UI 요소")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Slider affinitySlider; // 호감도 슬라이더 추가
    public Button selectButton;
    
    private CompanionData companionData;
    private CompanionSelectionUI parentUI;
    private CompanionSystem companionSystem;

    public void Initialize(CompanionData data, CompanionSelectionUI parent)
    {
        companionData = data;
        parentUI = parent;
        companionSystem = FindFirstObjectByType<CompanionSystem>();
        
        // 이벤트 구독
        if (companionSystem != null)
        {
            companionSystem.OnAffinityChanged += HandleAffinityChanged;
        }

        UpdateUI();
        
        // 버튼 이벤트 등록
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }
    }
    
    private void OnDestroy()
    {
        if (companionSystem != null)
        {
            companionSystem.OnAffinityChanged -= HandleAffinityChanged;
        }
    }
    
    private void UpdateUI()
    {
        if (companionData == null) return;

        if (nameText != null)
            nameText.text = companionData.characterName;
        
        if (iconImage != null && companionData.icon != null)
            iconImage.sprite = companionData.icon;

        // 호감도 슬라이더 업데이트 (statsText 제거됨)
        if (affinitySlider != null && companionSystem != null)
        {
            int currentAffinity = companionSystem.GetCompanionAffinity(companionData.companionId);
            affinitySlider.maxValue = 100; // 호감도 최대치 100
            affinitySlider.value = currentAffinity;
        }
    }

    private void HandleAffinityChanged(string id, int newAffinity)
    {
        // 내 ID와 일치할 때만 업데이트
        if (companionData != null && companionData.companionId == id)
        {
            if (affinitySlider != null)
            {
                affinitySlider.value = newAffinity;
            }
        }
    }

    private void OnSelectButtonClicked()
    {
        if (parentUI != null && companionData != null)
        {
            parentUI.SelectCompanion(companionData);
        }
    }
}
