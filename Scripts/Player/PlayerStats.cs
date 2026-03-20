using System;
using UnityEngine;
using DarkNaku.Number;

[System.Serializable]
public class PlayerStats : MonoBehaviour, ICombatTarget
{
    public string playerName = "기사";

    [Header("캐릭터 데이터")]
    public CharacterStatsData characterStats;

    [Header("기본 스탯")]
    public int level = 1;
    public int experience = 0;
    public int experienceToNextLevel = 100;
    
    [Header("업그레이드 레벨")]
    private int upgradeLevelHealth = 0;
    private int upgradeLevelAttack = 0;
    private int upgradeLevelDefense = 0;
    private int upgradeLevelAttackSpeed = 0;
    private int upgradeLevelCritChance = 0;
    private int upgradeLevelCritDamage = 0;
    
    [Header("장비 시스템 참조")]
    public EquipmentSystem equipmentSystem;
    
    [Header("현재 전투 스탯 (Calculated)")]
    public int maxHealth { get { return GetCalculatedStat(UpgradeType.Health); } }
    public int currentHealth = 150;
    
    public int attackPower { get { return GetCalculatedStat(UpgradeType.Attack); } }
    public int defense { get { return GetCalculatedStat(UpgradeType.Defense); } }
    public float attackSpeed { get { return GetCalculatedStatFloat(UpgradeType.AttackSpeed); } }
    
    [Header("치명타 스탯 (Calculated)")]
    public float critChance { get { return GetCalculatedStatFloat(UpgradeType.CritChance); } }
    public float critDamage { get { return GetCalculatedStatFloat(UpgradeType.CritDamage); } }
    
    // 이동속도 (장비만 적용)
    public float moveSpeed { get { return characterStats != null ? characterStats.moveSpeed + GetEquipmentMoveSpeedBonus() : 3f; } }
    
    [Header("방치형 관련")]
    public Number gold = Number.Zero;
    public int autoBattleLevel = 1;
    public bool isAutoBattleEnabled = true;
    
    public event Action OnLevelUp;
    public event Action OnStatsChanged;
    public event Action<int> OnDamageTaken;
    public event Action OnDeath;
    
    [Header("HP바")]
    public HealthBar healthBar;
    
    private void Awake()
    {
        // 기본값으로 초기화 (저장 파일이 있으면 덮어씌워짐)
        InitializeDefaultStats();
        
        // HP바 초기화
        InitializeHealthBar();
    }
    
    private void InitializeHealthBar()
    {
        // HealthBar 컴포넌트가 없으면 추가
        if (healthBar == null)
        {
            GameObject healthBarObj = new GameObject("HealthBar");
            healthBarObj.transform.SetParent(transform);
            healthBar = healthBarObj.AddComponent<HealthBar>();
        }
        
        // HP바 초기화
        if (healthBar != null)
        {
            healthBar.Initialize(transform, maxHealth, currentHealth);
        }
    }
    
    private void InitializeDefaultStats()
    {
        currentHealth = maxHealth;
    }
    
    public void AddExperience(int exp)
    {
        experience += exp;
        CheckLevelUp();
        OnStatsChanged?.Invoke();
    }
    
    private void CheckLevelUp()
    {
        int maxLevelUps = 10; // 최대 레벨업 횟수 제한 (무한 루프 방지)
        int levelUpCount = 0;
        
        while (experience >= experienceToNextLevel && levelUpCount < maxLevelUps)
        {
            LevelUp();
            levelUpCount++;
        }
        
        // 무한 루프 감지 시 경고
        if (levelUpCount >= maxLevelUps)
        {
            Debug.LogWarning("CheckLevelUp: 최대 레벨업 횟수 초과! 무한 루프 방지됨.");
        }
    }
    
