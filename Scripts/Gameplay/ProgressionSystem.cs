using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class ProgressionGate
{
    public string gateName;
    public int requiredLevel;
    public int requiredGold;
    public List<string> requiredItems;
    public List<string> requiredAchievements;
    public bool isUnlocked = false;
    
    [Header("해금 보상")]
    public int goldReward;
    public List<ItemData> itemRewards;
    public string description;
}

[System.Serializable]
public class DungeonTier
{
    public string tierName;
    public int minLevel;
    public int maxLevel;
    public float expMultiplier = 1f;
    public float goldMultiplier = 1f;
    public float rareItemChance = 0.1f;
    public List<string> availableLootTables;
    public Color tierColor = Color.white;
}

public class ProgressionSystem : MonoBehaviour
{
    [Header("진행도 게이트들")]
    public List<ProgressionGate> progressionGates = new List<ProgressionGate>();
    
    [Header("던전 티어들")]
    public List<DungeonTier> dungeonTiers = new List<DungeonTier>();
    
    [Header("현재 진행도")]
    public int currentPlayerLevel = 1;
    public int currentDungeonTier = 0;
    public List<string> unlockedGates = new List<string>();
    
    [Header("시스템 참조")]
    public PlayerStats playerStats;
    // public InventorySystem inventorySystem; // 인벤토리 시스템 제거됨
    
    public event Action<ProgressionGate> OnGateUnlocked;
    public event Action<DungeonTier> OnDungeonTierUnlocked;
    
    private void Start()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
        // if (inventorySystem == null)
        //     inventorySystem = FindFirstObjectByType<InventorySystem>();
        
