using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PlayerCarController : MonoBehaviour
{
    public ChaseCamera mainCamera; // 인스펙터에서 메인 카메라를 연결해줘야 함
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI splipstreamDistanceText;
    public Slider fuelSlider;
    public GameObject gameOverScreen;
    public Button restartButton;
    public Button nitroBoostButton;
    
    [Header("차량 성능 데이터")]
    [Tooltip("이 차량에 적용할 성능 스펙 파일을 연결해주세요.")]
    public CarStats carStats;
    
    // --- 시각적 모델 연결 ---
    [Header("오브젝트 연결")]
    [Tooltip("물리 계산과 분리된, 눈에 보이는 차의 3D 모델 트랜스폼")]
    public Transform carVisualModel;
    
    [Header("차선 설정")]
    [SerializeField] private float[] laneXPositions = { -2.5f, 0f, 2.5f }; // 3개 차선의 x좌표
    [SerializeField] private float laneChangeSpeed = 15f; // 차선 변경 속도
    private int _currentLaneIndex = 1; // 현재 차선 (가운데에서 시작)
    
    // --- 연료 시스템 관련 변수 추가 ---
    private float _currentFuel;
    
    [Header("속도 및 슬립스트림")]
    private bool _isBoosting = false;
    private bool _isSlipstream = false;
    public bool IsInSlipstream() { return _isSlipstream; }
    public bool IsBoosting() { return _isBoosting; }
    public bool IsNitroBoosting() { return currentState == CarState.NitroBoosting; }
    
    [Header("RayCast 위치값")]
    [SerializeField]private Transform rayCastPoint;
    private RaycastHit _slipstreamHit; // Raycast 정보를 저장할 변수
    
    // --- 자동 가속/감속 로직 변수 ---
    public enum CarState { Accelerating, Decelerating, OutOfFuel, NitroBoosting }
    private CarState currentState;
    public CarState CurrentState { get { return currentState; } }
    
    public float currentSpeed; // 현재 속도 (RoadScroller가 참조)    
    
    [Header("충돌 물리 효과")]
    [Tooltip("충돌 시 상대방을 위로 띄우는 힘의 크기")]
    public float upwardImpactModifier = 1.5f;
    
    [Header("틸트(기울기)=true or Yaw 회전 효과 선택=false")]
    [SerializeField] private bool isTiltOrYaw = false;
    
    [Header("틸트(기울기) 효과")]
    [SerializeField] private float tiltAngle = 15f; // 최대 기울기 각도
    [SerializeField] private float tiltSpeed = 10f; // 기울기 변화 속도
    
    // [수정] 이 부분을 '틸트'에서 '요'로 변경합니다.
    [Header("요(Yaw) 회전 효과")]
    [SerializeField] private float yawAngle = 10f; // 차선 변경 시 최대 회전 각도
    [SerializeField] private float rotationSpeed = 10f; // 회전 변화 속도
    
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    private Coroutine _carShakeCoroutine = null;
    private Coroutine _boostingCoroutine = null;
    private float _boostTimeRemaining = 0f;
    
    [Header("NitroBoost 콤보 횟수")]
    private int _boostComboCount = 0;
    
    void Start()
    {        
        // 차선 이동 관련 초기화
        _targetPosition = transform.position;
        _targetPosition.x = laneXPositions[_currentLaneIndex];
        _targetRotation = transform.rotation;
        
        // 자동 가속 관련 초기화
        currentState = CarState.Accelerating;
        // 연료 초기화
        _currentFuel = carStats.maxFuel;
        
        gameOverScreen.gameObject.SetActive(false);
        nitroBoostButton.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ActivateNitroBoost()
    {
        // 버튼이 활성화된 상태에서만 발동 가능
        if (nitroBoostButton != null && nitroBoostButton.gameObject.activeSelf)
        {
            nitroBoostButton.gameObject.SetActive(false);
            _boostComboCount = 0; // 콤보 카운트 초기화
            StartCoroutine(NitroBoostCoroutine());
        }
    }
    public void AddFuel(float amount)
    {
        // 현재 연료에 보급량을 더합니다.
        _currentFuel += amount;

        // 최대 연료량을 넘지 않도록 제한합니다.
        if (_currentFuel > carStats.maxFuel)
        {
            _currentFuel = carStats.maxFuel;
        }
        Debug.Log($"연료 {amount} 보급! 현재 연료: {_currentFuel}");
    }
    void Update()
    {
        // --- 1. 연료 소모 및 고갈 확인 (가장 먼저 처리) ---
        if (currentState != CarState.OutOfFuel)
        {
            _currentFuel -= carStats.fuelConsumptionRate * Time.deltaTime;
            fuelSlider.value = _currentFuel / carStats.maxFuel;
            
            if (_currentFuel <= 0)
            {
                _currentFuel = 0;
                currentState = CarState.OutOfFuel;
                gameOverScreen.gameObject.SetActive(true);
                Debug.Log("연료 고갈! 게임 오버!");
                // 여기서 게임 오버 UI를 띄우는 이벤트를 호출할 수 있습니다.
            }
        }
        
        // 1. 슬립스트림 조건 확인 (가속 상태일 때만)
        if (currentState == CarState.Accelerating)
        {
            CheckForSlipstream();
        }
        else
        {
            _isSlipstream = false; // 감속 중에는 슬립스트림 비활성화
        }
        
        //  2. 자동 가속/감속 상태 관리
        HandleAccelerationState();
        
        UpdatePosition();
        
        if (isTiltOrYaw)
        {
            UpdateTilt();
        }
        else
        {
            // [수정] 호출하는 메서드 이름을 변경합니다.
            UpdateYawRotation();     
        }      
        // 디버그용 로그 (슬립스트림 상태 표시)
        //Debug.Log($"State: {currentState}, Speed: {currentSpeed:F1}, Slipstream: {isSlipstream}");
        speedText.text = string.Format($"{currentSpeed:F0}km/h");
        splipstreamDistanceText.text = string.Format($"Combo : {_boostComboCount}");//string.Format($"Front Car Dist:{_slipstreamHit.distance:F2}");
        
        // Raycast를 시각적으로 표시 (초록색: 슬립스트림 활성, 빨간색: 비활성)
        Color rayColor = _isSlipstream ? Color.magenta : Color.yellow;
        Debug.DrawRay(rayCastPoint.position, rayCastPoint.forward * carStats.slipstreamActivationDistance, rayColor);
    }
    private void HandleAccelerationState()
    {
        switch (currentState)
        {
            case CarState.Accelerating:
                // --- 슬립스트림을 성공했으면 여부에 따라 목표 속도 변경 ---
                float targetSpeed = _isBoosting ? (carStats.maxSpeed + carStats.boostMaxSpeed) : carStats.maxSpeed;
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, carStats.acceleration * Time.deltaTime);
                break;
            case CarState.Decelerating:
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, carStats.decelerationAfterCrash * Time.deltaTime);
                break;
            case CarState.OutOfFuel:
                // 연료가 고갈되면 서서히 멈춥니다.
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, carStats.acceleration * Time.deltaTime);
                break;
            case CarState.NitroBoosting:
                // 일정 슬립스트림 콤보 유지시 니트로 부스트
                float targetSpped = carStats.maxSpeed + carStats.boostMaxSpeed + carStats.nitroBoostSpeed;
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpped, carStats.acceleration * Time.deltaTime); // 더 빠르게 가속
                break;
        }
    }
    // -------------------------

    // 왼쪽으로 이동
    public void MoveLeft()
    {
        if (_isSlipstream)
        {
            TriggerNearMissBoost();
        }
        if (_currentLaneIndex > 0)
        {
            _currentLaneIndex--;
            _targetPosition.x = laneXPositions[_currentLaneIndex];
        }
    }

    // 오른쪽으로 이동
    public void MoveRight()
    {
        if (_currentLaneIndex < laneXPositions.Length - 1)
        {
            if (_isSlipstream)
            {
                TriggerNearMissBoost();
            }
            _currentLaneIndex++;
            _targetPosition.x = laneXPositions[_currentLaneIndex];
        }
    }
    private void TriggerNearMissBoost()
    {
        float distanceFactor = Mathf.InverseLerp(carStats.slipstreamActivationDistance, 0, _slipstreamHit.distance);
        float bonusDuration = carStats.boostBaseDuration + (carStats.boostBonusDuration * distanceFactor);

        // 새로 계산된 시간을 기존 남은 시간에 더해줍니다.
        _boostTimeRemaining += bonusDuration;
    
        Debug.Log($"부스트 성공! {bonusDuration:F2}초 추가! (총 남은 시간: {_boostTimeRemaining:F2}초)");
        
        // --- 콤보 카운트 및 나이트로 버튼 활성화 로직 추가 ---
        _boostComboCount++;
        Debug.Log($"부스트 콤보: {_boostComboCount}");

        if (_boostComboCount >= carStats.nitroComboRequirement)
        {
            if (nitroBoostButton != null)
            {
                nitroBoostButton.gameObject.SetActive(true);
            }
        }
        
        // 만약 부스트 상태가 아니라면, 부스트 코루틴을 시작합니다.
        if (!_isBoosting)
        {
            _boostingCoroutine = StartCoroutine(NearMissBoostCoroutine());
        }
    }

    private IEnumerator NearMissBoostCoroutine()
    {
        _isBoosting = true;

        // _boostTimeRemaining이 0보다 큰 동안 계속 부스트 상태를 유지합니다.
        while (_boostTimeRemaining > 0)
        {
            // 매 프레임 남은 시간을 줄여나갑니다.
            _boostTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        // 시간이 모두 소진되면 부스트를 종료합니다.
        _isBoosting = false;
        _boostComboCount = 0;
        _boostingCoroutine = null;
        Debug.Log("부스트 완전 종료!");
    }
    // --- 슬립스트림 감지 로직 추가 ---
    private void CheckForSlipstream()
    {
        // 차의 바로 앞에서 앞 방향으로 Ray를 쏴서 다른 차가 있는지 확인
        if (Physics.Raycast(rayCastPoint.position, rayCastPoint.forward, out _slipstreamHit, carStats.slipstreamActivationDistance, carStats.otherCarLayer))
        {
            // Ray에 다른 차가 감지되면 슬립스트림 상태로 설정
            //Debug.Log("앞 차 감지! 거리: " + _slipstreamHit.distance); 
            _isSlipstream = true;
        }
        else
        {
            // 감지되지 않으면 슬립스트림 해제
            _isSlipstream = false;
        }
        splipstreamDistanceText.color = (_isSlipstream) ? Color.green :  Color.red;
    }    

    // 위치를 부드럽게 업데이트
    private void UpdatePosition()
    {
        // 현재 위치에서 목표 위치로 부드럽게 이동 (Lerp)
        //transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * laneChangeSpeed);
        // 1. 현재 위치를 임시 변수에 저장합니다.
        Vector3 currentPos = transform.position;

        // 2. X축 위치만! 목표 지점을 향해 부드럽게 이동시킵니다.
        // Vector3.Lerp 대신 X축 값만 계산하는 Mathf.Lerp를 사용합니다.
        currentPos.x = Mathf.Lerp(currentPos.x, _targetPosition.x, Time.deltaTime * laneChangeSpeed);

        // 3. Z축과 Y축은 그대로 둔 채, 변경된 X축 값만 최종적으로 적용합니다.
        transform.position = currentPos;
    }
    
    // 틸트(기울기) 효과 업데이트
    private void UpdateTilt()
    {
        // 목표 x좌표와 현재 x좌표의 차이를 계산하여 이동 방향을 파악
        float moveDirection = _targetPosition.x - transform.position.x;

        // 이동 방향에 따라 목표 회전값을 설정
        if (Mathf.Abs(moveDirection) > 0.05f) // 약간의 오차 범위를 둠
        {
            // 오른쪽으로 움직이면 왼쪽으로, 왼쪽으로 움직이면 오른쪽으로 기울어집니다.
            _targetRotation = Quaternion.Euler(0, 0, -Mathf.Sign(moveDirection) * tiltAngle);
        }
        else
        {
            // 움직임이 멈추면 원래 각도로 복귀
            _targetRotation = Quaternion.identity;
        }
        
        // 현재 각도에서 목표 각도로 부드럽게 회전 (Slerp)
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * tiltSpeed);
    }
    
    // [수정] UpdateTilt()를 아래의 UpdateYawRotation()으로 완전히 교체합니다.
    private void UpdateYawRotation()
    {
        // 목표 x좌표와 현재 x좌표의 차이를 계산하여 이동 방향을 파악
        float moveDirection = _targetPosition.x - transform.position.x;
        float targetYaw = 0f;

        // 이동 방향에 따라 목표 회전값을 설정 (Y축 기준)
        if (Mathf.Abs(moveDirection) > 0.1f) // 차선을 변경 중일 때
        {
            // 오른쪽으로 움직이면 양수, 왼쪽으로 움직이면 음수 각도로 회전
            targetYaw = Mathf.Sign(moveDirection) * yawAngle;
        }
        
        // 목표 회전값(Quaternion)을 생성합니다. Y축으로 회전하도록 설정합니다.
        _targetRotation = Quaternion.Euler(0, targetYaw, 0);
        
        // 현재 각도에서 목표 각도로 부드럽게 회전 (Slerp)
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * rotationSpeed);
    }
    // --- 충돌 감지 ---
    private void OnCollisionEnter(Collision collision)
    {
        // 연료가 고갈된 상태에서는 충돌 로직을 실행하지 않음
        if (currentState == CarState.OutOfFuel) 
            return;
        
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
            // --- 나이트로 상태일 때의 충돌 처리 (핵심) ---
            if (currentState == CarState.NitroBoosting)
            {                
                if (otherRb != null)
                {
                    // 상대방만 날려버림 (넉백 현상 없음)
                    Vector3 forceDirection = (collision.transform.position - transform.position).normalized + (Vector3.up * 0.5f);
                    otherRb.AddForce(forceDirection * carStats.nitroCollisionForce, ForceMode.Impulse);
                }
                // 내 차는 아무런 영향을 받지 않고, 함수를 즉시 종료
                return;
            }
            // -----------------------------------------

            // --- 일반 충돌 시 콤보 초기화 ---
            _boostComboCount = 0;
            if (nitroBoostButton != null)
            {
                nitroBoostButton.gameObject.SetActive(false);
            }
            
            // 1. 내 차의 상태를 '감속 중'으로 변경
            currentState = CarState.Decelerating;
            _isSlipstream = false;
            // --- 부스트 강제 종료 로직 추가 ---
            if (_isBoosting)
            {
                if (_boostingCoroutine != null)
                {
                    StopCoroutine(_boostingCoroutine); // 간단하게 모든 코루틴 중지    
                }                
                _isBoosting = false;
                _boostComboCount = 0;
                _boostTimeRemaining = 0f;
            }            
            
            // 2. 카메라를 흔들어 충격 효과를 줍니다. (0.3초 동안 0.5 강도로)
            if (mainCamera != null)
            {
                mainCamera.StartShake(0.3f, 0.5f);
            }
            
            // 3. 부딪힌 상대방 차량에 물리적 힘을 가합니다.
            if (otherRb != null)
            {
                // 충돌 지점 정보를 가져옵니다.
                ContactPoint contact = collision.contacts[0];            
            
                // 3-1. 기본적인 수평 방향의 힘 계산
                Vector3 forceDirection = (collision.transform.position - transform.position).normalized;
            
                // --- 3-2. Y축 방향으로 힘 추가 (핵심 수정 부분) ---
                forceDirection += Vector3.up * upwardImpactModifier;
                // ----------------------------------------------------

                float forceMagnitude = 20f;//currentSpeed * 2f;

                // 3-3. 위쪽 방향이 추가된 최종 힘을 가함
                otherRb.AddForceAtPosition(forceDirection.normalized * forceMagnitude, contact.point, ForceMode.Impulse);
            }

            // 4.내 차의 시각적 모델을 흔드는 효과
            if (carVisualModel != null)
            {
                // 만약 이미 실행 중인 흔들림 코루틴이 있다면 멈춥니다.
                if (_carShakeCoroutine != null)
                {
                    StopCoroutine(_carShakeCoroutine);
                }
                
                //(시간, 회전 강도, 넉백 거리)
                _carShakeCoroutine = StartCoroutine(ShakeCarModelCoroutine(0.6f, 2.0f, 1.5f));                                 
            }
            // ----------------------------------------------------    
        }        
    }
    // --- 차량 모델을 흔드는 코루틴 (새로 추가된 함수) ---
    private IEnumerator ShakeCarModelCoroutine(float duration, float magnitude, float knockbackDistance)
    {
        // 시작 시 모델의 원래 위치와 회전값을 저장
        Quaternion originalRotation = carVisualModel.localRotation;
        Vector3 originalPosition = carVisualModel.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // --- 1. 회전 흔들림 (기존과 동일) ---
            float z = Random.Range(-1f, 1f) * magnitude;
            float x = Random.Range(-1f, 1f) * magnitude;
            Quaternion randomRotation = Quaternion.Euler(x, 0, z);
            carVisualModel.localRotation = originalRotation * randomRotation;

            // --- 2. 뒤로 물러났다 돌아오는 넉백 (새로 추가) ---
            // Sin 함수를 이용해 부드럽게 뒤로 갔다가 돌아오는 곡선을 만듭니다.
            float knockback = Mathf.Sin(elapsed / duration * Mathf.PI) * -knockbackDistance;
            carVisualModel.localPosition = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z + knockback);
            // ----------------------------------------------------

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 효과가 끝나면 모델을 정확히 원래 위치와 회전값으로 복귀
        carVisualModel.localRotation = Quaternion.identity;//originalRotation;
        carVisualModel.localPosition = Vector3.zero; //originalPosition;
        
        // --- 코루틴이 끝나는 시점에 상태를 '가속 중'으로 복구! (핵심 추가 부분) ---
        // --- 중요: 연료가 있을 때만 다시 가속 상태로 전환 ---
        if (currentState != CarState.OutOfFuel)
        {
            currentState = CarState.Accelerating;            
        }
        
        _carShakeCoroutine = null;
        Debug.Log("충격 효과 종료. 다시 가속합니다!");
    }
    // ------------------------------------------------
    // --- 나이트로 부스트 코루틴 (새로 추가) ---
    private IEnumerator NitroBoostCoroutine()
    {
        // 기존 부스트는 모두 중단
        if (_boostingCoroutine != null) StopCoroutine(_boostingCoroutine);
        _isBoosting = false;
        _boostTimeRemaining = 0;
        _boostComboCount = 0;
        
        // 나이트로 상태로 전환
        currentState = CarState.NitroBoosting;
        Debug.Log("나이트로 부스트 발동!");

        // 정해진 시간만큼 대기
        yield return new WaitForSeconds(carStats.nitroBoostDuration);

        // 나이트로가 끝나면 다시 일반 가속 상태로 복귀
        if (currentState == CarState.NitroBoosting)
        {
            currentState = CarState.Accelerating;
            Debug.Log("나이트로 부스트 종료!");
        }
    }
}