    private void LevelUp()
    {
        // 안전장치: experienceToNextLevel이 0이면 기본값 설정
        if (experienceToNextLevel <= 0)
        {
            experienceToNextLevel = 100;
            Debug.LogWarning("LevelUp: experienceToNextLevel이 0이었습니다. 기본값 100으로 설정됨.");
        }
        
        experience -= experienceToNextLevel;
        level++;
        
        // 레벨업 시 체력 회복 (스탯은 업그레이드로만 증가)
        currentHealth = maxHealth; // 레벨업 시 체력 회복
        
        // HP바 업데이트
        UpdateHealthBar();
        
        // 다음 레벨까지 필요한 경험치 증가 (최소값 보장)
        experienceToNextLevel = Mathf.Max(100, Mathf.RoundToInt(experienceToNextLevel * 1.15f));
        
        OnLevelUp?.Invoke();
    }
    
    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(1, damage - defense);
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);
        
        // HP바 업데이트
        UpdateHealthBar();
        
        OnStatsChanged?.Invoke();
        OnDamageTaken?.Invoke(actualDamage);
        
        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        // HP바 업데이트
        UpdateHealthBar();
        
        OnStatsChanged?.Invoke();
    }
    
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    public void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }
    
    // 장비 스탯 보너스 가져오기
    private int GetEquipmentHealthBonus()
    {
        if (equipmentSystem == null)
        {
            equipmentSystem = FindFirstObjectByType<EquipmentSystem>();
        }
        if (equipmentSystem == null) return 0;
        
        EquipmentStats stats = equipmentSystem.GetTotalEquipmentStats();
        return stats.healthBonus;
    }
    
    private int GetEquipmentAttackBonus()
    {
        if (equipmentSystem == null)
        {
            equipmentSystem = FindFirstObjectByType<EquipmentSystem>();
        }
        if (equipmentSystem == null) return 0;
        
        EquipmentStats stats = equipmentSystem.GetTotalEquipmentStats();
        return stats.attackBonus;
    }
    
    private int GetEquipmentDefenseBonus()
    {
        if (equipmentSystem == null)
        {
            equipmentSystem = FindFirstObjectByType<EquipmentSystem>();
        }
        if (equipmentSystem == null) return 0;
        
        EquipmentStats stats = equipmentSystem.GetTotalEquipmentStats();
        return stats.defenseBonus;
    }
    
    private float GetEquipmentAttackSpeedBonus()
    {
        if (equipmentSystem == null)
        {
            equipmentSystem = FindFirstObjectByType<EquipmentSystem>();
        }
        if (equipmentSystem == null) return 0f;
        
        EquipmentStats stats = equipmentSystem.GetTotalEquipmentStats();
        return stats.attackSpeedBonus;
    }
    
    private float GetEquipmentMoveSpeedBonus()
    {
        if (equipmentSystem == null)
        {
            equipmentSystem = FindFirstObjectByType<EquipmentSystem>();
        }
        if (equipmentSystem == null) return 0f;
        
        EquipmentStats stats = equipmentSystem.GetTotalEquipmentStats();
        return stats.moveSpeedBonus;
    }
    
    private float GetEquipmentCritChanceBonus()
    {
        if (equipmentSystem == null)
        {
            equipmentSystem = FindFirstObjectByType<EquipmentSystem>();
        }
        if (equipmentSystem == null) return 0f;
        
        EquipmentStats stats = equipmentSystem.GetTotalEquipmentStats();
        return stats.critChanceBonus;
    }
    
    private float GetEquipmentCritDamageBonus()
    {
        if (equipmentSystem == null)
        {
            equipmentSystem = FindFirstObjectByType<EquipmentSystem>();
        }
        if (equipmentSystem == null) return 0f;
        
        EquipmentStats stats = equipmentSystem.GetTotalEquipmentStats();
        return stats.critDamageBonus;
    }
    
    // 스탯 계산 메서드
    private int GetCalculatedStat(UpgradeType type)
    {
        if (characterStats == null) return 0;
        
        // 레벨 + 업그레이드 레벨 적용
        // 플레이어 레벨은 기본 스탯에 영향을 주고, 업그레이드 레벨은 추가적인 성장?
        // 아니면 업그레이드 레벨이 곧 레벨?
        // 기존 코드: CalculateStat(base, growth, upgradeLevel)
        // 즉, 플레이어 레벨(level)은 스탯에 영향 없고, upgradeLevel만 영향 있었음.
        // 하지만 CharacterStatsData는 "레벨" 기반 성장임.
        // 플레이어의 경우 "업그레이드"가 레벨 역할을 하므로, 
        // 각 스탯별 레벨 = 1 + upgradeLevel 로 계산하는 것이 적절함.
        
        switch (type)
        {
            case UpgradeType.Health:
                return characterStats.GetMaxHealth(1 + upgradeLevelHealth) + GetEquipmentHealthBonus();
            case UpgradeType.Attack:
                return characterStats.GetAttackPower(1 + upgradeLevelAttack) + GetEquipmentAttackBonus();
            case UpgradeType.Defense:
                return characterStats.GetDefense(1 + upgradeLevelDefense) + GetEquipmentDefenseBonus();
            default: return 0;
        }
    }
    
    private float GetCalculatedStatFloat(UpgradeType type)
    {
        if (characterStats == null) return 0f;
        
        switch (type)
        {
            case UpgradeType.AttackSpeed:
                return characterStats.GetAttackSpeed(1 + upgradeLevelAttackSpeed) + GetEquipmentAttackSpeedBonus();
            case UpgradeType.CritChance:
                return Mathf.Clamp01(characterStats.GetCritChance(1 + upgradeLevelCritChance) + GetEquipmentCritChanceBonus());
            case UpgradeType.CritDamage:
                return characterStats.GetCritDamage(1 + upgradeLevelCritDamage) + GetEquipmentCritDamageBonus();
            default: return 0f;
        }
    }
    
    // 업그레이드 레벨 가져오기
    public int GetUpgradeLevel(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Health: return upgradeLevelHealth;
            case UpgradeType.Attack: return upgradeLevelAttack;
            case UpgradeType.Defense: return upgradeLevelDefense;
            case UpgradeType.AttackSpeed: return upgradeLevelAttackSpeed;
            case UpgradeType.CritChance: return upgradeLevelCritChance;
            case UpgradeType.CritDamage: return upgradeLevelCritDamage;
            default: return 0;
        }
    }
    
    // 업그레이드 레벨 설정
    public void SetUpgradeLevel(UpgradeType type, int level)
    {
        switch (type)
        {
            case UpgradeType.Health:
                upgradeLevelHealth = Mathf.Max(0, level);
                currentHealth = maxHealth; // 업그레이드 시 체력 회복
                UpdateHealthBar();
                break;
            case UpgradeType.Attack:
                upgradeLevelAttack = Mathf.Max(0, level);
                break;
            case UpgradeType.Defense:
                upgradeLevelDefense = Mathf.Max(0, level);
                break;
            case UpgradeType.AttackSpeed:
                upgradeLevelAttackSpeed = Mathf.Max(0, level);
                break;
            case UpgradeType.CritChance:
                upgradeLevelCritChance = Mathf.Max(0, level);
                break;
            case UpgradeType.CritDamage:
                upgradeLevelCritDamage = Mathf.Max(0, level);
                break;
        }
        OnStatsChanged?.Invoke();
    }
    
    // 골드로 스탯 강화 시스템 (레벨 증가 방식)
    public bool UpgradeHealth(int cost)
    {
        if (gold >= cost)
        {
            gold -= cost;
            upgradeLevelHealth++;
            currentHealth = maxHealth; // 업그레이드 시 체력 회복
            UpdateHealthBar();
            OnStatsChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    public bool UpgradeAttack(int cost)
    {
        if (gold >= cost)
        {
            gold -= cost;
            upgradeLevelAttack++;
            OnStatsChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    public bool UpgradeAttackSpeed(int cost)
    {
        if (gold >= cost)
        {
            gold -= cost;
            upgradeLevelAttackSpeed++;
            OnStatsChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    public bool UpgradeDefense(int cost)
    {
        if (gold >= cost)
        {
            gold -= cost;
            upgradeLevelDefense++;
            OnStatsChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    public bool UpgradeCritChance(int cost)
    {
        if (gold >= cost)
        {
            gold -= cost;
            upgradeLevelCritChance++;
            OnStatsChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    public bool UpgradeCritDamage(int cost)
    {
        if (gold >= cost)
        {
            gold -= cost;
            upgradeLevelCritDamage++;
            OnStatsChanged?.Invoke();
            return true;
        }
        return false;
    }
    public float GetStatValue(UpgradeType type, int level)
    {
        // 미리보기용 메서드 (현재 레벨 대신 인자로 받은 레벨 사용)
        if (characterStats == null) return 0;
        
        switch (type)
        {
            case UpgradeType.Health:
                return characterStats.GetMaxHealth(1 + level) + GetEquipmentHealthBonus();
            case UpgradeType.Attack:
                return characterStats.GetAttackPower(1 + level) + GetEquipmentAttackBonus();
            case UpgradeType.Defense:
                return characterStats.GetDefense(1 + level) + GetEquipmentDefenseBonus();
            case UpgradeType.AttackSpeed:
                return characterStats.GetAttackSpeed(1 + level) + GetEquipmentAttackSpeedBonus();
            case UpgradeType.CritChance:
                return Mathf.Clamp01(characterStats.GetCritChance(1 + level) + GetEquipmentCritChanceBonus());
            case UpgradeType.CritDamage:
                return characterStats.GetCritDamage(1 + level) + GetEquipmentCritDamageBonus();
            default:
                return 0;
        }
    }

    // ICombatTarget 구현
    public Transform GetTransform()
    {
        return transform;
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    public string GetName()
    {
        return playerName;
    }
}