        InitializeProgression();
    }
    
    private void InitializeProgression()
    {
        // 기본 게이트들 생성
        CreateDefaultGates();
        CreateDefaultDungeonTiers();
        
        // 현재 진행도 로드
        LoadProgression();
    }
    
    private void CreateDefaultGates()
    {
        // 레벨 5: 첫 번째 던전 해금
        progressionGates.Add(new ProgressionGate
        {
            gateName = "첫 번째 던전",
            requiredLevel = 5,
            requiredGold = 0,
            description = "레벨 5 달성 시 새로운 던전이 해금됩니다."
        });
        
        // 레벨 10: 강화 시스템 해금
        progressionGates.Add(new ProgressionGate
        {
            gateName = "아이템 강화",
            requiredLevel = 10,
            requiredGold = 1000,
            description = "레벨 10 달성 시 아이템 강화가 가능해집니다."
        });
        
        // 레벨 20: 보스 던전 해금
        progressionGates.Add(new ProgressionGate
        {
            gateName = "보스 던전",
            requiredLevel = 20,
            requiredGold = 5000,
            description = "레벨 20 달성 시 보스 던전이 해금됩니다."
        });
        
        // 레벨 30: 전설 아이템 해금
        progressionGates.Add(new ProgressionGate
        {
            gateName = "전설 아이템",
            requiredLevel = 30,
            requiredGold = 10000,
            description = "레벨 30 달성 시 전설 아이템이 드롭됩니다."
        });
    }
    
    private void CreateDefaultDungeonTiers()
    {
        // 초급 던전 (레벨 1-10)
        dungeonTiers.Add(new DungeonTier
        {
            tierName = "초급 던전",
            minLevel = 1,
            maxLevel = 10,
            expMultiplier = 1f,
            goldMultiplier = 1f,
            rareItemChance = 0.05f,
            availableLootTables = new List<string> { "BasicLoot", "CommonLoot" },
            tierColor = Color.white
        });
        
        // 중급 던전 (레벨 11-25)
        dungeonTiers.Add(new DungeonTier
        {
            tierName = "중급 던전",
            minLevel = 11,
            maxLevel = 25,
            expMultiplier = 1.5f,
            goldMultiplier = 1.3f,
            rareItemChance = 0.15f,
            availableLootTables = new List<string> { "CommonLoot", "RareLoot" },
            tierColor = Color.green
        });
        
        // 고급 던전 (레벨 26-50)
        dungeonTiers.Add(new DungeonTier
        {
            tierName = "고급 던전",
            minLevel = 26,
            maxLevel = 50,
            expMultiplier = 2f,
            goldMultiplier = 1.8f,
            rareItemChance = 0.25f,
            availableLootTables = new List<string> { "RareLoot", "EpicLoot" },
            tierColor = Color.blue
        });
        
        // 전설 던전 (레벨 51+)
        dungeonTiers.Add(new DungeonTier
        {
            tierName = "전설 던전",
            minLevel = 51,
            maxLevel = 999,
            expMultiplier = 3f,
            goldMultiplier = 2.5f,
            rareItemChance = 0.4f,
            availableLootTables = new List<string> { "EpicLoot", "LegendaryLoot", "BossLoot" },
            tierColor = Color.magenta
        });
    }
    
    public void UpdateProgression()
    {
        if (playerStats == null) return;
        
        currentPlayerLevel = playerStats.level;
        
        // 게이트 해금 확인
        CheckGateUnlocks();
        
        // 던전 티어 업데이트
        UpdateDungeonTier();
    }
    
    private void CheckGateUnlocks()
    {
        foreach (ProgressionGate gate in progressionGates)
        {
            if (gate.isUnlocked || unlockedGates.Contains(gate.gateName)) continue;
            
            bool canUnlock = true;
            
            // 레벨 체크
            if (currentPlayerLevel < gate.requiredLevel)
                canUnlock = false;
            
            // 골드 체크
            if (playerStats.gold < gate.requiredGold)
                canUnlock = false;
            
            // 아이템 체크 (비활성화 - 인벤토리 시스템 제거됨)
            /*
            foreach (string requiredItemName in gate.requiredItems)
            {
                bool hasItem = false;
                // 인벤토리에서 아이템 이름으로 검색
                for (int i = 0; i < inventorySystem.slots.Count; i++)
                {
                    if (!inventorySystem.slots[i].IsEmpty() && 
                        inventorySystem.slots[i].item.itemName == requiredItemName)
                    {
                        hasItem = true;
                        break;
                    }
                }
                if (!hasItem)
                {
                    canUnlock = false;
                    break;
                }
            }
            */
            
            if (canUnlock)
            {
                UnlockGate(gate);
            }
        }
    }
    
    private void UnlockGate(ProgressionGate gate)
    {
        gate.isUnlocked = true;
        unlockedGates.Add(gate.gateName);
        
        // 보상 지급
        if (gate.goldReward > 0)
        {
            playerStats.gold += gate.goldReward;
        }
        
        // 보상 지급 (비활성화 - 인벤토리 시스템 제거됨)
        /*
        foreach (ItemData item in gate.itemRewards)
        {
            inventorySystem.AddItem(item, 1);
        }
        */
        
        OnGateUnlocked?.Invoke(gate);
        Debug.Log($"게이트 해금: {gate.gateName} - {gate.description}");
    }
    
    private void UpdateDungeonTier()
    {
        for (int i = 0; i < dungeonTiers.Count; i++)
        {
            if (currentPlayerLevel >= dungeonTiers[i].minLevel && 
                currentPlayerLevel <= dungeonTiers[i].maxLevel)
            {
                if (currentDungeonTier != i)
                {
                    currentDungeonTier = i;
                    OnDungeonTierUnlocked?.Invoke(dungeonTiers[i]);
                    Debug.Log($"던전 티어 업그레이드: {dungeonTiers[i].tierName}");
                }
                break;
            }
        }
    }
    
    public DungeonTier GetCurrentDungeonTier()
    {
        if (currentDungeonTier < dungeonTiers.Count)
            return dungeonTiers[currentDungeonTier];
        return dungeonTiers[dungeonTiers.Count - 1]; // 최고 티어 반환
    }
    
    public bool IsGateUnlocked(string gateName)
    {
        return unlockedGates.Contains(gateName);
    }
    
    private void LoadProgression()
    {
        // 저장된 진행도 로드 (PlayerPrefs 또는 JSON)
        currentPlayerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        currentDungeonTier = PlayerPrefs.GetInt("DungeonTier", 0);
        
        string unlockedGatesString = PlayerPrefs.GetString("UnlockedGates", "");
        if (!string.IsNullOrEmpty(unlockedGatesString))
        {
            unlockedGates = new List<string>(unlockedGatesString.Split(','));
        }
    }
    
    public void SaveProgression()
    {
        PlayerPrefs.SetInt("PlayerLevel", currentPlayerLevel);
        PlayerPrefs.SetInt("DungeonTier", currentDungeonTier);
        PlayerPrefs.SetString("UnlockedGates", string.Join(",", unlockedGates));
    }
}
