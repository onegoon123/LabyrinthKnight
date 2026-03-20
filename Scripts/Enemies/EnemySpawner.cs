using UnityEngine;

/// <summary>
/// [적 생성기]
/// 적 오브젝트의 생성(Instantiate)과 재사용(Pooling)을 전담하는 클래스입니다.
/// 게임 로직(언제, 무엇을 스폰할지)은 포함하지 않으며, '어디에', '어떻게' 만들지만 담당합니다.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject enemyPrefab; // 생성할 적의 원본 프리팹
    public float spawnRadius = 8f; // 플레이어/중심점 기준 스폰 반경
    public int maxEnemies = 100; // 오브젝트 풀의 최대 크기 (메모리 관리용)
    
    private PoolManager enemyPool; // 오브젝트 풀링 매니저
    
    private void Awake()
    {
        // Enemy Pool 초기화
        if (enemyPrefab != null)
        {
            // 풀링을 관리할 자식 오브젝트 생성
            GameObject poolObj = new GameObject("EnemyPool");
            poolObj.transform.SetParent(transform);
            
            // PoolManager 컴포넌트 설정
            enemyPool = poolObj.AddComponent<PoolManager>();
            enemyPool.prefab = enemyPrefab;
            enemyPool.defaultCapacity = 10; // 초기 생성 개수
            enemyPool.maxSize = maxEnemies; // 최대 개수
        }
    }
    
    /// <summary>
    /// 스폰 반경 내의 랜덤한 위치를 반환합니다.
    /// </summary>
    public Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);
    }
    
    /// <summary>
    /// 지정된 위치에 적을 스폰(또는 풀에서 가져오기)하고 데이터를 초기화합니다.
    /// </summary>
    /// <param name="data">적에게 적용할 스탯 데이터</param>
    /// <param name="position">스폰 위치</param>
    /// <returns>생성된 Enemy 컴포넌트</returns>
    public Enemy SpawnEnemy(EnemyData data, Vector3 position)
    {
        if (enemyPool == null) return null;
        
        // 풀에서 오브젝트 가져오기 (없으면 새로 생성됨)
        GameObject enemyObj = enemyPool.Get(position, Quaternion.identity);
        if (enemyObj == null) return null;
        
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            // 적 데이터 및 상태 초기화
            enemy.InitializeEnemy(data);
            
            // 이벤트 중복 구독 방지 (풀링된 객체 재사용 시 중요)
            enemy.OnEnemyDefeated -= OnEnemyDefeated;
            enemy.OnEnemyDefeated += OnEnemyDefeated;
        }
        
        return enemy;
    }
    
    /// <summary>
    /// 적이 처치되었을 때 호출되는 내부 콜백 (풀 반환 처리)
    /// </summary>
    private void OnEnemyDefeated(Enemy enemy)
    {
        // 이벤트 구독 해제
        enemy.OnEnemyDefeated -= OnEnemyDefeated;
        
        if (enemyPool != null)
        {
            enemyPool.Release(enemy.gameObject);
        }
    }
    
    public void ReleaseEnemy(Enemy enemy)
    {
        if (enemy != null && enemyPool != null)
        {
            enemyPool.Release(enemy.gameObject);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // 에디터에서 스폰 반경을 시각적으로 확인하기 위함
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
