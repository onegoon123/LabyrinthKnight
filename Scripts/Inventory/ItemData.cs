using Naninovel;
using System;
using UnityEngine;

[System.Serializable]
public enum ItemType
{
    Consumable,  // 소모품 (포션, 음식 등)
    Weapon,      // 무기
    Armor,       // 방어구
    Accessory,   // 액세서리
    Material,    // 재료
    Quest,       // 퀘스트 아이템
    Misc         // 기타
}

[System.Serializable]
public enum EquipmentSlot
{
    None,        // 장비 불가
    Weapon,      // 무기
    Helmet,      // 투구
    Armor,       // 갑옷
    Boots        // 신발
}

[System.Serializable]
public enum ItemRarity
{
    Common,      // 일반
    Uncommon,    // 고급
    Rare,        // 희귀
    Epic,        // 영웅
    Legendary    // 전설
}

[System.Serializable]
public class ItemData
{
    [Header("기본 정보")]
    public string itemId;
    public string itemName;
    public string description;
    public ItemType itemType;
    public ItemRarity rarity;
    public Sprite icon;
    
    [Header("스탯")]
    public int value;           // 아이템 가치 (골드)
    public int stackSize = 1;   // 최대 스택 수
    public bool isStackable = false;
    public bool isUsable = false;
    public bool isEquippable = false;
    
    [Header("장비 정보")]
    public EquipmentSlot equipmentSlot = EquipmentSlot.None;  // 장비 부위
    public int itemLevel = 1;  // 아이템 레벨 (생성 시 기준 레벨)
    
    [Header("효과")]
    public int healthBonus = 0;
    public int attackBonus = 0;
    public int defenseBonus = 0;
    public float attackSpeedBonus = 0f;
    public float moveSpeedBonus = 0f;  // 이동속도 보너스
    public float critChanceBonus = 0f;  // 치명타 확률 보너스
    public float critDamageBonus = 0f;  // 치명타 데미지 보너스
    public int goldBonus = 0;
    
    [Header("사용 효과")]
    public int healthRestore = 0;
    public float duration = 0f;  // 버프 지속시간
    
    [Header("강화 정보")]
    public int enhancementLevel = 0;
    public int maxEnhancementLevel = 15;
    
    public ItemData()
    {
        itemId = System.Guid.NewGuid().ToString();
    }
    
    public ItemData(string name, ItemType type, ItemRarity rarity)
    {
        itemId = System.Guid.NewGuid().ToString();
        itemName = name;
        itemType = type;
        this.rarity = rarity;
    }
    
    // 아이템 복사 생성자
    public ItemData(ItemData other)
    {
        itemId = System.Guid.NewGuid().ToString();
        itemName = other.itemName;
        description = other.description;
        itemType = other.itemType;
        rarity = other.rarity;
        icon = other.icon;
        value = other.value;
        stackSize = other.stackSize;
        isStackable = other.isStackable;
        isUsable = other.isUsable;
        isEquippable = other.isEquippable;
        equipmentSlot = other.equipmentSlot;
        itemLevel = other.itemLevel;
        healthBonus = other.healthBonus;
        attackBonus = other.attackBonus;
        defenseBonus = other.defenseBonus;
        attackSpeedBonus = other.attackSpeedBonus;
        moveSpeedBonus = other.moveSpeedBonus;
        critChanceBonus = other.critChanceBonus;
        critDamageBonus = other.critDamageBonus;
        goldBonus = other.goldBonus;
        healthRestore = other.healthRestore;
        duration = other.duration;
    }
}
