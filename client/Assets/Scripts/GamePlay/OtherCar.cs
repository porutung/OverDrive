using UnityEngine;

public class OtherCar : MonoBehaviour, IPoolable
{
    public float speed;
    public float _currentSpeed; // 각 차량의 속도
    public PlayerCarController playerCar;
    private PoolService _poolService;
    private PoolObject _poolObject;
    private Transform _cachedTransform;
    private Rigidbody _cachedRigidbody;
    public void OnSpawn()
    {        
        // 1. 속도 초기화 (이전 속도 제거)
        _currentSpeed = speed;

        // 2. 물리적 힘 초기화 (날아가던 힘 제거)
        if (_cachedRigidbody != null)
        {
            _cachedRigidbody.linearVelocity = Vector3.zero;// 이동 가속도 제거 (Unity 6 / 2023.3 이상)
            // _rb.velocity = Vector3.zero;     // (구버전 Unity라면 이 코드 사용)
            
            _cachedRigidbody.angularVelocity = Vector3.zero; // 회전 가속도 제거
            _cachedRigidbody.Sleep(); // 물리 연산 강제 중지 (확실하게 멈춤)
        }
        
        // 3. 뒤집힌 차체 바로잡기
        transform.rotation = Quaternion.identity; 
        // 만약 차가 180도 돌아서 생성되어야 한다면: Quaternion.Euler(0, 180, 0);
    }

    public void OnDespawn()
    {      
    }

    void Awake()
    {
        _cachedTransform = transform;
        _cachedRigidbody = GetComponent<Rigidbody>();
    }
    void Start()
    {
        
        _poolService = ServiceLocator.Get<PoolService>();
        _poolObject = GetComponent<PoolObject>();
    }
    void Update()
    {        
        if (playerCar == null) 
            return;

        if (playerCar.CurrentState == PlayerCarController.CarState.OutOfFuel)
            return;

        float targetSpeed = speed;
        if (playerCar.IsBoosting())
        {
            targetSpeed += playerCar.carStats.boostMaxSpeed;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, playerCar.carStats.acceleration * Time.deltaTime);
        }
        else if (playerCar.IsNitroBoosting())
        {
            targetSpeed += playerCar.carStats.nitroBoostSpeed;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, playerCar.carStats.acceleration * Time.deltaTime);
        }
        else
        {
            _currentSpeed = targetSpeed;
        }
                
        transform.Translate(Vector3.back * _currentSpeed * Time.deltaTime, Space.World);

        // 화면 밖으로 나가면 스스로 파괴
        if (transform.position.z < playerCar.transform.position.z - 20) // 임의의 파괴 지점
        {
            _poolObject.ReturnToPool();
        }
    }
}