using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompanionDetailPopup : MonoBehaviour
{
    [Header("UI 요소")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI affinityText; // 호감도 정보
    
    [Header("스토리 해금")]
    public Transform storyParent; // 스토리 오브젝트들의 부모

    [Header("버튼")]
    public Button actionButton; // 장착/해제 버튼
    public TextMeshProUGUI actionButtonText;
    public Button closeButton;

    private CompanionData currentData;
    private CompanionSystem companionSystem;

    private void Awake()
    {
        companionSystem = FindFirstObjectByType<CompanionSystem>();
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }
    }

    public void Open(CompanionData data)
    {
        currentData = data;
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void UpdateUI()
    {
        if (currentData == null || companionSystem == null) return;

        // 기본 정보
        if (portraitImage != null) portraitImage.sprite = currentData.portrait;
        if (nameText != null) nameText.text = currentData.characterName;

        // 호감도 정보 및 스토리 해금
        int affinity = companionSystem.GetCompanionAffinity(currentData.companionId);
        
        if (affinityText != null)
        {
            affinityText.text = $"{affinity} / 100";
        }

        // 스토리 오브젝트 활성화/비활성화 (20, 40, 60, 80, 100)
        if (storyParent != null)
        {
            var dialogueManager = FindFirstObjectByType<DialogueManager>();
            if (dialogueManager != null)
            {
                dialogueManager.SetCurrentCompanionId(currentData.companionId);
            }

            int childCount = storyParent.childCount;
            // 각 단계별 요구 호감도: 20 * (index + 1)
            for (int i = 0; i < childCount; i++)
            {
                Transform child = storyParent.GetChild(i);
                int requiredAffinity = (i + 1) * 20;
                
                // 예: 인덱스 0 -> 20, 인덱스 1 -> 40 ...
                bool isUnlocked = affinity >= requiredAffinity;
                
                child.gameObject.SetActive(isUnlocked);
                
                // 해금된 경우 버튼 이벤트 연결
                if (isUnlocked)
                {
                    Button btn = child.GetComponent<Button>();
                    if (btn != null)
                    {
                        string storySuffix = $"Story{i + 1}"; // Story1, Story2...
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => {
                            if (dialogueManager != null)
                            {
                                dialogueManager.StartCompanionStory(storySuffix);
                            }
                            else
                            {
                                Debug.LogError("DialogueManager not found!");
                            }
                        });
                    }
                }
            }
        }

        // 버튼 설정 (장착/해제)
        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            string currentCompanionId = companionSystem.GetCurrentCompanionId();
            bool isEquipped = currentCompanionId == currentData.companionId;

            if (isEquipped)
            {
                actionButtonText.text = "퇴장";
                actionButton.onClick.AddListener(() => {
                    companionSystem.RemoveCompanion();
                    UpdateUI(); // 버튼 텍스트 갱신
                });
            }
            else
            {
                actionButtonText.text = "참가";
                actionButton.onClick.AddListener(() => {
                    companionSystem.SetActiveCompanion(currentData);
                    UpdateUI(); // 버튼 텍스트 갱신
                });
            }
        }
    }

    public void StartStory(string id)
    {
        var dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.StartCompanionStory(id);
        }
    }
}
