using System.Collections.Generic;
using UnityEngine;
using System;

public class EquipmentSystem : MonoBehaviour
{
    [Header("장착된 장비")]
    public ItemData equippedWeapon;
    public ItemData equippedHelmet;
    public ItemData equippedArmor;
    public ItemData equippedBoots;
    
    [Header("시스템 참조")]
    public PlayerStats playerStats;
    
    // 이벤트
    public event Action<ItemData, EquipmentSlot> OnItemEquipped;
    public event Action<ItemData, EquipmentSlot> OnItemUnequipped;
    public event Action OnEquipmentChanged;
    
    // 장비 딕셔너리 (빠른 접근용)
    private Dictionary<EquipmentSlot, ItemData> equippedItems = new Dictionary<EquipmentSlot, ItemData>();
    
    private void Awake()
    {
        // Force clear any empty items assigned in Inspector
        if (equippedWeapon != null && string.IsNullOrEmpty(equippedWeapon.itemName))
            equippedWeapon = null;
        if (equippedHelmet != null && string.IsNullOrEmpty(equippedHelmet.itemName))
            equippedHelmet = null;
        if (equippedArmor != null && string.IsNullOrEmpty(equippedArmor.itemName))
            equippedArmor = null;
        if (equippedBoots != null && string.IsNullOrEmpty(equippedBoots.itemName))
            equippedBoots = null;
            
        InitializeEquipment();
    }
    
    private void Start()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
        
        // 빈 아이템 정리 (Start에서 한 번 더 확인)
        CleanupEmptyItems();
        
        // 장비 딕셔너리 초기화
        UpdateEquipmentDictionary();
        
