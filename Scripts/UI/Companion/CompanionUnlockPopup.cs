using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompanionUnlockPopup : MonoBehaviour
{
    [Header("UI 요소")]
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI costText;
    
    [Header("버튼")]
    public Button unlockButton;
    public Button cancelButton;

    private CompanionData currentData;
    private CompanionSystem companionSystem;
    private CompanionSelectionUI selectionUI;

    private void Awake()
    {
        companionSystem = FindFirstObjectByType<CompanionSystem>();
        
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(Close);
        }
    }

    public void Open(CompanionData data, CompanionSelectionUI ui)
    {
        currentData = data;
        selectionUI = ui;
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void UpdateUI()
    {
        if (currentData == null) return;

        if (messageText != null)
        {
            messageText.text = $"{currentData.characterName}을(를) 영입하시겠습니까?";
        }

        bool canAfford = false;
        var playerStats = FindFirstObjectByType<PlayerStats>();
        
        if (costText != null)
        {
            costText.text = $"{currentData.unlockCost:N0} G";
            
            // 골드 부족 시 텍스트 색상 변경 (예: 빨간색)
            if (playerStats != null)
            {
                if (playerStats.gold < currentData.unlockCost)
                {
                    costText.color = Color.red;
                }
                else
                {
                    costText.color = Color.white;
                    canAfford = true;
                }
            }
        }

        if (unlockButton != null)
        {
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockButtonClicked);
            
            unlockButton.interactable = canAfford;
        }
    }

    private void OnUnlockButtonClicked()
    {
        if (companionSystem != null && currentData != null)
        {
            if (companionSystem.UnlockCompanion(currentData.companionId))
            {
                Debug.Log("해금 성공!");
                // 해금 성공 시 상세 정보 팝업 열기
                if (selectionUI != null)
                {
                    selectionUI.ShowDetailPopup(currentData);
                }
                Close();
            }
            else
            {
                Debug.Log("해금 실패: 골드가 부족합니다.");
            }
        }
    }
}
