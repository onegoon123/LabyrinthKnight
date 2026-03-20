using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using DarkNaku.Number;

[System.Serializable]
public class GameSaveData
{
    public PlayerSaveData playerStats;
    public BattleSystemData battleSystem;
    public InventorySaveData inventory;
    public EquipmentSaveData equipment;
    public DateTime lastSaveTime;
    public string lastSaveTimeString; // DateTime 직렬화용
    public int craftingPoints; // 장비 제작 포인트
    public int saveVersion = 1;
}

[System.Serializable]
public class PlayerSaveData
{
    public string playerName;
    public int level;
    public int experience;
    public int experienceToNextLevel;
    
    // 업그레이드 레벨만 저장 (base 스탯과 growth 계수는 게임 설정이므로 저장하지 않음)
    public int upgradeLevelHealth;
    public int upgradeLevelAttack;
    public int upgradeLevelDefense;
    public int upgradeLevelAttackSpeed;
    public int upgradeLevelCritChance;
    public int upgradeLevelCritDamage;
    
    // 현재 상태
    public int currentHealth;
    public string gold;
    public int autoBattleLevel;
    public bool isAutoBattleEnabled;
    
    // 동료
    public string currentCompanionId;
    public List<CompanionSaveData> companions = new List<CompanionSaveData>();
}

[System.Serializable]
public class CompanionSaveData
{
    public string companionId;
    public bool isUnlocked;
    public int currentAffinity;
    public bool isStoryUnlocked;
}

[System.Serializable]
public class BattleSystemData
{
    public bool isAutoBattleActive;
    public int totalBattles;
    public int totalVictories;
    public int totalDefeats;
    public int currentEnemyLevel;
}

[System.Serializable]
public class InventorySaveData
{
    public List<ItemSlotSaveData> slots = new List<ItemSlotSaveData>();
}

[System.Serializable]
public class ItemSlotSaveData
{
    public ItemSaveData item;
    public int quantity;
}

[System.Serializable]
public class ItemSaveData
{
    public string itemId;
    public string itemName;
    public string description;
    public int itemType;
    public int rarity;
    public int equipmentSlot;
    public int itemLevel;
    public int value;
    public int stackSize;
    public bool isStackable;
    public bool isUsable;
    public bool isEquippable;
    
    // 스탯
    public int healthBonus;
    public int attackBonus;
    public int defenseBonus;
    public float attackSpeedBonus;
    public float moveSpeedBonus;
    public float critChanceBonus;
    public float critDamageBonus;
    public int goldBonus;
    public int healthRestore;
    public float duration;
    public int enhancementLevel;
}

[System.Serializable]
public class EquipmentSaveData
{
    public ItemSaveData weapon;
    public ItemSaveData helmet;
    public ItemSaveData armor;
    public ItemSaveData boots;
}

public class GameSaveSystem : MonoBehaviour
{
    [Header("저장 설정")]
    public string saveFileName = "idle_rpg_save.json";
    public float autoSaveInterval = 30f; // 자동 저장 간격 (초)
    
    [Header("시스템 참조")]
    public PlayerStats playerStats;
    public DungeonSystem dungeonSystem;
    // public InventorySystem inventorySystem; // 인벤토리 시스템 제거됨
    public EquipmentSystem equipmentSystem;
    public CompanionSystem companionSystem;
    public EquipmentCraftingSystem equipmentCraftingSystem;
    private float lastAutoSaveTime;
    
    public event Action OnGameSaved;
    public event Action OnGameLoaded;
    
    private void Start()
    {
        // Inspector에서 참조가 설정되지 않은 경우에만 자동으로 찾기
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
        if (dungeonSystem == null)
            dungeonSystem = FindFirstObjectByType<DungeonSystem>();
        // if (inventorySystem == null)
        //     inventorySystem = FindFirstObjectByType<InventorySystem>();
        if (equipmentSystem == null)
            equipmentSystem = FindFirstObjectByType<EquipmentSystem>();
        if (companionSystem == null)
            companionSystem = FindFirstObjectByType<CompanionSystem>();
        if (equipmentCraftingSystem == null)
            equipmentCraftingSystem = FindFirstObjectByType<EquipmentCraftingSystem>(FindObjectsInactive.Include);
        
        // 게임 시작 시 자동 로드
        LoadGame();
    }
    
