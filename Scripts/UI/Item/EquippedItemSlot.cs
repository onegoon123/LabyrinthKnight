using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 개별 장착 아이템 슬롯 UI 컴포넌트
/// 클릭하면 ItemAcquisitionPopup의 Detail Panel로 아이템 정보 표시
/// </summary>
public class EquippedItemSlot : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public Button slotButton;
    
    [Header("Slot Settings")]
    public EquipmentSlot equipmentSlot;
    
    [Header("System References")]
    public EquipmentSystem equipmentSystem;
    public ItemAcquisitionPopup itemAcquisitionPopup;
    
    private ItemData currentItem;
    
    private void Start()
    {
        // Find systems if not assigned
        if (equipmentSystem == null)
            equipmentSystem = FindFirstObjectByType<EquipmentSystem>();
            
        if (itemAcquisitionPopup == null)
            itemAcquisitionPopup = FindFirstObjectByType<ItemAcquisitionPopup>();
        
        // Setup button click event
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotClicked);
        }
        
        // Subscribe to equipment changes
        if (equipmentSystem != null)
        {
            equipmentSystem.OnEquipmentChanged += UpdateSlot;
        }
        
        // Initial update
        UpdateSlot();
    }
    
    private void OnDestroy()
    {
        if (equipmentSystem != null)
        {
            equipmentSystem.OnEquipmentChanged -= UpdateSlot;
        }
        
        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(OnSlotClicked);
        }
    }
    
    /// <summary>
    /// 슬롯 UI 업데이트
    /// </summary>
    public void UpdateSlot()
    {
        if (equipmentSystem == null) return;
        
        // Get equipped item for this slot
        currentItem = equipmentSystem.GetEquippedItem(equipmentSlot);
        
        if (currentItem != null)
        {
            // Show item
            if (itemIcon != null)
            {
                itemIcon.sprite = currentItem.icon;
                itemIcon.enabled = true;
            }
            
            if (slotButton != null)
            {
                slotButton.interactable = true;
            }
        }
        else
        {
            // Empty slot
            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.enabled = false;
            }
            
            if (slotButton != null)
            {
                slotButton.interactable = false;
            }
        }
    }
    
    /// <summary>
    /// 슬롯 클릭 시 호출
    /// </summary>
    private void OnSlotClicked()
    {
        if (currentItem == null)
        {
            Debug.Log($"[EquippedItemSlot] No item equipped in {equipmentSlot} slot");
            return;
        }
        
        if (itemAcquisitionPopup == null)
        {
            Debug.LogError("[EquippedItemSlot] ItemAcquisitionPopup is null!");
            return;
        }
        
        // Show item detail in popup
        itemAcquisitionPopup.ShowEquippedItemDetail(currentItem);
        Debug.Log($"[EquippedItemSlot] Showing detail for {currentItem.itemName}");
    }
}
