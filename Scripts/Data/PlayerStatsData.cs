using System;

[System.Serializable]
public class PlayerStatsData
{
    public string statName;
    public float baseValue;
    public float levelIncrease;
    public string description;
}

[System.Serializable]
public class EnemyStatsData
{
    public int enemyLevel;
    public int baseHealth;
    public int healthPerLevel;
    public int baseAttack;
    public int attackPerLevel;
    public int baseDefense;
    public int defensePerLevel;
    public float baseAttackSpeed;
    public float attackSpeedPerLevel;
    public int baseExpReward;
    public int expPerLevel;
    public int baseGoldReward;
    public int goldPerLevel;
    public float attackRange;
    public float moveSpeed;
    public float detectionRange;
    public string enemyName;
}

[System.Serializable]
public class GameSettingData
{
    public string settingName;
    public float value;
    public string description;
}