    private void Update()
    {
        // 자동 저장
        if (Time.time - lastAutoSaveTime >= autoSaveInterval)
        {
            SaveGame();
            lastAutoSaveTime = Time.time;
        }
    }
    
    public void SaveGame()
    {
        if (playerStats == null || dungeonSystem == null) return;
        
        try
        {
            GameSaveData saveData = new GameSaveData
            {
                playerStats = new PlayerSaveData
                {
                    playerName = playerStats.playerName,
                    level = playerStats.level,
                    experience = playerStats.experience,
                    experienceToNextLevel = playerStats.experienceToNextLevel,
                    
                    // 업그레이드 레벨만 저장 (base 스탯과 growth 계수는 게임 설정이므로 저장하지 않음)
                    upgradeLevelHealth = playerStats.GetUpgradeLevel(UpgradeType.Health),
                    upgradeLevelAttack = playerStats.GetUpgradeLevel(UpgradeType.Attack),
                    upgradeLevelDefense = playerStats.GetUpgradeLevel(UpgradeType.Defense),
                    upgradeLevelAttackSpeed = playerStats.GetUpgradeLevel(UpgradeType.AttackSpeed),
                    upgradeLevelCritChance = playerStats.GetUpgradeLevel(UpgradeType.CritChance),
                    upgradeLevelCritDamage = playerStats.GetUpgradeLevel(UpgradeType.CritDamage),
                    
                    // 현재 상태 저장
                    currentHealth = playerStats.currentHealth,
                    gold = playerStats.gold.ToString(),
                    autoBattleLevel = playerStats.autoBattleLevel,
                    isAutoBattleEnabled = playerStats.isAutoBattleEnabled,
                    
                    // 동료 저장
                    currentCompanionId = companionSystem != null ? companionSystem.GetCurrentCompanionId() : "",
                    companions = companionSystem != null ? companionSystem.GetCompanionSaveDataList() : new List<CompanionSaveData>()
                },
                battleSystem = new BattleSystemData
                {
                    isAutoBattleActive = dungeonSystem.isDungeonActive,
                    totalBattles = dungeonSystem.totalBattles,
                    totalVictories = dungeonSystem.totalVictories,
                    totalDefeats = dungeonSystem.totalDefeats,
                    currentEnemyLevel = dungeonSystem.currentStage
                },
                inventory = SaveInventory(),
                equipment = SaveEquipment(),
                lastSaveTime = DateTime.Now,
                lastSaveTimeString = DateTime.Now.ToBinary().ToString(),
                craftingPoints = equipmentCraftingSystem != null ? equipmentCraftingSystem.GetCurrentPoints() : 5,
                saveVersion = 5
            };
            
            string json = JsonUtility.ToJson(saveData, true);
            string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            File.WriteAllText(filePath, json);
            
            Debug.Log($"게임 저장 완료: {filePath}");
            OnGameSaved?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 저장 실패: {e.Message}");
        }
    }
    
