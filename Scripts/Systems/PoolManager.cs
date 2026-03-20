using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 범용 오브젝트 풀 매니저
/// UnityEngine.Pool을 사용하여 GameObject를 재사용합니다.
/// </summary>
public class PoolManager : MonoBehaviour
{
    [Header("Pool Settings")]
    [Tooltip("풀링할 프리팹")]
    public GameObject prefab;
    
    [Tooltip("기본 풀 용량")]
    public int defaultCapacity = 10;
    
    [Tooltip("최대 풀 크기")]
    public int maxSize = 50;
    
    private ObjectPool<GameObject> pool;
    
    private void Awake()
    {
        // UnityEngine.Pool 초기화
        pool = new ObjectPool<GameObject>(
            createFunc: CreateObject,
            actionOnGet: OnGetFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }
    
    /// <summary>
    /// 새 오브젝트 생성
    /// </summary>
    private GameObject CreateObject()
    {
        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(transform);
        return obj;
    }
    
    /// <summary>
    /// 풀에서 가져올 때 호출
    /// </summary>
    private void OnGetFromPool(GameObject obj)
    {
        obj.SetActive(true);
    }
    
    /// <summary>
    /// 풀로 반환할 때 호출
    /// </summary>
    private void OnReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
    }
    
    /// <summary>
    /// 오브젝트 파괴 시 호출
    /// </summary>
    private void OnDestroyPoolObject(GameObject obj)
    {
        Destroy(obj);
    }
    
    /// <summary>
    /// 풀에서 오브젝트 가져오기
    /// </summary>
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = pool.Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }
    
    /// <summary>
    /// 풀로 오브젝트 반환
    /// </summary>
    public void Release(GameObject obj)
    {
        if (obj != null)
        {
            pool.Release(obj);
        }
    }
    
    /// <summary>
    /// 일정 시간 후 자동 반환
    /// </summary>
    public GameObject GetAndReleaseAfter(Vector3 position, Quaternion rotation, float delay)
    {
        GameObject obj = Get(position, rotation);
        StartCoroutine(ReleaseAfterDelay(obj, delay));
        return obj;
    }
    
    private System.Collections.IEnumerator ReleaseAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Release(obj);
    }
    
    /// <summary>
    /// 풀 정리
    /// </summary>
    public void Clear()
    {
        pool.Clear();
    }
}