        // 장비 변경 시 PlayerStats에 알림
        OnEquipmentChanged += NotifyPlayerStatsChanged;
    }
    
    /// <summary>
    /// 빈 아이템 정리
    /// </summary>
    private void CleanupEmptyItems()
    {
        if (!IsValidItem(equippedWeapon)) equippedWeapon = null;
        if (!IsValidItem(equippedHelmet)) equippedHelmet = null;
        if (!IsValidItem(equippedArmor)) equippedArmor = null;
        if (!IsValidItem(equippedBoots)) equippedBoots = null;
    }
    
    private void OnDestroy()
    {
        OnEquipmentChanged -= NotifyPlayerStatsChanged;
    }
    
    private void NotifyPlayerStatsChanged()
    {
        if (playerStats != null)
        {
            playerStats.NotifyStatsChanged();
        }
    }
    
    private void InitializeEquipment()
    {
        // Inspector에서 설정된 기본 장착 아이템이 빈 아이템인 경우 null로 초기화
        if (!IsValidItem(equippedWeapon)) equippedWeapon = null;
        if (!IsValidItem(equippedHelmet)) equippedHelmet = null;
        if (!IsValidItem(equippedArmor)) equippedArmor = null;
        if (!IsValidItem(equippedBoots)) equippedBoots = null;
        
        // 딕셔너리 초기화 (검증된 값으로 설정)
        UpdateEquipmentDictionary();
    }
    
    private void UpdateEquipmentDictionary()
    {
        // 유효한 아이템만 딕셔너리에 설정
        equippedItems[EquipmentSlot.Weapon] = IsValidItem(equippedWeapon) ? equippedWeapon : null;
        equippedItems[EquipmentSlot.Helmet] = IsValidItem(equippedHelmet) ? equippedHelmet : null;
        equippedItems[EquipmentSlot.Armor] = IsValidItem(equippedArmor) ? equippedArmor : null;
        equippedItems[EquipmentSlot.Boots] = IsValidItem(equippedBoots) ? equippedBoots : null;
    }
    
    /// <summary>
    /// 아이템 장착
    /// </summary>
    public bool EquipItem(ItemData item, bool suppressWarning = false)
    {
        if (item == null || !item.isEquippable || item.equipmentSlot == EquipmentSlot.None)
        {
            if (!suppressWarning)
            {
                Debug.LogWarning("장착할 수 없는 아이템입니다.");
            }
            return false;
        }
        
        EquipmentSlot slot = item.equipmentSlot;
        
        // 기존 장비 가져오기 (빈 아이템은 null로 반환됨)
        ItemData previousItem = GetEquippedItem(slot);
        
        // 기존 장비 해제 (previousItem이 null이면 빈 아이템이므로 해제만 함)
        if (previousItem != null)
        {
            // GetEquippedItem이 이미 빈 아이템을 필터링하므로 여기서는 유효한 아이템만 옴
            // 유효한 아이템만 해제하고 인벤토리에 추가
            UnequipItem(slot, true); // addToInventory = true로 설정하여 인벤토리에 추가
        }
        else
        {
            // 빈 아이템이 장착되어 있으면 그냥 해제만 함 (인벤토리 추가 안 함)
            SetEquippedItem(slot, null);
        }
        
        // 새 장비 장착
        SetEquippedItem(slot, item);
        
        // 스탯 적용
        ApplyEquipmentStats();
        
        OnItemEquipped?.Invoke(item, slot);
        OnEquipmentChanged?.Invoke();
        
        Debug.Log($"{item.itemName} 장착 완료!");
        return true;
    }
    
    /// <summary>
    /// 아이템이 유효한지 확인 (빈 아이템 체크)
    /// </summary>
    private bool IsValidItem(ItemData item)
    {
        if (item == null) return false;
        
        // 이름, 아이콘, itemId가 모두 없으면 빈 아이템으로 간주
        bool hasName = !string.IsNullOrWhiteSpace(item.itemName);
        bool hasId = !string.IsNullOrWhiteSpace(item.itemId);
        bool hasIcon = item.icon != null;
        
        // 하나라도 있으면 유효한 아이템
        if (hasName || hasId || hasIcon) return true;
        
        // 모든 것이 비어있으면 빈 아이템
        return false;
    }
    
    /// <summary>
    /// 아이템 해제
    /// </summary>
    public bool UnequipItem(EquipmentSlot slot, bool addToInventory = true)
    {
        ItemData item = GetEquippedItem(slot);
        if (item == null) return false;
        
        // 빈 아이템은 인벤토리에 추가하지 않음
        bool isValid = IsValidItem(item);
        
        // 장비 해제
        SetEquippedItem(slot, null);
        
        // 스탯 재계산
        ApplyEquipmentStats();
        
        if (isValid)
        {
            OnItemUnequipped?.Invoke(item, slot);
            Debug.Log($"{item.itemName} 해제 완료!");
        }
        
        OnEquipmentChanged?.Invoke();
        return true;
    }
    
    /// <summary>
    /// 모든 장비 해제
    /// </summary>
    public void UnequipAll(bool addToInventory = true)
    {
        foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (slot != EquipmentSlot.None)
            {
                UnequipItem(slot, addToInventory);
            }
        }
    }
    
    /// <summary>
    /// 장착된 아이템 가져오기
    /// </summary>
    public ItemData GetEquippedItem(EquipmentSlot slot)
    {
        // public 필드에서 직접 가져오기 (딕셔너리보다 우선)
        ItemData item = null;
        
        switch (slot)
        {
            case EquipmentSlot.Weapon:
                item = equippedWeapon;
                break;
            case EquipmentSlot.Helmet:
                item = equippedHelmet;
                break;
            case EquipmentSlot.Armor:
                item = equippedArmor;
                break;
            case EquipmentSlot.Boots:
                item = equippedBoots;
                break;
        }
        
        // 빈 아이템이면 null 반환 및 정리
        if (!IsValidItem(item))
        {
            // public 필드도 null로 설정
            switch (slot)
            {
                case EquipmentSlot.Weapon:
                    equippedWeapon = null;
                    break;
                case EquipmentSlot.Helmet:
                    equippedHelmet = null;
                    break;
                case EquipmentSlot.Armor:
                    equippedArmor = null;
                    break;
                case EquipmentSlot.Boots:
                    equippedBoots = null;
                    break;
            }
            
            // 딕셔너리에서도 제거
            if (equippedItems.ContainsKey(slot))
            {
                equippedItems[slot] = null;
            }
            
            return null;
        }
        
        return item;
    }
    
    /// <summary>
    /// 장비 설정 (내부용)
    /// </summary>
    private void SetEquippedItem(EquipmentSlot slot, ItemData item)
    {
        // 빈 아이템은 null로 설정
        if (!IsValidItem(item))
        {
            item = null;
        }
        
        switch (slot)
        {
            case EquipmentSlot.Weapon:
                equippedWeapon = item;
                break;
            case EquipmentSlot.Helmet:
                equippedHelmet = item;
                break;
            case EquipmentSlot.Armor:
                equippedArmor = item;
                break;
            case EquipmentSlot.Boots:
                equippedBoots = item;
                break;
        }
        
        // 딕셔너리도 업데이트
        equippedItems[slot] = item;
    }
    
    /// <summary>
    /// 장비 스탯 적용
    /// </summary>
    private void ApplyEquipmentStats()
    {
        if (playerStats == null) return;
        
        // 장비 스탯 합계 계산
        int totalHealthBonus = 0;
        int totalAttackBonus = 0;
        int totalDefenseBonus = 0;
        float totalAttackSpeedBonus = 0f;
        float totalMoveSpeedBonus = 0f;
        float totalCritChanceBonus = 0f;
        float totalCritDamageBonus = 0f;
        
        foreach (var kvp in equippedItems)
        {
            ItemData item = kvp.Value;
            if (item == null) continue;
            
            totalHealthBonus += item.healthBonus;
            totalAttackBonus += item.attackBonus;
            totalDefenseBonus += item.defenseBonus;
            totalAttackSpeedBonus += item.attackSpeedBonus;
            totalMoveSpeedBonus += item.moveSpeedBonus;
            totalCritChanceBonus += item.critChanceBonus;
            totalCritDamageBonus += item.critDamageBonus;
        }
        
        // PlayerStats에 장비 스탯 저장 (나중에 계산에 반영)
        // PlayerStats에 장비 보너스 필드가 필요함
        // 일단 이벤트로 알림만 보내고, PlayerStats에서 직접 계산하도록 할 수도 있음
        
        // 임시: PlayerStats에 장비 보너스 필드가 있다고 가정
        // 실제로는 PlayerStats에 장비 보너스 필드를 추가해야 함
        OnEquipmentChanged?.Invoke();
    }
    
    /// <summary>
    /// 모든 장비 스탯 합계 가져오기
    /// </summary>
    public EquipmentStats GetTotalEquipmentStats()
    {
        EquipmentStats stats = new EquipmentStats();
        
        foreach (var kvp in equippedItems)
        {
            ItemData item = kvp.Value;
            if (item == null) continue;
            
            stats.healthBonus += item.healthBonus;
            stats.attackBonus += item.attackBonus;
            stats.defenseBonus += item.defenseBonus;
            stats.attackSpeedBonus += item.attackSpeedBonus;
            stats.moveSpeedBonus += item.moveSpeedBonus;
            stats.critChanceBonus += item.critChanceBonus;
            stats.critDamageBonus += item.critDamageBonus;
        }
        
        return stats;
    }
}

/// <summary>
/// 장비 스탯 합계 구조체
/// </summary>
[System.Serializable]
public class EquipmentStats
{
    public int healthBonus = 0;
    public int attackBonus = 0;
    public int defenseBonus = 0;
    public float attackSpeedBonus = 0f;
    public float moveSpeedBonus = 0f;
    public float critChanceBonus = 0f;
    public float critDamageBonus = 0f;
}

