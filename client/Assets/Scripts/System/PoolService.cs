using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Pool;

public class PoolService : MonoBehaviour
{
    // 프리팹(원본)을 키(Key)로 사용하여 풀을 관리합니다.
    private Dictionary<GameObject, IObjectPool<GameObject>> _pools = new Dictionary<GameObject, IObjectPool<GameObject>>();
    
    // 풀 기본 설정값
    [SerializeField] private bool _collectionCheck = true; // 반납 시 중복 검사 (에러 방지)
    [SerializeField] private int _defaultCapacity = 10;
    [SerializeField] private int _maxSize = 100;
    
    // 1. 오브젝트 가져오기 (없으면 풀 생성)
    public GameObject Get(GameObject prefab)
    {
        if (prefab == null)
            return null;
        
        // 해당 프리팹의 풀이 없으면 새로 만듭니다.
        if (!_pools.ContainsKey(prefab))
        {
            CreatePool(prefab);
        }
        
        return _pools[prefab].Get();
    }
    
    // 2. 내부적으로 풀 생성하는 로직
    private void CreatePool(GameObject prefab)
    {
        // 유니티 내장 ObjectPool 사용
        IObjectPool<GameObject> pool = null;
        pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                var obj = Instantiate(prefab);
                obj.name = prefab.name;
                // PoolObject를 붙이고, 자기 자신의 풀(pool 변수)을 알려줍니다.
                var poolObj = obj.GetComponent<PoolObject>();
                if (poolObj == null)
                {
                    poolObj = obj.AddComponent<PoolObject>();
                }
                
                poolObj.SetPool(pool); // [핵심] 여기서 풀을 주입!

                return obj;
            },
            actionOnGet: OnGetObject,
            actionOnRelease: OnReleaseObject,
            actionOnDestroy: OnDestroyObject,
            collectionCheck: _collectionCheck,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );
        
        _pools.Add(prefab, pool);
    }
    
    // --- 풀 콜백 함수들 ---
    // B. 풀에서 꺼낼 때
    private void OnGetObject(GameObject obj)
    {
        obj.SetActive(true);
        
        // IPoolable 인터페이스가 있다면 OnSpawn 호출
        var poolables = obj.GetComponentsInChildren<IPoolable>();
        foreach (var poolable in poolables)
        {
            poolable.OnSpawn();
        }
    }
    
    // C. 풀에 반납할 때
    private void OnReleaseObject(GameObject obj)
    {
        // IPoolable 인터페이스가 있다면 OnDespawn 호출
        var poolables = obj.GetComponentsInChildren<IPoolable>();
        foreach (var poolable in poolables)
        {
            poolable.OnDespawn();
        }
        
        obj.SetActive(false);
        obj.transform.SetParent(transform); // 풀 서비스(또는 별도 컨테이너) 아래로 정리
    }
    
    // D. 풀이 넘쳐서 진짜 파괴할 때
    private void OnDestroyObject(GameObject obj)
    {
        Destroy(obj);
    }
}
