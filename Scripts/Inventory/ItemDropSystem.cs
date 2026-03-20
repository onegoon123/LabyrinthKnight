using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropItem
{
    public ItemData item;
    public float dropRate;  // 0.0 ~ 1.0
    public int minQuantity = 1;
    public int maxQuantity = 1;
}

public class ItemDropSystem : MonoBehaviour
{
    [Header("드롭 설정")]
    public List<DropItem> dropItems = new List<DropItem>();
    public float baseDropRate = 0.3f;  // 기본 드롭 확률
    public int maxDropsPerKill = 3;    // 처치당 최대 드롭 수
    
    [Header("랜덤 장비 드롭 설정")]
    public bool enableRandomEquipmentDrop = false; // 기본값 false로 변경 (생성 시스템 사용)
    public float equipmentDropRate = 0.2f;  // 장비 드롭 확률
    public EquipmentSlot[] possibleEquipmentSlots = { EquipmentSlot.Weapon, EquipmentSlot.Helmet, EquipmentSlot.Armor, EquipmentSlot.Boots };
    
    [Header("시스템 참조")]
    public EquipmentCraftingSystem equipmentCraftingSystem;
    
    private void Start()
    {
        if (equipmentCraftingSystem == null)
            equipmentCraftingSystem = FindFirstObjectByType<EquipmentCraftingSystem>();
    }
    
    // 적 처치 시 아이템 드롭 (비활성화됨)
    public void DropItemsOnKill(int enemyLevel = 1)
    {
        // 드롭 시스템 비활성화 - 아이템은 EquipmentCraftingSystem에서 생성
        return;
    }
    
    // 특정 아이템 드롭 (비활성화 - 인벤토리 시스템 제거됨)
    public bool DropSpecificItem(ItemData item, int quantity = 1)
    {
        Debug.LogWarning("드롭 시스템이 비활성화되었습니다.");
        return false;
    }
    
    // 드롭 아이템 추가
    public void AddDropItem(ItemData item, float dropRate, int minQuantity = 1, int maxQuantity = 1)
    {
        DropItem newDrop = new DropItem
        {
            item = item,
            dropRate = dropRate,
            minQuantity = minQuantity,
            maxQuantity = maxQuantity
        };
        dropItems.Add(newDrop);
    }
    
    // 드롭 아이템 제거
    public void RemoveDropItem(ItemData item)
    {
        dropItems.RemoveAll(drop => drop.item == item);
    }
    
    // 드롭 확률 조정
    public void AdjustDropRate(ItemData item, float newDropRate)
    {
        DropItem dropItem = dropItems.Find(drop => drop.item == item);
        if (dropItem != null)
        {
            dropItem.dropRate = newDropRate;
        }
    }
}
