using UnityEngine;
using System.Collections.Generic;
using DarkNaku.Number;

[CreateAssetMenu(fileName = "New Dungeon", menuName = "Game/Dungeon Data")]
public class DungeonData : ScriptableObject
{
    [Header("기본 정보")]
    public string dungeonName;
    [TextArea] public string description;
    public int recommendedLevel;
    public Sprite icon;
    public StageThemeData theme; // 던전 배경 테마
    
    [Header("웨이브 설정")]
    public List<DungeonWave> waves = new List<DungeonWave>();
    
    [Header("보상 설정")]
    public DungeonRewards rewards;
}

[System.Serializable]
public class DungeonWave
{
    public string waveName;
    public List<EnemyData> enemiesToSpawn; // 적 데이터 리스트
    public EnemyData bossData; // 보스 데이터 (마지막 웨이브인 경우)
    public int spawnCount = 5; // 일반 몬스터 수
    public float spawnInterval = 1.5f;
}

[System.Serializable]
public struct DungeonRewards
{
    public int goldReward;
    public int expReward;
    public List<GameObject> itemRewards; // 아이템 프리팹 또는 데이터
}
