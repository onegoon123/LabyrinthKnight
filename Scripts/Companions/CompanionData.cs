using UnityEngine;

/// <summary>
/// 동료 캐릭터의 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Companion", menuName = "Game/Companion Data")]
public class CompanionData : CharacterStatsData
{
    [Header("기본 정보 (추가)")]
    public string companionId = "companion_default"; // 고유 ID (저장/로드용)
    public string description;
    public Sprite icon;
    public Sprite portrait;
    public RuntimeAnimatorController companionAnimatorController; // 애니메이션 컨트롤러
    
    [Header("스탯 (추가)")]
    public int level = 1;
    
    [Header("전투 설정 (추가)")]
    public GameObject attackParticlePrefab; // 공격 파티클 프리팹
    
    [Header("해금 및 호감도 설정 (추가)")]
    public int unlockCost = 1000; // 해금 비용

    [Header("AI 설정")]
    public float searchInterval = 0.5f; // 적 탐색 간격
    public float followDistance = 5f; // 플레이어 추적 시작 거리
    public float followSpeed = 6f; // 플레이어 추적 속도
    
    // CharacterStatsData의 필드들을 상속받음
    // characterName, portrait, baseStats, growthRates 등
    // attackSpeed, attackRange, moveSpeed도 상속받음
}
