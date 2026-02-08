using UnityEngine;
using UnityEngine.Pool;

public interface IPoolable
{
    // 풀에서 꺼내질 때 호출 (OnEnable 역할)
    void OnSpawn();
    
    // 풀로 돌아갈 때 호출 (OnDisable 역할)
    void OnDespawn();
}
public class PoolObject : MonoBehaviour
{
    // 내가 소속된 풀의 참조 (반납할 때 사용)
    private IObjectPool<GameObject> _pool;
    
    // 풀을 설정해주는 초기화 함수
    public void SetPool(IObjectPool<GameObject> pool)
    {
        _pool = pool;
    }
    
    // [핵심] 스스로를 반납하는 함수
    public void ReturnToPool()
    {
        if (_pool != null)
        {
            _pool.Release(gameObject);
        }
        else
        {
            // 풀 시스템을 안 거치고 그냥 생성된 경우를 대비해 파괴
            Destroy(gameObject);
        }
    }
}
