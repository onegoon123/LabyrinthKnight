using UnityEngine;

public static class UpgradeProgress
{
    // PlayerStats를 통한 업그레이드 레벨 관리 (PlayerPrefs 대신)
    // GameSaveSystem의 JSON 저장 시스템과 통합됨
    
    private static PlayerStats GetPlayerStats()
    {
        return Object.FindFirstObjectByType<PlayerStats>();
    }

    public static int GetLevel(UpgradeType type)
    {
        PlayerStats playerStats = GetPlayerStats();
        if (playerStats != null)
        {
            return playerStats.GetUpgradeLevel(type);
        }
        return 0;
    }

    public static void SetLevel(UpgradeType type, int level)
    {
        PlayerStats playerStats = GetPlayerStats();
        if (playerStats != null)
        {
            playerStats.SetUpgradeLevel(type, level);
        }
    }

    public static void IncrementLevel(UpgradeType type)
    {
        SetLevel(type, GetLevel(type) + 1);
    }
}

