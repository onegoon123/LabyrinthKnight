using System.Collections.Generic;
using UnityEngine;

public class GameDataLoader : MonoBehaviour
{
    [Header("로드된 데이터")]
    public List<PlayerStatsData> playerStatsData = new List<PlayerStatsData>();
    public List<EnemyStatsData> enemyStatsData = new List<EnemyStatsData>();
    public List<GameSettingData> gameSettingsData = new List<GameSettingData>();
    
    private void Start()
    {
        LoadAllData();
    }
    
    public void LoadAllData()
    {
        LoadPlayerStats();
        LoadEnemyStats();
        LoadGameSettings();
        
        Debug.Log("모든 게임 데이터를 로드했습니다.");
    }
    
    private void LoadPlayerStats()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "PlayerStats.csv");
        if (System.IO.File.Exists(filePath))
        {
            List<Dictionary<string, string>> csvData = CSVReader.ReadCSV(filePath);
            playerStatsData.Clear();
            
            foreach (Dictionary<string, string> row in csvData)
            {
                PlayerStatsData data = new PlayerStatsData();
                if (row.ContainsKey("statName")) data.statName = row["statName"];
                if (row.ContainsKey("baseValue") && float.TryParse(row["baseValue"], out float baseValue)) data.baseValue = baseValue;
                if (row.ContainsKey("levelIncrease") && float.TryParse(row["levelIncrease"], out float levelIncrease)) data.levelIncrease = levelIncrease;
                if (row.ContainsKey("description")) data.description = row["description"];
                
                playerStatsData.Add(data);
            }
            
            Debug.Log($"플레이어 스탯 데이터 {playerStatsData.Count}개를 로드했습니다.");
        }
    }
    
    private void LoadEnemyStats()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "EnemyStats.csv");
        if (System.IO.File.Exists(filePath))
        {
            List<Dictionary<string, string>> csvData = CSVReader.ReadCSV(filePath);
            enemyStatsData.Clear();
            
            foreach (Dictionary<string, string> row in csvData)
            {
                EnemyStatsData data = new EnemyStatsData();
                if (row.ContainsKey("enemyLevel") && int.TryParse(row["enemyLevel"], out int enemyLevel)) data.enemyLevel = enemyLevel;
                if (row.ContainsKey("baseHealth") && int.TryParse(row["baseHealth"], out int baseHealth)) data.baseHealth = baseHealth;
                if (row.ContainsKey("healthPerLevel") && int.TryParse(row["healthPerLevel"], out int healthPerLevel)) data.healthPerLevel = healthPerLevel;
                if (row.ContainsKey("baseAttack") && int.TryParse(row["baseAttack"], out int baseAttack)) data.baseAttack = baseAttack;
                if (row.ContainsKey("attackPerLevel") && int.TryParse(row["attackPerLevel"], out int attackPerLevel)) data.attackPerLevel = attackPerLevel;
                if (row.ContainsKey("baseDefense") && int.TryParse(row["baseDefense"], out int baseDefense)) data.baseDefense = baseDefense;
                if (row.ContainsKey("defensePerLevel") && int.TryParse(row["defensePerLevel"], out int defensePerLevel)) data.defensePerLevel = defensePerLevel;
                if (row.ContainsKey("baseAttackSpeed") && float.TryParse(row["baseAttackSpeed"], out float baseAttackSpeed)) data.baseAttackSpeed = baseAttackSpeed;
                if (row.ContainsKey("attackSpeedPerLevel") && float.TryParse(row["attackSpeedPerLevel"], out float attackSpeedPerLevel)) data.attackSpeedPerLevel = attackSpeedPerLevel;
                if (row.ContainsKey("baseExpReward") && int.TryParse(row["baseExpReward"], out int baseExpReward)) data.baseExpReward = baseExpReward;
                if (row.ContainsKey("expPerLevel") && int.TryParse(row["expPerLevel"], out int expPerLevel)) data.expPerLevel = expPerLevel;
                if (row.ContainsKey("baseGoldReward") && int.TryParse(row["baseGoldReward"], out int baseGoldReward)) data.baseGoldReward = baseGoldReward;
                if (row.ContainsKey("goldPerLevel") && int.TryParse(row["goldPerLevel"], out int goldPerLevel)) data.goldPerLevel = goldPerLevel;
                if (row.ContainsKey("attackRange") && float.TryParse(row["attackRange"], out float attackRange)) data.attackRange = attackRange;
                if (row.ContainsKey("moveSpeed") && float.TryParse(row["moveSpeed"], out float moveSpeed)) data.moveSpeed = moveSpeed;
                if (row.ContainsKey("detectionRange") && float.TryParse(row["detectionRange"], out float detectionRange)) data.detectionRange = detectionRange;
                if (row.ContainsKey("enemyName")) data.enemyName = row["enemyName"];
                
                enemyStatsData.Add(data);
            }
            
            Debug.Log($"적 스탯 데이터 {enemyStatsData.Count}개를 로드했습니다.");
        }
    }
    
    
    private void LoadGameSettings()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "GameSettings.csv");
        if (System.IO.File.Exists(filePath))
        {
            List<Dictionary<string, string>> csvData = CSVReader.ReadCSV(filePath);
            gameSettingsData.Clear();
            
            foreach (Dictionary<string, string> row in csvData)
            {
                GameSettingData data = new GameSettingData();
                if (row.ContainsKey("settingName")) data.settingName = row["settingName"];
                if (row.ContainsKey("value") && float.TryParse(row["value"], out float value)) data.value = value;
                if (row.ContainsKey("description")) data.description = row["description"];
                
                gameSettingsData.Add(data);
            }
            
            Debug.Log($"게임 설정 데이터 {gameSettingsData.Count}개를 로드했습니다.");
        }
    }
    
    // 데이터 조회 메서드들
    public PlayerStatsData GetPlayerStat(string statName)
    {
        return playerStatsData.Find(data => data.statName == statName);
    }
    
    public EnemyStatsData GetEnemyStats(int level)
    {
        return enemyStatsData.Find(data => data.enemyLevel == level);
    }
    
    
    public float GetGameSetting(string settingName)
    {
        GameSettingData setting = gameSettingsData.Find(data => data.settingName == settingName);
        return setting != null ? setting.value : 0f;
    }
}
