using System; // Added for DateTime, TimeSpan, Convert
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class EquipmentCraftingSystem : MonoBehaviour
{
    [Header("시스템 설정")]
    public int maxPoints = 5;
    public float regenInterval = 60f; // 1 포인트 회복 시간 (초)
    
    [Header("UI 참조")]
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI timerText;
    public Button generateButton;
    public Slider regenSlider; // 슬라이더 추가

    [Header("아이템 생성 설정")]
    [Tooltip("레벨당 스탯 증가량")]
    public float statPerLevel = 10f;
    [Tooltip("스탯 최소 개수")]
    public int minStats = 1;
    [Tooltip("스탯 최대 개수")]
    public int maxStats = 4;

    [Header("시스템 상태")]
    [SerializeField] private int currentPoints;
    [SerializeField] private float regenTimer;

    [Header("외부 참조")]
    public PlayerStats playerStats;
    public ItemAcquisitionPopup itemAcquisitionPopup; // 팝업 참조 추가

    [Header("아이콘 설정")]
    public Sprite[] weaponIcons;
    public Sprite[] helmetIcons;
    public Sprite[] armorIcons;
    public Sprite[] bootsIcons;

    [Header("희귀도 확률 (누적 값)")]
    [Range(0f, 1f)] public float commonChance = 0.5f;
    [Range(0f, 1f)] public float uncommonChance = 0.75f;
    [Range(0f, 1f)] public float rareChance = 0.9f;
    [Range(0f, 1f)] public float epicChance = 0.98f;

    [Header("희귀도 배수")]
    public float commonMultiplier = 1f;
    public float uncommonMultiplier = 1.2f;
    public float rareMultiplier = 1.5f;
    public float epicMultiplier = 2f;
    public float legendaryMultiplier = 3f;

    private enum StatType
    {
        Health,
        Attack,
        Defense,
        AttackSpeed,
        MoveSpeed,
        CritChance,
        CritDamage
    }

#if UNITY_2023_1_OR_NEWER
    private static T FindService<T>() where T : UnityEngine.Object
    {
        return FindFirstObjectByType<T>();
    }
#else
    private static T FindService<T>() where T : UnityEngine.Object
    {
        return FindObjectOfType<T>();
    }
#endif

    private const string LastUpdateTimeKey = "Crafting_LastUpdateTime";

    private void Start()
    {
        InitializeSystem();
        InitializeUI();
    }
    
    private void OnEnable()
    {
        if (PlayerPrefs.HasKey(LastUpdateTimeKey))
        {
            long temp = Convert.ToInt64(PlayerPrefs.GetString(LastUpdateTimeKey));
            DateTime lastTime = DateTime.FromBinary(temp);
            TimeSpan elapsed = DateTime.Now - lastTime;
            
            if (elapsed.TotalSeconds > 0)
            {
                SimulateTimePassing((float)elapsed.TotalSeconds);
            }
        }
    }

    private void OnDisable()
    {
        PlayerPrefs.SetString(LastUpdateTimeKey, DateTime.Now.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetString(LastUpdateTimeKey, DateTime.Now.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    public void SimulateTimePassing(float seconds)
    {
        if (currentPoints >= maxPoints) return;
        
        Debug.Log($"[Crafting] Offline/Background time elapsed: {seconds:F1}s");
        regenTimer -= seconds;
        
        while (regenTimer <= 0f && currentPoints < maxPoints)
        {
            currentPoints++;
            regenTimer += regenInterval;
        }
        
        if (currentPoints >= maxPoints)
        {
            regenTimer = regenInterval;
        }
    }

    private void Update()
    {
        UpdatePoints();
        UpdateUI();
    }

    private void InitializeSystem()
    {
        if (playerStats == null)
        {
            playerStats = FindService<PlayerStats>();
        }

        maxPoints = Mathf.Max(1, maxPoints);
        
        // 초기 실행 시 (앱 재시작 등) 포인트가 0이면 꽉 채워줌 (정책에 따라 변경 가능)
        // 단, OnEnable이 Start보다 먼저 실행되므로, 거기서 포인트가 찼다면 초기화 안 함.
        if (currentPoints <= 0)
        {
            currentPoints = maxPoints;
        }
        else
        {
            currentPoints = Mathf.Clamp(currentPoints, 0, maxPoints);
        }

        if (regenTimer <= 0) regenTimer = regenInterval;
    }

    private void InitializeUI()
    {
        if (generateButton != null)
        {
            generateButton.onClick.RemoveListener(GenerateEquipment);
            generateButton.onClick.AddListener(GenerateEquipment);
        }

        if (regenSlider != null)
        {
            regenSlider.minValue = 0f;
            regenSlider.maxValue = 1f;
        }

        UpdateUI();
    }

    private void UpdatePoints()
    {
        if (regenInterval <= 0f)
        {
            currentPoints = maxPoints;
            regenTimer = 0f;
            return;
        }

        if (currentPoints >= maxPoints)
        {
            regenTimer = regenInterval;
            return;
        }

        regenTimer -= Time.deltaTime;

        while (regenTimer <= 0f && currentPoints < maxPoints)
        {
            currentPoints++;
            regenTimer += regenInterval;
        }
    }

    private void UpdateUI()
    {
        // 포인트 텍스트
        if (pointsText != null)
        {
            pointsText.text = $"{currentPoints}/{maxPoints}";
        }

        // 타이머 텍스트 및 슬라이더
        if (currentPoints >= maxPoints)
        {
            if (timerText != null) timerText.text = "MAX";
            if (regenSlider != null) regenSlider.value = 1f;
        }
        else
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(regenTimer / 60F);
                int seconds = Mathf.FloorToInt(regenTimer % 60F);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
            
            if (regenSlider != null)
            {
                // 남은 시간이 줄어들수록 슬라이더가 참 (0 -> 1)
                float progress = 1f - (regenTimer / regenInterval);
                regenSlider.value = progress;
            }
        }

        // 버튼 활성화 상태
        if (generateButton != null)
        {
            generateButton.interactable = currentPoints > 0;
        }
    }

    public void GenerateEquipment()
    {
        if (currentPoints <= 0)
        {
            Debug.Log("포인트가 부족합니다.");
            return;
        }

        // 포인트 소모
        currentPoints--;
        
        // 타이머가 멈춰있었다면(최대치였으면) 다시 시작
        if (currentPoints == maxPoints - 1 && regenTimer >= regenInterval)
        {
            regenTimer = regenInterval;
        }

        // 아이템 생성 로직
        CreateRandomEquipment();
    }
    
    // 저장 시스템 연동을 위한 getter/setter
    public int GetCurrentPoints()
    {
        return currentPoints;
    }
    
    public void SetCurrentPoints(int points)
    {
        currentPoints = points;
        // UI 갱신 등 추가 로직 필요 시 여기 작성
        UpdateUI();
    }

    private void CreateRandomEquipment()
    {
        // 랜덤 부위 선택
        EquipmentSlot[] slots = { EquipmentSlot.Weapon, EquipmentSlot.Helmet, EquipmentSlot.Armor, EquipmentSlot.Boots };
        EquipmentSlot randomSlot = slots[Random.Range(0, slots.Length)];

        // 플레이어 레벨 기반 아이템 생성
        int level = playerStats != null ? playerStats.level : 1;
        ItemData newItem = GenerateRandomEquipmentData(randomSlot, level);

        if (newItem != null)
        {
            Debug.Log($"장비 생성 완료: {newItem.itemName}");
            
            // 팝업 표시 (인벤토리에 추가하지 않음)
            if (itemAcquisitionPopup != null)
            {
                itemAcquisitionPopup.ShowItemPopup(newItem);
            }
            else
            {
                Debug.LogError("[EquipmentCraftingSystem] ItemAcquisitionPopup is null! Cannot show popup.");
            }
        }
    }

    // --- Random Item Generator Logic ---

    /// <summary>
    /// 랜덤 장비 아이템을 생성합니다.
    /// </summary>
    /// <param name="equipmentSlot">생성할 장비 슬롯 (무기, 투구, 갑옷, 신발)</param>
    /// <param name="itemLevel">아이템 레벨 (플레이어 레벨 기반)</param>
    /// <returns>생성된 ItemData 객체</returns>
    public ItemData GenerateRandomEquipmentData(EquipmentSlot equipmentSlot, int itemLevel)
    {
        if (equipmentSlot == EquipmentSlot.None) return null;
        
        // 1. 기본 아이템 데이터 생성
        ItemData item = new ItemData();
        item.equipmentSlot = equipmentSlot;
        item.itemLevel = itemLevel;
        item.isEquippable = true;
        item.isStackable = false;
        item.stackSize = 1;
        
        // 2. 희귀도 결정 (확률 기반)
        item.rarity = DetermineRarity();
        // 3. 희귀도에 따른 스텟 배율 가져오기
        float rarityMultiplier = GetRarityMultiplier(item.rarity);
        
        // 4. 장비 타입에 맞는 랜덤 아이콘 설정
        SetEquipmentIcon(item, equipmentSlot);
        
        // 5. 랜덤 스텟 생성 (레벨 * 희귀도 배율)
        GenerateRandomStats(item, itemLevel, rarityMultiplier);
        
        // 6. 아이템 이름 및 설명 생성
        item.itemName = GenerateItemName(equipmentSlot, item.rarity);
        item.description = GenerateItemDescription(item);
        
        // 7. 아이템 가치 계산
        item.value = CalculateItemValue(item, itemLevel);
        
        return item;
    }

    /// <summary>
    /// 확률 기반으로 희귀도를 결정합니다.
    /// Inspector에서 설정한 누적 확률을 사용합니다.
    /// </summary>
    private ItemRarity DetermineRarity()
    {
        // 0.0 ~ 1.0 사이의 랜덤 값 생성
        float roll = Random.Range(0f, 1f);
        
        // 누적 확률로 희귀도 결정
        // 예: commonChance=0.5, uncommonChance=0.75, rareChance=0.9, epicChance=0.98
        // roll이 0.3이면 Common, 0.6이면 Uncommon, 0.95면 Epic
        if (roll <= commonChance) return ItemRarity.Common;
        else if (roll <= uncommonChance) return ItemRarity.Uncommon;
        else if (roll <= rareChance) return ItemRarity.Rare;
        else if (roll <= epicChance) return ItemRarity.Epic;
        else return ItemRarity.Legendary;
    }

    /// <summary>
    /// 희귀도에 따른 스텟 배율을 반환합니다.
    /// Inspector에서 설정한 값을 사용합니다.
    /// </summary>
    private float GetRarityMultiplier(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return commonMultiplier;      // 기본 1.0
            case ItemRarity.Uncommon: return uncommonMultiplier;  // 기본 1.2
            case ItemRarity.Rare: return rareMultiplier;          // 기본 1.5
            case ItemRarity.Epic: return epicMultiplier;          // 기본 2.0
            case ItemRarity.Legendary: return legendaryMultiplier; // 기본 3.0
            default: return 1f;
        }
    }

    private void SetEquipmentIcon(ItemData item, EquipmentSlot slot)
    {
        Sprite[] iconArray = null;

        switch (slot)
        {
            case EquipmentSlot.Weapon: iconArray = weaponIcons; break;
            case EquipmentSlot.Helmet: iconArray = helmetIcons; break;
            case EquipmentSlot.Armor: iconArray = armorIcons; break;
            case EquipmentSlot.Boots: iconArray = bootsIcons; break;
        }

        if (iconArray == null || iconArray.Length == 0)
        {
            return;
        }

        List<Sprite> validIcons = new List<Sprite>();
        foreach (Sprite icon in iconArray)
        {
            if (icon != null)
            {
                validIcons.Add(icon);
            }
        }

        if (validIcons.Count > 0)
        {
            item.icon = validIcons[Random.Range(0, validIcons.Count)];
        }
    }

    /// <summary>
    /// 랜덤 스텟을 생성하여 아이템에 적용합니다.
    /// </summary>
    /// <param name="item">스텟을 적용할 아이템</param>
    /// <param name="level">아이템 레벨</param>
    /// <param name="multiplier">희귀도 배율</param>
    private void GenerateRandomStats(ItemData item, int level, float multiplier)
    {
        // 1. 생성할 스텟 개수 결정 (minStats ~ maxStats)
        int statCount = Random.Range(minStats, maxStats + 1);
        
        // 2. 선택 가능한 모든 스텟 타입 리스트
        List<StatType> availableStats = new List<StatType>
        {
            StatType.Health,      // 체력
            StatType.Attack,      // 공격력
            StatType.Defense,     // 방어력
            StatType.AttackSpeed, // 공격 속도
            StatType.MoveSpeed,   // 이동 속도
            StatType.CritChance,  // 치명타 확률
            StatType.CritDamage   // 치명타 피해
        };
        
        // 3. 랜덤으로 스텟 선택 및 적용 (중복 없음)
        for (int i = 0; i < statCount && availableStats.Count > 0; i++)
        {
            // 랜덤하게 스텟 타입 선택
            int randomIndex = Random.Range(0, availableStats.Count);
            StatType selectedStat = availableStats[randomIndex];
            // 선택된 스텟은 리스트에서 제거 (중복 방지)
            availableStats.RemoveAt(randomIndex);
            
            // 기본 스텟 값 계산: 레벨 * 레벨당 스텟 * 희귀도 배율
            // 예: 레벨 10, statPerLevel 10, multiplier 1.5 => baseStat = 150
            float baseStat = level * statPerLevel * multiplier;
            
            // 선택된 스텟에 값 적용
            ApplyStat(item, selectedStat, baseStat);
        }
    }

    /// <summary>
    /// 선택된 스텟 타입에 따라 아이템에 스텟 값을 적용합니다.
    /// </summary>
    /// <param name="item">스텟을 적용할 아이템</param>
    /// <param name="statType">스텟 타입</param>
    /// <param name="baseValue">기본 스텟 값</param>
    private void ApplyStat(ItemData item, StatType statType, float baseValue)
    {
        switch (statType)
        {
            // 정수 스텟: 반올림하여 적용
            case StatType.Health: 
                item.healthBonus += Mathf.RoundToInt(baseValue); 
                break;
            case StatType.Attack: 
                item.attackBonus += Mathf.RoundToInt(baseValue); 
                break;
            case StatType.Defense: 
                item.defenseBonus += Mathf.RoundToInt(baseValue); 
                break;
            
            // 퍼센트 스텟: baseValue의 1%로 변환 후 소수점 1자리 반올림
            case StatType.AttackSpeed: 
                // 예: 1.23 -> 1.2
                // 부동소수점 오차 방지를 위해 System.Math.Round 사용
                float atkSpeed = Mathf.Round(baseValue * 0.1f) * 0.1f;
                item.attackSpeedBonus += (float)System.Math.Round(atkSpeed, 2); 
                break;
            case StatType.MoveSpeed: 
                float moveSpeed = Mathf.Round(baseValue * 0.1f) * 0.1f;
                item.moveSpeedBonus += (float)System.Math.Round(moveSpeed, 2);
                break;
            case StatType.CritDamage: 
                float critDmg = Mathf.Round(baseValue * 0.1f) * 0.1f;
                item.critDamageBonus += (float)System.Math.Round(critDmg, 2);
                break;
            
            // 치명타 확률: baseValue의 0.1%로 변환. % 표기 시 소수점 1자리 유지(0.001 단위)
            case StatType.CritChance: 
                // 예: 12.34% -> 12.3% (0.123)
                float critChance = Mathf.Round(baseValue) * 0.001f;
                item.critChanceBonus += (float)System.Math.Round(critChance, 4); 
                break;
        }
    }

    private string GenerateItemName(EquipmentSlot slot, ItemRarity rarity)
    {
        string rarityPrefix = "";
        switch (rarity)
        {
            case ItemRarity.Common: rarityPrefix = "일반"; break;
            case ItemRarity.Uncommon: rarityPrefix = "고급"; break;
            case ItemRarity.Rare: rarityPrefix = "희귀"; break;
            case ItemRarity.Epic: rarityPrefix = "영웅"; break;
            case ItemRarity.Legendary: rarityPrefix = "전설"; break;
        }
        
        string slotName = "";
        switch (slot)
        {
            case EquipmentSlot.Weapon: slotName = "무기"; break;
            case EquipmentSlot.Helmet: slotName = "투구"; break;
            case EquipmentSlot.Armor: slotName = "갑옷"; break;
            case EquipmentSlot.Boots: slotName = "신발"; break;
            default: slotName = "장비"; break;
        }
        
        return $"{rarityPrefix} {slotName}";
    }

    private string GenerateItemDescription(ItemData item)
    {
        System.Text.StringBuilder desc = new System.Text.StringBuilder();
        if (item.healthBonus > 0) desc.Append($"체력 +{item.healthBonus} ");
        if (item.attackBonus > 0) desc.Append($"공격력 +{item.attackBonus} ");
        if (item.defenseBonus > 0) desc.Append($"방어력 +{item.defenseBonus} ");
        if (item.attackSpeedBonus > 0) desc.Append($"공격속도 +{item.attackSpeedBonus:F2} ");
        if (item.moveSpeedBonus > 0) desc.Append($"이동속도 +{item.moveSpeedBonus:F2} ");
        if (item.critChanceBonus > 0) desc.Append($"치명타 확률 +{item.critChanceBonus:P1} ");
        if (item.critDamageBonus > 0) desc.Append($"치명타 데미지 +{item.critDamageBonus:F2} ");
        return desc.ToString().Trim();
    }

    private int CalculateItemValue(ItemData item, int level)
    {
        int baseValue = level * 10;
        float rarityMultiplier = GetRarityMultiplier(item.rarity);
        return Mathf.RoundToInt(baseValue * rarityMultiplier);
    }
}
