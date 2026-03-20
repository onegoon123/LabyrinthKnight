using UnityEngine;

/// <summary>
/// 플레이어 공격 시 파티클 효과를 관리하는 컴포넌트
/// 공격 방향에 맞춰 파티클을 회전시켜 검 휘두르기 효과를 연출합니다.
/// </summary>
public class PlayerAttackParticles : MonoBehaviour
{
    [Header("Particle Settings")]
    [Tooltip("공격 파티클 프리팹 (Particle System)")]
    public GameObject attackParticlePrefab;
    
    [Tooltip("파티클 생성 위치 오프셋 (플레이어 기준)")]
    public Vector2 particleOffset = new Vector2(0.5f, 0f);
    
    [Tooltip("파티클 자동 삭제 시간 (초)")]
    public float particleLifetime = 1f;
    
    [Header("Rotation Settings")]
    [Tooltip("파티클 기본 회전 오프셋 (도)")]
    public float rotationOffset = 0f;
    
    private SpriteRenderer spriteRenderer;
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    /// <summary>
    /// 공격 파티클을 생성합니다.
    /// </summary>
    /// <param name="targetPosition">공격 대상의 위치</param>
    public void SpawnAttackParticle(Vector3 targetPosition)
    {
        if (attackParticlePrefab == null)
        {
            Debug.LogWarning("[PlayerAttackParticles] Attack particle prefab is not assigned!");
            return;
        }
        
        // 1. 공격 방향 계산
        Vector2 attackDirection = (targetPosition - transform.position).normalized;
        
        // 2. 파티클 생성 위치 (플레이어 위치 + 오프셋)
        Vector3 spawnPosition = targetPosition;
        
        // 3. 방향에 따른 회전 각도 계산
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        
        // 4. 기본 회전 오프셋 적용
        angle += rotationOffset;
        
        // 5. 파티클 풀에서 생성
        if (AttackParticlePool.Instance != null)
        {
            AttackParticlePool.Instance.SpawnParticle(
                attackParticlePrefab, 
                spawnPosition, 
                Quaternion.Euler(0f, 0f, angle), 
                particleLifetime
            );
        }
        else
        {
            // 폴백: 풀이 없으면 일반 생성
            GameObject particle = Instantiate(attackParticlePrefab, spawnPosition, Quaternion.Euler(0f, 0f, angle));
            Destroy(particle, particleLifetime);
        }
    }
    
    /// <summary>
    /// 특정 방향으로 파티클을 생성합니다.
    /// </summary>
    /// <param name="direction">공격 방향 (정규화된 벡터)</param>
    public void SpawnAttackParticleInDirection(Vector2 direction)
    {
        if (attackParticlePrefab == null)
        {
            Debug.LogWarning("[PlayerAttackParticles] Attack particle prefab is not assigned!");
            return;
        }
        
        // 1. 파티클 생성 위치 (플레이어 위치 + 오프셋)
        Vector3 spawnPosition = transform.position + (Vector3)particleOffset;
        
        // 2. 방향에 따른 회전 각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // 3. 기본 회전 오프셋 적용
        angle += rotationOffset;
        
        // 4. 파티클 풀에서 생성
        if (AttackParticlePool.Instance != null)
        {
            AttackParticlePool.Instance.SpawnParticle(
                attackParticlePrefab, 
                spawnPosition, 
                Quaternion.Euler(0f, 0f, angle), 
                particleLifetime
            );
        }
        else
        {
            // 폴백: 풀이 없으면 일반 생성
            GameObject particle = Instantiate(attackParticlePrefab, spawnPosition, Quaternion.Euler(0f, 0f, angle));
            Destroy(particle, particleLifetime);
        }
    }
}
