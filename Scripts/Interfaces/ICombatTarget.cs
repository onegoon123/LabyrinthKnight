using UnityEngine;

/// <summary>
/// 전투 대상이 될 수 있는 엔티티를 위한 인터페이스
/// 플레이어와 동료가 구현하여 적이 타겟팅할 수 있도록 합니다.
/// </summary>
public interface ICombatTarget
{
    /// <summary>
    /// 대상의 현재 위치를 반환합니다.
    /// </summary>
    Transform GetTransform();
    
    /// <summary>
    /// 대상이 살아있는지 확인합니다.
    /// </summary>
    bool IsAlive();
    
    /// <summary>
    /// 대상에게 데미지를 입힙니다.
    /// </summary>
    /// <param name="damage">입힐 데미지</param>
    void TakeDamage(int damage);
    
    /// <summary>
    /// 대상의 현재 체력을 반환합니다.
    /// </summary>
    int GetCurrentHealth();
    
    /// <summary>
    /// 대상의 최대 체력을 반환합니다.
    /// </summary>
    int GetMaxHealth();
    
    /// <summary>
    /// 대상의 이름을 반환합니다.
    /// </summary>
    string GetName();
}
