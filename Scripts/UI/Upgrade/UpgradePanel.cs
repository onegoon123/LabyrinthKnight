using System.Collections.Generic;
using UnityEngine;
using DarkNaku.Number;

public class UpgradePanel : MonoBehaviour
{
    [Header("업그레이드 항목")]
    [SerializeField] private List<UpgradeItem> upgradeItems = new List<UpgradeItem>();

    private readonly Dictionary<UpgradeType, UpgradeItem> upgradeLookup = new Dictionary<UpgradeType, UpgradeItem>();
    private PlayerStats playerStats;

    private void Awake()
    {
        if (upgradeItems == null || upgradeItems.Count == 0)
        {
            upgradeItems = new List<UpgradeItem>(GetComponentsInChildren<UpgradeItem>(true));
        }
    }

    private void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();

        foreach (var item in upgradeItems)
        {
            RegisterItem(item);
        }

        if (playerStats != null)
        {
            playerStats.OnStatsChanged += UpdateUI;
            // 저장된 업그레이드 레벨 로드
            LoadUpgradeLevels();
        }

        UpdateUI();
    }
    
    private void LoadUpgradeLevels()
    {
        // UpgradeProgress는 이제 PlayerStats를 직접 참조하므로
        // GameSaveSystem의 LoadGame에서 이미 PlayerStats에 로드됨
        // 여기서는 추가 동기화가 필요 없음 (UpgradeProgress.GetLevel이 PlayerStats를 참조)
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged -= UpdateUI;
    }

    private void RegisterItem(UpgradeItem item)
    {
        if (item == null) return;

        upgradeLookup[item.Type] = item;
        item.Initialize(() => AttemptUpgrade(item));
    }

    public bool TryUpgrade(UpgradeType type)
    {
        if (!upgradeLookup.TryGetValue(type, out var item) || item == null)
        {
            Debug.LogWarning($"UpgradePanel: {type} 항목을 찾을 수 없습니다.");
            return false;
        }

        AttemptUpgrade(item);
        return true;
    }

    private void AttemptUpgrade(UpgradeItem item)
    {
        if (playerStats == null) return;

        int currentLevel = GetCurrentLevel(item.Type);
        if (IsMaxLevel(item, currentLevel))
        {
            Debug.Log("이미 최대 레벨입니다!");
            return;
        }

        int cost = CalculateCost(item, currentLevel);
        if (playerStats.gold < cost)
        {
            Debug.Log("골드가 부족합니다!");
            return;
        }

        if (PerformUpgrade(item, cost))
        {
            // PerformUpgrade 내부에서 이미 레벨이 증가하고 저장됨
            UpdateItemUI(item);
        }
    }

    private bool PerformUpgrade(UpgradeItem item, int cost)
    {
        // 지수 성장 곡선 방식: 레벨만 증가시키고 스탯은 자동 계산됨
        // UpgradeProgress는 이제 PlayerStats를 직접 참조하므로 별도 동기화 불필요
        switch (item.Type)
        {
            case UpgradeType.Health:
                return playerStats.UpgradeHealth(cost);
            case UpgradeType.Attack:
                return playerStats.UpgradeAttack(cost);
            case UpgradeType.Defense:
                return playerStats.UpgradeDefense(cost);
            case UpgradeType.AttackSpeed:
                return playerStats.UpgradeAttackSpeed(cost);
            case UpgradeType.CritChance:
                return playerStats.UpgradeCritChance(cost);
            case UpgradeType.CritDamage:
                return playerStats.UpgradeCritDamage(cost);
            default:
                return false;
        }
    }

    private void UpdateUI()
    {
        foreach (var item in upgradeItems)
        {
            UpdateItemUI(item);
        }
    }

    private void UpdateItemUI(UpgradeItem item)
    {
        if (item == null) return;

        int currentLevel = GetCurrentLevel(item.Type);
        int cost = CalculateCost(item, currentLevel);
        bool maxed = IsMaxLevel(item, currentLevel);
        string formattedCost = $"{FormatNumber(cost)}";

        // Calculate next level stat value
        string nextStatValue = "";
        if (!maxed && playerStats != null)
        {
            float currentValue = playerStats.GetStatValue(item.Type, currentLevel);
            float nextValue = playerStats.GetStatValue(item.Type, currentLevel + 1);
            float increaseValue = nextValue - currentValue;
            
            switch (item.Type)
            {
                case UpgradeType.Health:
                case UpgradeType.Attack:
                case UpgradeType.Defense:
                    nextStatValue = $"{(int)increaseValue}";
                    break;
                case UpgradeType.AttackSpeed:
                case UpgradeType.CritDamage:
                    nextStatValue = $"{increaseValue:F2}";
                    break;
                case UpgradeType.CritChance:
                    nextStatValue = $"{increaseValue:P1}";
                    break;
            }
        }

        item.UpdateUI(playerStats, currentLevel, cost, maxed, formattedCost, nextStatValue);
    }

    private int GetCurrentLevel(UpgradeType type)
    {
        return UpgradeProgress.GetLevel(type);
    }

    private int CalculateCost(UpgradeItem item, int level)
    {
        return Mathf.RoundToInt(item.BaseCost * Mathf.Pow(item.CostIncreaseRate, level));
    }

    private bool IsMaxLevel(UpgradeItem item, int level)
    {
        return item.MaxLevel > 0 && level >= item.MaxLevel;
    }

    private string FormatNumber(long number)
    {
        return new Number(number).ToString();
    }
}

