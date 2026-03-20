using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DungeonPanel : MonoBehaviour
{
    [Header("UI 요소")]
    public Transform contentParent;
    public GameObject dungeonListItemPrefab;
    public Button closeButton;
    
    [Header("데이터")]
    public List<DungeonData> availableDungeons; // Inspector에서 할당하거나 로드
    
    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
        
        RefreshList();
    }
    
    public void OpenPanel()
    {
        gameObject.SetActive(true);
        RefreshList();
    }
    
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
    
    private void RefreshList()
    {
        if (contentParent == null || dungeonListItemPrefab == null) return;
        
        // 기존 아이템 제거
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        
        // 던전 목록 생성
        foreach (var dungeon in availableDungeons)
        {
            if (dungeon == null) continue;
            
            GameObject itemObj = Instantiate(dungeonListItemPrefab, contentParent);
            DungeonListItem item = itemObj.GetComponent<DungeonListItem>();
            
            if (item != null)
            {
                item.Initialize(dungeon, OnDungeonEnterClicked);
            }
        }
    }
    
    private void OnDungeonEnterClicked(DungeonData dungeon)
    {
        Debug.Log($"[DungeonPanel] 던전 입장 요청: {dungeon.dungeonName}");

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.EnterDungeon(dungeon);
            ClosePanel();
        }
        else
        {
            // 폴백: 인스턴스가 설정되지 않았다면 직접 찾기
            var manager = FindFirstObjectByType<DungeonManager>();
            if (manager != null)
            {
                Debug.Log("[DungeonPanel] DungeonManager를 수동으로 찾았습니다.");
                manager.EnterDungeon(dungeon);
                ClosePanel();
            }
            else
            {
                Debug.LogError("[DungeonPanel] DungeonManager가 씬에 없습니다! 던전에 입장할 수 없습니다.");
            }
        }
    }
}
