using UnityEngine;

/// <summary>
/// 공격 파티클 풀 매니저
/// PlayerController와 CompanionController에서 파티클을 풀링합니다.
/// </summary>
public class AttackParticlePool : MonoBehaviour
{
    public static AttackParticlePool Instance { get; private set; }
    
    [Header("Particle Pools")]
    public PoolManager defaultParticlePool;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 파티클을 생성하고 자동으로 반환합니다.
    /// </summary>
    public void SpawnParticle(GameObject prefab, Vector3 position, Quaternion rotation, float lifetime = 1f)
    {
        if (prefab == null) return;
        
        // 해당 프리팹의 풀을 찾거나 생성
        PoolManager pool = GetOrCreatePool(prefab);
        if (pool != null)
        {
            pool.GetAndReleaseAfter(position, rotation, lifetime);
        }
    }
    
    /// <summary>
    /// 프리팹에 대한 풀을 찾거나 생성합니다.
    /// </summary>
    private PoolManager GetOrCreatePool(GameObject prefab)
    {
        // 기존 풀 찾기
        PoolManager[] pools = GetComponentsInChildren<PoolManager>();
        foreach (PoolManager pool in pools)
        {
            if (pool.prefab == prefab)
            {
                return pool;
            }
        }
        
        // 새 풀 생성
        GameObject poolObj = new GameObject($"ParticlePool_{prefab.name}");
        poolObj.transform.SetParent(transform);
        PoolManager newPool = poolObj.AddComponent<PoolManager>();
        newPool.prefab = prefab;
        newPool.defaultCapacity = 10;
        newPool.maxSize = 30;
        
        return newPool;
    }
}