    public void LoadGame()
    {
        try
        {
            string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            if (!File.Exists(filePath))
            {
                Debug.Log("저장 파일이 없습니다. 새 게임을 시작합니다.");
                return;
            }
            
            string json = File.ReadAllText(filePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            
            if (saveData == null)
            {
                Debug.LogError("저장 데이터를 읽을 수 없습니다.");
                return;
            }
            
            // 플레이어 스탯 로드
            if (playerStats != null && saveData.playerStats != null)
            {
                if (!string.IsNullOrEmpty(saveData.playerStats.playerName))
                {
                    playerStats.playerName = saveData.playerStats.playerName;
                }
                playerStats.level = saveData.playerStats.level;
                playerStats.experience = saveData.playerStats.experience;
                playerStats.experienceToNextLevel = saveData.playerStats.experienceToNextLevel;
                
                // Base 스탯과 Growth 계수는 게임 설정이므로 로드하지 않음 (Inspector/CSV에서 설정된 값 사용)
                
                // 업그레이드 레벨 로드
                if (saveData.saveVersion >= 3)
                {
                    playerStats.SetUpgradeLevel(UpgradeType.Health, saveData.playerStats.upgradeLevelHealth);
                    playerStats.SetUpgradeLevel(UpgradeType.Attack, saveData.playerStats.upgradeLevelAttack);
                    playerStats.SetUpgradeLevel(UpgradeType.Defense, saveData.playerStats.upgradeLevelDefense);
                    playerStats.SetUpgradeLevel(UpgradeType.AttackSpeed, saveData.playerStats.upgradeLevelAttackSpeed);
                    playerStats.SetUpgradeLevel(UpgradeType.CritChance, saveData.playerStats.upgradeLevelCritChance);
                    playerStats.SetUpgradeLevel(UpgradeType.CritDamage, saveData.playerStats.upgradeLevelCritDamage);
                    
                    // UpgradeProgress는 이제 PlayerStats를 직접 참조하므로 자동으로 동기화됨
                }
                else
                {
                    // 이전 버전 호환성: 기존 저장 파일에서 업그레이드 레벨 복구 시도
                    // (기존 방식에서는 직접 스탯 값이 저장되었으므로 역계산 불가능)
                    // 새 게임으로 시작하거나 기본값 사용
                }
                
                // 현재 상태 로드
                playerStats.currentHealth = saveData.playerStats.currentHealth;
                if (!string.IsNullOrEmpty(saveData.playerStats.gold))
                {
                    playerStats.gold = new Number(saveData.playerStats.gold);
                }
                else
                {
                    playerStats.gold = Number.Zero;
                }
                playerStats.autoBattleLevel = saveData.playerStats.autoBattleLevel;
                playerStats.isAutoBattleEnabled = saveData.playerStats.isAutoBattleEnabled;
                
                playerStats.NotifyStatsChanged();
            }
            
            // 던전 시스템 로드
            if (dungeonSystem != null && saveData.battleSystem != null)
            {
                dungeonSystem.isDungeonActive = saveData.battleSystem.isAutoBattleActive;
                dungeonSystem.totalBattles = saveData.battleSystem.totalBattles;
                dungeonSystem.totalVictories = saveData.battleSystem.totalVictories;
                dungeonSystem.totalDefeats = saveData.battleSystem.totalDefeats;
                dungeonSystem.currentStage = saveData.battleSystem.currentEnemyLevel;
                
                if (saveData.battleSystem.isAutoBattleActive)
                {
                    dungeonSystem.StartDungeon();
                }
            }
            
            // 인벤토리 로드 (비활성화 - 인벤토리 시스템 제거됨)
            if (saveData.saveVersion >= 4)
            {
                // LoadInventory(saveData.inventory); // 비활성화
                LoadEquipment(saveData.equipment);
            }

            // 제작 포인트 로드 (오프라인 보상 포함)
            if (equipmentCraftingSystem != null && saveData.saveVersion >= 5)
            {
                equipmentCraftingSystem.SetCurrentPoints(saveData.craftingPoints);
                
                if (!string.IsNullOrEmpty(saveData.lastSaveTimeString))
                {
                    try
                    {
                        long binaryTime = Convert.ToInt64(saveData.lastSaveTimeString);
                        DateTime lastTime = DateTime.FromBinary(binaryTime);
                        TimeSpan elapsed = DateTime.Now - lastTime;
                        
                        if (elapsed.TotalSeconds > 0)
                        {
                            equipmentCraftingSystem.SimulateTimePassing((float)elapsed.TotalSeconds);
                        }
                    }
                    catch { }
                }
            }
            
            // 동료 로드
            if (companionSystem != null)
            {
                // 저장된 동료 데이터 복원
                if (saveData.playerStats.companions != null)
                {
                    companionSystem.LoadCompanionSaveData(saveData.playerStats.companions);
                }

                if (!string.IsNullOrEmpty(saveData.playerStats.currentCompanionId))
                {
                    companionSystem.SpawnCompanionById(saveData.playerStats.currentCompanionId);
                }
            }
            
            Debug.Log($"게임 로드 완료: {saveData.lastSaveTime}");
            OnGameLoaded?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 로드 실패: {e.Message}");
        }
    }

    [ContextMenu("Delete Save")]
    public void DeleteSave()
    {
        try
        {
            string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"저장 파일이 삭제되었습니다: {filePath}");
            }
            else
            {
                Debug.Log($"저장 파일이 존재하지 않습니다: {filePath}");
            }
            
            // 게임 상태 초기화
            ResetGameState();
        }
        catch (Exception e)
        {
            Debug.LogError($"저장 파일 삭제 실패: {e.Message}");
        }
    }
    
    private void ResetGameState()
    {
        // 플레이어 스탯 초기화
        if (playerStats != null)
        {
            playerStats.level = 1;
            playerStats.experience = 0;
            playerStats.experienceToNextLevel = 100;
            playerStats.currentHealth = playerStats.maxHealth;
            playerStats.playerName = "기사";
            playerStats.gold = Number.Zero;
            playerStats.autoBattleLevel = 1;
            playerStats.isAutoBattleEnabled = false;
            
            // 업그레이드 레벨 초기화
            playerStats.SetUpgradeLevel(UpgradeType.Health, 0);
            playerStats.SetUpgradeLevel(UpgradeType.Attack, 0);
            playerStats.SetUpgradeLevel(UpgradeType.Defense, 0);
            playerStats.SetUpgradeLevel(UpgradeType.AttackSpeed, 0);
            playerStats.SetUpgradeLevel(UpgradeType.CritChance, 0);
            playerStats.SetUpgradeLevel(UpgradeType.CritDamage, 0);
            
            // UpgradeProgress도 초기화 (PlayerStats를 통해 관리되므로 자동으로 동기화됨)
            
            playerStats.NotifyStatsChanged();
        }
        
        // 던전 시스템 초기화
        if (dungeonSystem != null)
        {
            dungeonSystem.isDungeonActive = false;
            dungeonSystem.totalBattles = 0;
            dungeonSystem.totalVictories = 0;
            dungeonSystem.totalDefeats = 0;
            dungeonSystem.currentStage = 1;
        }
        
        // 인벤토리 초기화 (비활성화 - 인벤토리 시스템 제거됨)
        /*
        if (inventorySystem != null)
        {
            inventorySystem.ClearInventory();
        }
        */
        
        // 장비 초기화
        if (equipmentSystem != null)
        {
            equipmentSystem.UnequipAll(false);
        }
        
        Debug.Log("게임 상태가 초기화되었습니다.");
    }
    
    public bool HasSaveFile()
    {
        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
        return File.Exists(filePath);
    }
    
    public DateTime GetLastSaveTime()
    {
        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
        
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                return saveData.lastSaveTime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
        
        return DateTime.MinValue;
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationQuit()
    {
        SaveGame();
    }
    
    // 인벤토리 저장 (비활성화 - 인벤토리 시스템 제거됨)
    private InventorySaveData SaveInventory()
    {
        InventorySaveData inventoryData = new InventorySaveData();
        
        /*
        if (inventorySystem != null)
        {
            foreach (var slot in inventorySystem.slots)
            {
                if (!slot.IsEmpty() && slot.item != null)
                {
                    inventoryData.slots.Add(new ItemSlotSaveData
                    {
                        item = ConvertItemToSaveData(slot.item),
                        quantity = slot.quantity
                    });
                }
            }
        }
        */
        
        return inventoryData;
    }
    
    // 장비 저장
    private EquipmentSaveData SaveEquipment()
    {
        EquipmentSaveData equipmentData = new EquipmentSaveData();
        
        if (equipmentSystem != null)
        {
            equipmentData.weapon = equipmentSystem.equippedWeapon != null ? ConvertItemToSaveData(equipmentSystem.equippedWeapon) : null;
            equipmentData.helmet = equipmentSystem.equippedHelmet != null ? ConvertItemToSaveData(equipmentSystem.equippedHelmet) : null;
            equipmentData.armor = equipmentSystem.equippedArmor != null ? ConvertItemToSaveData(equipmentSystem.equippedArmor) : null;
            equipmentData.boots = equipmentSystem.equippedBoots != null ? ConvertItemToSaveData(equipmentSystem.equippedBoots) : null;
        }
        
        return equipmentData;
    }
    
    // ItemData를 ItemSaveData로 변환
    private ItemSaveData ConvertItemToSaveData(ItemData item)
    {
        if (item == null) return null;
        
        return new ItemSaveData
        {
            itemId = item.itemId,
            itemName = item.itemName,
            description = item.description,
            itemType = (int)item.itemType,
            rarity = (int)item.rarity,
            equipmentSlot = (int)item.equipmentSlot,
            itemLevel = item.itemLevel,
            value = item.value,
            stackSize = item.stackSize,
            isStackable = item.isStackable,
            isUsable = item.isUsable,
            isEquippable = item.isEquippable,
            healthBonus = item.healthBonus,
            attackBonus = item.attackBonus,
            defenseBonus = item.defenseBonus,
            attackSpeedBonus = item.attackSpeedBonus,
            moveSpeedBonus = item.moveSpeedBonus,
            critChanceBonus = item.critChanceBonus,
            critDamageBonus = item.critDamageBonus,
            goldBonus = item.goldBonus,
            healthRestore = item.healthRestore,
            duration = item.duration,
            enhancementLevel = item.enhancementLevel
        };
    }
    
    // ItemSaveData를 ItemData로 변환
    private ItemData ConvertSaveDataToItem(ItemSaveData saveData)
    {
        // ... (existing code, not showing here)
        if (saveData == null) return null;
        
        ItemData item = new ItemData
        {
            itemId = saveData.itemId,
            itemName = saveData.itemName,
            description = saveData.description,
            itemType = (ItemType)saveData.itemType,
            rarity = (ItemRarity)saveData.rarity,
            equipmentSlot = (EquipmentSlot)saveData.equipmentSlot,
            itemLevel = saveData.itemLevel,
            value = saveData.value,
            stackSize = saveData.stackSize,
            isStackable = saveData.isStackable,
            isUsable = saveData.isUsable,
            isEquippable = saveData.isEquippable,
            healthBonus = saveData.healthBonus,
            attackBonus = saveData.attackBonus,
            defenseBonus = saveData.defenseBonus,
            attackSpeedBonus = saveData.attackSpeedBonus,
            moveSpeedBonus = saveData.moveSpeedBonus,
            critChanceBonus = saveData.critChanceBonus,
            critDamageBonus = saveData.critDamageBonus,
            goldBonus = saveData.goldBonus,
            healthRestore = saveData.healthRestore,
            duration = saveData.duration,
            enhancementLevel = saveData.enhancementLevel
        };
        
        // 아이콘 복원 (RandomItemGenerator에서)
        RestoreItemIcon(item);
        
        return item;
    }

    /// <summary>
    /// PlayerStats 참조 없이 새로운 저장 파일을 생성합니다. (타이틀 화면용)
    /// </summary>
    public void CreateNewSaveFile(string playerName)
    {
        GameSaveData newDetails = new GameSaveData
        {
            playerStats = new PlayerSaveData
            {
                playerName = playerName,
                level = 1,
                experience = 0,
                experienceToNextLevel = 100,
                
                upgradeLevelHealth = 0,
                upgradeLevelAttack = 0,
                upgradeLevelDefense = 0,
                upgradeLevelAttackSpeed = 0,
                upgradeLevelCritChance = 0,
                upgradeLevelCritDamage = 0,
                
                currentHealth = 150, // 게임 시작 시 초기화됨
                gold = "0",
                autoBattleLevel = 1,
                isAutoBattleEnabled = true,
                
                companions = new List<CompanionSaveData>()
            },
            battleSystem = new BattleSystemData
            {
                isAutoBattleActive = true,
                totalBattles = 0,
                totalVictories = 0,
                totalDefeats = 0,
                currentEnemyLevel = 1
            },
            inventory = new InventorySaveData(),
            equipment = new EquipmentSaveData(),
            lastSaveTime = DateTime.Now,
            lastSaveTimeString = DateTime.Now.ToBinary().ToString(),
            craftingPoints = 5,
            saveVersion = 5
        };
        
        try 
        {
            string json = JsonUtility.ToJson(newDetails, true);
            string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
            File.WriteAllText(filePath, json);
            Debug.Log($"새 게임(닉네임: {playerName}) 저장 파일 생성 완료: {filePath}");
            
            // 저장 완료 이벤트 발생
            OnGameSaved?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"새 게임 파일 생성 실패: {e.Message}");
        }
    }
    
    // 아이템 아이콘 복원
    private void RestoreItemIcon(ItemData item)
    {
        if (item == null || item.equipmentSlot == EquipmentSlot.None) return;
        
        EquipmentCraftingSystem craftingSystem = FindFirstObjectByType<EquipmentCraftingSystem>(FindObjectsInactive.Include);
        if (craftingSystem == null) 
        {
            Debug.LogWarning("EquipmentCraftingSystem not found, cannot restore item icon.");
            return;
        }
        
        // EquipmentCraftingSystem의 아이콘 배열 사용
        Sprite[] iconArray = null;
        
        switch (item.equipmentSlot)
        {
            case EquipmentSlot.Weapon:
                iconArray = craftingSystem.weaponIcons;
                break;
            case EquipmentSlot.Helmet:
                iconArray = craftingSystem.helmetIcons;
                break;
            case EquipmentSlot.Armor:
                iconArray = craftingSystem.armorIcons;
                break;
            case EquipmentSlot.Boots:
                iconArray = craftingSystem.bootsIcons;
                break;
        }
        
        if (iconArray != null && iconArray.Length > 0)
        {
            // null이 아닌 아이콘만 필터링
            System.Collections.Generic.List<Sprite> validIcons = new System.Collections.Generic.List<Sprite>();
            foreach (Sprite icon in iconArray)
            {
                if (icon != null)
                {
                    validIcons.Add(icon);
                }
            }
            
            if (validIcons.Count > 0)
            {
                // 같은 아이템은 같은 아이콘을 유지하기 위해 itemId를 시드로 사용
                System.Random random = new System.Random(item.itemId.GetHashCode());
                item.icon = validIcons[random.Next(validIcons.Count)];
            }
        }
    }
    
    // 인벤토리 로드 (비활성화 - 인벤토리 시스템 제거됨)
    private void LoadInventory(InventorySaveData inventoryData)
    {
        /*
        if (inventorySystem == null || inventoryData == null) return;
        
        // 인벤토리 초기화
        inventorySystem.ClearInventory();
        
        // 아이템 로드
        foreach (var slotData in inventoryData.slots)
        {
            if (slotData.item != null)
            {
                ItemData item = ConvertSaveDataToItem(slotData.item);
                if (item != null)
                {
                    inventorySystem.AddItem(item, slotData.quantity);
                }
            }
        }
        */
    }
    
    // 장비 로드
    private void LoadEquipment(EquipmentSaveData equipmentData)
    {
        if (equipmentSystem == null || equipmentData == null) return;
        
        // 기존 장비 해제
        if (equipmentSystem.equippedWeapon != null)
            equipmentSystem.UnequipItem(EquipmentSlot.Weapon, false);
        if (equipmentSystem.equippedHelmet != null)
            equipmentSystem.UnequipItem(EquipmentSlot.Helmet, false);
        if (equipmentSystem.equippedArmor != null)
            equipmentSystem.UnequipItem(EquipmentSlot.Armor, false);
        if (equipmentSystem.equippedBoots != null)
            equipmentSystem.UnequipItem(EquipmentSlot.Boots, false);
        
        // 장비 로드 (검증 후 장착, 경고 억제)
        if (equipmentData.weapon != null)
        {
            ItemData item = ConvertSaveDataToItem(equipmentData.weapon);
            if (item != null && item.isEquippable && item.equipmentSlot == EquipmentSlot.Weapon)
            {
                equipmentSystem.EquipItem(item, suppressWarning: true);
            }
        }
        if (equipmentData.helmet != null)
        {
            ItemData item = ConvertSaveDataToItem(equipmentData.helmet);
            if (item != null && item.isEquippable && item.equipmentSlot == EquipmentSlot.Helmet)
            {
                equipmentSystem.EquipItem(item, suppressWarning: true);
            }
        }
        if (equipmentData.armor != null)
        {
            ItemData item = ConvertSaveDataToItem(equipmentData.armor);
            if (item != null && item.isEquippable && item.equipmentSlot == EquipmentSlot.Armor)
            {
                equipmentSystem.EquipItem(item, suppressWarning: true);
            }
        }
        if (equipmentData.boots != null)
        {
            ItemData item = ConvertSaveDataToItem(equipmentData.boots);
            if (item != null && item.isEquippable && item.equipmentSlot == EquipmentSlot.Boots)
            {
                equipmentSystem.EquipItem(item, suppressWarning: true);
            }
        }
    }
    
}
