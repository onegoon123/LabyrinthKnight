using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ItemAcquisitionPopup : MonoBehaviour
{
    [Header("Panels")]
    public GameObject itemDetailPanel; // Used when no item is equipped
    public GameObject itemChangePanel; // Used when comparing with equipped item
    
    [Header("Item Detail Panel UI")]
    public Image detailIcon;
    public TextMeshProUGUI detailNameText;
    public Transform detailStatsContainer; // Container for stat displays
    public TextMeshProUGUI detailRarityText;
    public Button detailConfirmButton; // Confirm button (closes popup)

    [Header("Item Change Panel UI - New Item")]
    public Image changeNewIcon;
    public TextMeshProUGUI changeNewNameText;
    public Transform changeNewStatsContainer; // Container for stat displays
    public TextMeshProUGUI changeNewRarityText;
    
    [Header("Item Change Panel UI - Equipped Item")]
    public Image changeEquippedIcon;
    public TextMeshProUGUI changeEquippedNameText;
    public Transform changeEquippedStatsContainer; // Container for stat displays
    public TextMeshProUGUI changeEquippedRarityText;
    
    [Header("Item Change Panel Buttons")]
    public Button changeEquipButton;
    public Button changeCloseButton;
    
    [Header("System References")]
    public EquipmentSystem equipmentSystem;
    
    [Header("Rarity Text Localization")]
    [Tooltip("등급별 표시할 텍스트를 설정하세요")]
    public string commonText = "일반";
    public string uncommonText = "고급";
    public string rareText = "희귀";
    public string epicText = "영웅";
    public string legendaryText = "전설";
    
    [Header("Rarity Colors")]
    [Tooltip("등급별 텍스트 색상을 설정하세요")]
    public Color commonColor = Color.white;
    public Color uncommonColor = Color.green;
    public Color rareColor = Color.blue;
    public Color epicColor = new Color(0.5f, 0f, 0.5f); // Purple
    public Color legendaryColor = new Color(1f, 0.5f, 0f); // Orange
    
    private ItemData currentNewItem;
    
    private void Start()
    {
        if (equipmentSystem == null)
            equipmentSystem = FindFirstObjectByType<EquipmentSystem>();
        
        // Setup Buttons
        if (detailConfirmButton != null) detailConfirmButton.onClick.AddListener(OnCloseButtonClicked);
        if (changeEquipButton != null) changeEquipButton.onClick.AddListener(OnEquipButtonClicked);
        if (changeCloseButton != null) changeCloseButton.onClick.AddListener(OnCloseButtonClicked);
            
        // Hide panels initially
        ClosePopup();
    }
    
    // Public method to show popup directly (for new items)
    public void ShowItemPopup(ItemData newItem)
    {
        if (newItem == null || !newItem.isEquippable)
        {
            Debug.LogWarning("[ItemAcquisitionPopup] Cannot show popup for null or non-equippable item");
            return;
        }
        
        currentNewItem = newItem;
        
        // Check if there's already an equipped item BEFORE auto-equipping
        ItemData previouslyEquipped = null;
        if (equipmentSystem != null)
        {
            previouslyEquipped = equipmentSystem.GetEquippedItem(newItem.equipmentSlot);
        }
        
        // Auto-equip the new item
        if (equipmentSystem != null)
        {
            equipmentSystem.EquipItem(newItem);
            Debug.Log($"[ItemAcquisitionPopup] Auto-equipped {newItem.itemName}");
        }
        
        // Show appropriate panel based on whether there was a previous item
        if (previouslyEquipped != null)
        {
            // Show Change Panel (Comparison)
            Debug.Log($"[ItemAcquisitionPopup] Showing Change Panel - New: {newItem.itemName}, Previous: {previouslyEquipped.itemName}");
            if (itemChangePanel != null)
            {
                itemChangePanel.SetActive(true);
                if (itemDetailPanel != null) itemDetailPanel.SetActive(false);
                
                // Update New Item UI
                UpdateItemUI(newItem, changeNewIcon, changeNewNameText, changeNewStatsContainer, changeNewRarityText);
                
                // Update Previously Equipped Item UI
                UpdateItemUI(previouslyEquipped, changeEquippedIcon, changeEquippedNameText, changeEquippedStatsContainer, changeEquippedRarityText);
            }
        }
        else
        {
            // Show Detail Panel (no previous item)
            Debug.Log($"[ItemAcquisitionPopup] Showing Detail Panel - New: {newItem.itemName}");
            if (itemDetailPanel != null)
            {
                itemDetailPanel.SetActive(true);
                if (itemChangePanel != null) itemChangePanel.SetActive(false);
                
                UpdateItemUI(newItem, detailIcon, detailNameText, detailStatsContainer, detailRarityText);
            }
        }
    }
    
    // Public method to show equipped item detail
    public void ShowEquippedItemDetail(ItemData equippedItem)
    {
        if (equippedItem == null || !equippedItem.isEquippable)
        {
            Debug.LogWarning("[ItemAcquisitionPopup] Cannot show detail for null or non-equippable item");
            return;
        }
        
        currentNewItem = equippedItem;
        
        // Show Detail Panel for equipped item
        if (itemDetailPanel != null)
        {
            itemDetailPanel.SetActive(true);
            if (itemChangePanel != null) itemChangePanel.SetActive(false);
            
            UpdateItemUI(equippedItem, detailIcon, detailNameText, detailStatsContainer, detailRarityText);
            
            Debug.Log($"[ItemAcquisitionPopup] Showing equipped item detail: {equippedItem.itemName}");
        }
    }
    
    private void ShowPopup(ItemData newItem)
    {
        // Check if there is an equipped item in the same slot
        ItemData equippedItem = null;
        if (equipmentSystem != null)
        {
            equippedItem = equipmentSystem.GetEquippedItem(newItem.equipmentSlot);
        }

        if (equippedItem != null)
        {
            Debug.Log($"[ItemAcquisitionPopup] Showing Change Panel - New: {newItem.itemName}, Equipped: {equippedItem.itemName}");
            // Show Change Panel (Comparison)
            if (itemChangePanel != null)
            {
                itemChangePanel.SetActive(true);
                if (itemDetailPanel != null) itemDetailPanel.SetActive(false);
                
                // Update New Item UI
                UpdateItemUI(newItem, changeNewIcon, changeNewNameText, changeNewStatsContainer, changeNewRarityText);
                
                // Update Equipped Item UI
                UpdateItemUI(equippedItem, changeEquippedIcon, changeEquippedNameText, changeEquippedStatsContainer, changeEquippedRarityText);
            }
            else
            {
                Debug.LogError("[ItemAcquisitionPopup] itemChangePanel is null!");
            }
        }
        else
        {
            Debug.Log($"[ItemAcquisitionPopup] Showing Detail Panel - New: {newItem.itemName}");
            // Show Detail Panel (Single Item)
            if (itemDetailPanel != null)
            {
                itemDetailPanel.SetActive(true);
                if (itemChangePanel != null) itemChangePanel.SetActive(false);
                
                UpdateItemUI(newItem, detailIcon, detailNameText, detailStatsContainer, detailRarityText);
            }
            else
            {
                Debug.LogError("[ItemAcquisitionPopup] itemDetailPanel is null!");
            }
        }
    }
    
    private void UpdateItemUI(ItemData item, Image iconImage, TextMeshProUGUI nameText, Transform statsContainer, TextMeshProUGUI rarityText)
    {
        if (item == null) return;
        
        if (iconImage != null)
            iconImage.sprite = item.icon;
            
        if (nameText != null)
        {
            nameText.text = item.itemName;
            nameText.color = GetRarityColor(item.rarity);
        }
            
        if (rarityText != null)
        {
            rarityText.text = GetRarityText(item.rarity);
            rarityText.color = GetRarityColor(item.rarity);
        }
            
        if (statsContainer != null)
        {
            UpdateStatsDisplay(item, statsContainer);
        }
    }
    
    private void UpdateStatsDisplay(ItemData item, Transform statsContainer)
    {
        // Get all StatDisplay components in the container
        StatDisplay[] statDisplays = statsContainer.GetComponentsInChildren<StatDisplay>(true);
        
        // Create a list of stats to display
        List<(string name, string value)> stats = new List<(string, string)>();
        
        if (item.attackBonus > 0) stats.Add(("공격력", $"+{item.attackBonus}"));
        if (item.defenseBonus > 0) stats.Add(("방어력", $"+{item.defenseBonus}"));
        if (item.healthBonus > 0) stats.Add(("체력", $"+{item.healthBonus}"));
        if (item.attackSpeedBonus > 0) stats.Add(("공격속도", $"+{item.attackSpeedBonus*100}%"));
        if (item.critChanceBonus > 0) stats.Add(("치명확률", $"+{item.critChanceBonus*100}%"));
        if (item.critDamageBonus > 0) stats.Add(("치명피해", $"+{item.critDamageBonus*100}%"));
        if (item.moveSpeedBonus > 0) stats.Add(("이동속도", $"+{item.moveSpeedBonus*100}%"));
        
        // Update stat displays
        for (int i = 0; i < statDisplays.Length; i++)
        {
            if (i < stats.Count)
            {
                statDisplays[i].SetStat(stats[i].name, stats[i].value);
                statDisplays[i].Show();
            }
            else
            {
                statDisplays[i].Hide();
            }
        }
    }
    
    private string GetRarityText(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return commonText;
            case ItemRarity.Uncommon: return uncommonText;
            case ItemRarity.Rare: return rareText;
            case ItemRarity.Epic: return epicText;
            case ItemRarity.Legendary: return legendaryText;
            default: return rarity.ToString();
        }
    }
    
    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return commonColor;
            case ItemRarity.Uncommon: return uncommonColor;
            case ItemRarity.Rare: return rareColor;
            case ItemRarity.Epic: return epicColor;
            case ItemRarity.Legendary: return legendaryColor;
            default: return Color.white;
        }
    }
    
    private void OnEquipButtonClicked()
    {
        if (currentNewItem != null && equipmentSystem != null)
        {
            equipmentSystem.EquipItem(currentNewItem);
            ClosePopup();
        }
    }
    
    private void OnUnequipButtonClicked()
    {
        if (currentNewItem != null && equipmentSystem != null)
        {
            equipmentSystem.UnequipItem(currentNewItem.equipmentSlot, false); // Don't add to inventory
            ClosePopup();
            Debug.Log($"[ItemAcquisitionPopup] Unequipped {currentNewItem.itemName}");
        }
    }
    
    private void OnCloseButtonClicked()
    {
        ClosePopup();
    }
    
    public void ClosePopup()
    {
        if (itemDetailPanel != null) itemDetailPanel.SetActive(false);
        if (itemChangePanel != null) itemChangePanel.SetActive(false);
        currentNewItem = null;
    }
}
