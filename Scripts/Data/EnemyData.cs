using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Game/Enemy Data")]
public class EnemyData : CharacterStatsData
{
    [Header("적 설정")]
    public int level = 1; // 기본 레벨

    public Sprite sprite;

    [Header("보상")]
    public int experienceReward = 10;
    public int goldReward = 5;
    
    // CharacterStatsData의 필드들을 상속받음
    // characterName, portrait, baseStats, growthRates 등
}
