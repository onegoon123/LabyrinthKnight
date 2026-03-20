using UnityEngine;

/// <summary>
/// 모든 장착 아이템 슬롯을 관리하는 매니저
/// </summary>
public class EquippedItemsUI : MonoBehaviour
{
    [Header("Equipped Item Slots")]
    public EquippedItemSlot weaponSlot;
    public EquippedItemSlot helmetSlot;
    public EquippedItemSlot armorSlot;
    public EquippedItemSlot bootsSlot;
    
    [Header("System References")]
    public EquipmentSystem equipmentSystem;
    public ItemAcquisitionPopup itemAcquisitionPopup;
    
    private void Start()
    {
        // Find systems if not assigned
        if (equipmentSystem == null)
            equipmentSystem = FindFirstObjectByType<EquipmentSystem>();
            
        if (itemAcquisitionPopup == null)
            itemAcquisitionPopup = FindFirstObjectByType<ItemAcquisitionPopup>();
        
        // Initialize all slots
        InitializeSlots();
        
        // Subscribe to equipment changes
        if (equipmentSystem != null)
        {
            equipmentSystem.OnEquipmentChanged += UpdateAllSlots;
        }
        
        // Initial update
        UpdateAllSlots();
    }
    
    private void OnDestroy()
    {
        if (equipmentSystem != null)
        {
            equipmentSystem.OnEquipmentChanged -= UpdateAllSlots;
        }
    }
    
    /// <summary>
    /// 모든 슬롯 초기화
    /// </summary>
    private void InitializeSlots()
    {
        if (weaponSlot != null)
        {
            weaponSlot.equipmentSlot = EquipmentSlot.Weapon;
            weaponSlot.equipmentSystem = equipmentSystem;
            weaponSlot.itemAcquisitionPopup = itemAcquisitionPopup;
        }
        
        if (helmetSlot != null)
        {
            helmetSlot.equipmentSlot = EquipmentSlot.Helmet;
            helmetSlot.equipmentSystem = equipmentSystem;
            helmetSlot.itemAcquisitionPopup = itemAcquisitionPopup;
        }
        
        if (armorSlot != null)
        {
            armorSlot.equipmentSlot = EquipmentSlot.Armor;
            armorSlot.equipmentSystem = equipmentSystem;
            armorSlot.itemAcquisitionPopup = itemAcquisitionPopup;
        }
        
        if (bootsSlot != null)
        {
            bootsSlot.equipmentSlot = EquipmentSlot.Boots;
            bootsSlot.equipmentSystem = equipmentSystem;
            bootsSlot.itemAcquisitionPopup = itemAcquisitionPopup;
        }
    }
    
    /// <summary>
    /// 모든 슬롯 UI 업데이트
    /// </summary>
    public void UpdateAllSlots()
    {
        if (weaponSlot != null) weaponSlot.UpdateSlot();
        if (helmetSlot != null) helmetSlot.UpdateSlot();
        if (armorSlot != null) armorSlot.UpdateSlot();
        if (bootsSlot != null) bootsSlot.UpdateSlot();
    }
    
    /// <summary>
    /// 특정 슬롯 업데이트
    /// </summary>
    public void UpdateSlot(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon:
                if (weaponSlot != null) weaponSlot.UpdateSlot();
                break;
            case EquipmentSlot.Helmet:
                if (helmetSlot != null) helmetSlot.UpdateSlot();
                break;
            case EquipmentSlot.Armor:
                if (armorSlot != null) armorSlot.UpdateSlot();
                break;
            case EquipmentSlot.Boots:
                if (bootsSlot != null) bootsSlot.UpdateSlot();
                break;
        }
    }
}
