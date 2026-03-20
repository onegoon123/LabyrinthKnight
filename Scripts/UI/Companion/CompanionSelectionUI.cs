using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 동료 선택 UI를 관리하는 패널
/// NavigationController에 의해 제어됩니다.
/// </summary>
public class CompanionSelectionUI : MonoBehaviour
{
    [Header("UI 참조")]
    public Transform companionListParent; // 동료 목록이 생성될 부모 (ScrollView Content)
    [Header("팝업 참조")]
    public CompanionUnlockPopup unlockPopup;
    public CompanionDetailPopup detailPopup;

    public GameObject companionItemPrefab; // 동료 아이템 프리팹
    
    [Header("시스템 참조")]
    private CompanionSystem companionSystem;
    
    private void Start()
    {
        companionSystem = FindFirstObjectByType<CompanionSystem>();
        
        if (companionSystem == null)
        {
            Debug.LogError("[CompanionSelectionUI] CompanionSystem not found!");
            return;
        }
        
        // 동료 목록 생성
        PopulateCompanionList();
    }
    
    private void OnEnable()
    {
        // 패널이 활성화될 때마다 목록 갱신
        if (companionSystem != null)
        {
            PopulateCompanionList();
        }
    }

    private void OnDisable()
    {
        // 패널이 닫힐 때 팝업들도 함께 닫음
        if (unlockPopup != null) unlockPopup.Close();
        if (detailPopup != null) detailPopup.Close();
    }
    
    /// <summary>
    /// 동료 목록을 UI에 표시합니다.
    /// </summary>
    private void PopulateCompanionList()
    {
        if (companionListParent == null || companionItemPrefab == null)
        {
            Debug.LogError("[CompanionSelectionUI] UI references are not set!");
            return;
        }
        
        // 기존 목록 제거
        foreach (Transform child in companionListParent)
        {
            Destroy(child.gameObject);
        }
        
        // 동료 목록 가져오기
        List<CompanionData> companions = companionSystem.GetAllCompanions();
        
        if (companions == null || companions.Count == 0)
        {
            Debug.LogWarning("[CompanionSelectionUI] No companions available!");
            return;
        }
        
        // 각 동료에 대한 UI 아이템 생성
        foreach (CompanionData companion in companions)
        {
            if (companion == null) continue;
            
            GameObject itemObj = Instantiate(companionItemPrefab, companionListParent);
            CompanionSelectionItem item = itemObj.GetComponent<CompanionSelectionItem>();
            
            if (item != null)
            {
                item.Initialize(companion, this);
            }
        }
    }

    /// <summary>
    /// 동료를 선택했을 때의 동작 (CompanionSelectionItem에서 호출)
    /// </summary>
    public void SelectCompanion(CompanionData companion)
    {
        if (companionSystem == null || companion == null) return;

        if (companionSystem.IsCompanionUnlocked(companion.companionId))
        {
            // 해금된 상태 -> 상세 정보 창 열기
            ShowDetailPopup(companion);
        }
        else
        {
            // 잠긴 상태 -> 해금 창 열기
            ShowUnlockPopup(companion);
        }
    }

    public void ShowUnlockPopup(CompanionData data)
    {
        if (unlockPopup != null)
        {
            // CompanionUI 호환성을 위해 임시로 this를 CompanionUI로 캐스팅하거나 
            // CompanionUnlockPopup이 CompanionSelectionUI를 받도록 수정해야 함.
            // 여기서는 CompanionUnlockPopup을 수정하는 대신, 
            // 팝업이 CompanionSelectionUI를 알 필요가 없도록 하거나 인터페이스를 쓰는데,
            // 일단 기존에 만든 CompanionUnlockPopup은 CompanionUI를 받도록 되어있음.
            // 따라서 CompanionUnlockPopup의 Open 메서드를 오버로딩하거나 수정해야 함.
            // 일단 Open 메서드 호출 시 ui 인자를 null로 주거나, 팝업 코드를 수정해야 함.
            // 가장 좋은 방법: CompanionUnlockPopup이 부모 UI 의존성을 줄이거나 제네릭하게 처리.
            
            // 임시 해결책: CompanionUnlockPopup에 CompanionSelectionUI도 받을 수 있게 오버로딩 추가 예정.
            unlockPopup.Open(data, this); 
        }
        else
        {
            Debug.LogWarning("Unlock Popup is not assigned!");
        }
    }

    public void ShowDetailPopup(CompanionData data)
    {
        if (detailPopup != null)
        {
            detailPopup.Open(data);
        }
        else
        {
            Debug.LogWarning("Detail Popup is not assigned!");
        }
    }
}
