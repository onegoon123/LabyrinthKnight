using UnityEngine;

[CreateAssetMenu(fileName = "New Character Stats", menuName = "Game/Character Stats Data")]
public class CharacterStatsData : ScriptableObject
{
    [Header("기본 정보")]
    public string characterName;
    
    [Header("기본 전투 스탯 (Base Stats at Level 1)")]
    public int baseMaxHealth = 150;
    public int baseAttackPower = 10;
    public int baseDefense = 5;
    public float baseAttackSpeed = 1.0f;
    [Range(0f, 1f)] public float baseCritChance = 0.05f;
    public float baseCritDamage = 1.5f;
    
    [Header("이동 및 기타")]
    public float moveSpeed = 3.0f;
    public float attackRange = 2.0f;
    public float detectionRange = 10.0f;
    
    [Header("성장 계수 (Growth Rate)")]
    [Tooltip("HP(n) = baseMaxHealth × growthHealth^(n-1)")]
    public float growthHealth = 1.1f;
    [Tooltip("Attack(n) = baseAttackPower × growthAttack^(n-1)")]
    public float growthAttack = 1.08f;
    [Tooltip("Defense(n) = baseDefense × growthDefense^(n-1)")]
    public float growthDefense = 1.1f;
    [Tooltip("AttackSpeed(n) = baseAttackSpeed × growthAttackSpeed^(n-1)")]
    public float growthAttackSpeed = 1.05f;
    [Tooltip("CritChance(n) = baseCritChance × growthCritChance^(n-1)")]
    public float growthCritChance = 1.1f;
    [Tooltip("CritDamage(n) = baseCritDamage × growthCritDamage^(n-1)")]
    public float growthCritDamage = 1.05f;
    
    // 스탯 계산 메서드
    public int GetMaxHealth(int level) => CalculateStat(baseMaxHealth, growthHealth, level);
    public int GetAttackPower(int level) => CalculateStat(baseAttackPower, growthAttack, level);
    public int GetDefense(int level) => CalculateStat(baseDefense, growthDefense, level);
    public float GetAttackSpeed(int level) => CalculateStatFloat(baseAttackSpeed, growthAttackSpeed, level);
    public float GetCritChance(int level) => Mathf.Clamp01(CalculateStatFloat(baseCritChance, growthCritChance, level));
    public float GetCritDamage(int level) => CalculateStatFloat(baseCritDamage, growthCritDamage, level);
    
    private int CalculateStat(int baseValue, float growth, int level)
    {
        if (level <= 1) return baseValue;
        
        // Overflow 방지: float/double 연산 후 long으로 변환하여 체크
        double result = baseValue * System.Math.Pow(growth, level - 1);
        
        if (result > int.MaxValue)
        {
            return int.MaxValue;
        }
        
        return (int)result;
    }
    
    private float CalculateStatFloat(float baseValue, float growth, int level)
    {
        if (level <= 1) return baseValue;
        return baseValue * Mathf.Pow(growth, level - 1);
    }
}
