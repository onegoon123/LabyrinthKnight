using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UpgradeItem : MonoBehaviour
{
    [Header("업그레이드 설정")]
    [SerializeField] private UpgradeType type;

    [SerializeField] private int baseCost = 100;
    [SerializeField] private float costIncreaseRate = 1.5f;
    [SerializeField] private int maxLevel = 50;
    [SerializeField] private Sprite iconSprite;

    [Header("UI 참조")]
    public Button upgradeButton;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI statText;
    public TextMeshProUGUI nextStatText; // Added for next value
    public Image iconImage;

    public UpgradeType Type => type;
    public int BaseCost => baseCost;
    public float CostIncreaseRate => costIncreaseRate;
    public int MaxLevel => maxLevel;

    public void Initialize(Action onUpgradeClicked)
    {
        if (iconImage != null && iconSprite != null)
            iconImage.sprite = iconSprite;

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            if (onUpgradeClicked != null)
                upgradeButton.onClick.AddListener(() => onUpgradeClicked());
        }
    }

    public void UpdateUI(PlayerStats stats, int currentLevel, int currentCost, bool reachedMax, string formattedCost, string nextStatValue = "")
    {
        if (levelText != null)
            levelText.text = maxLevel > 0 ? $"Lv.{currentLevel}/{maxLevel}" : $"Lv.{currentLevel}";

        if (costText != null)
            costText.text = reachedMax ? "MAX" : formattedCost;
            
        if (statText != null && stats != null)
        {
            switch (type)
            {
                case UpgradeType.Health:
                    statText.text = $"{stats.maxHealth} HP";
                    break;
                case UpgradeType.Attack:
                    statText.text = $"{stats.attackPower} ATK";
                    break;
                case UpgradeType.Defense:
                    statText.text = $"{stats.defense} DEF";
                    break;
                case UpgradeType.AttackSpeed:
                    statText.text = $"{stats.attackSpeed:F2} SPD";
                    break;
                case UpgradeType.CritChance:
                    statText.text = $"{stats.critChance:P1}";
                    break;
                case UpgradeType.CritDamage:
                    statText.text = $"x{stats.critDamage:F2}";
                    break;
                default:
                    statText.text = string.Empty;
                    break;
            }
        }

        if (nextStatText != null)
        {
            // Ensure parent is active
            if (nextStatText.transform.parent != null)
                nextStatText.transform.parent.gameObject.SetActive(true);

            if (!reachedMax && !string.IsNullOrEmpty(nextStatValue))
            {
                // Optional: Add a "+" prefix or specific formatting if desired
                nextStatText.text = "+" + nextStatValue;
            }
            else
            {
                nextStatText.text = "";
            }
        }

        if (upgradeButton != null)
        {
            bool canUpgrade = !reachedMax && stats != null && stats.gold >= currentCost;
            upgradeButton.interactable = canUpgrade;
        }
    }
}


