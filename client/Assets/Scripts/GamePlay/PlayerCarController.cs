using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerCarController : MonoBehaviour
{
    public ChaseCamera mainCamera; // 인스펙터에서 메인 카메라를 연결해줘야 함
    public TextMeshProUGUI speedText;
    
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

    [Header("속도 및 슬립스트림")]
    public bool isSlipstream = false;
    public bool IsInSlipstream() { return isSlipstream; }
    
    [Header("RayCast 위치값")]
    [SerializeField]private Transform rayCastPoint;
    
    // --- 자동 가속/감속 로직 변수 ---
    private enum CarState { Accelerating, Decelerating }
    private CarState currentState;
    
    public float currentSpeed; // 현재 속도 (RoadScroller가 참조)
    [SerializeField] private float accelerationRate = 5f; // <--- 새롭게 추가된 부분: 속도 가감 속도
    
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
    
    void Start()
    {        
        // 차선 이동 관련 초기화
        _targetPosition = transform.position;
        _targetPosition.x = laneXPositions[_currentLaneIndex];
        _targetRotation = transform.rotation;
        
        // 자동 가속 관련 초기화
        currentState = CarState.Accelerating;
    }

    void Update()
    {
        // 1. 슬립스트림 조건 확인 (가속 상태일 때만)
        if (currentState == CarState.Accelerating)
        {
            CheckForSlipstream();
        }
        else
        {
            isSlipstream = false; // 감속 중에는 슬립스트림 비활성화
        }
        
        //  2. 자동 가속/감속 상태 관리
        HandleAccelerationState();
        
        // 계산된 현재 속력으로 차를 앞으로 이동시킵니다.
        //transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
        
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
        
        // Raycast를 시각적으로 표시 (초록색: 슬립스트림 활성, 빨간색: 비활성)
        Color rayColor = isSlipstream ? Color.magenta : Color.yellow;
        Debug.DrawRay(rayCastPoint.position, rayCastPoint.forward * carStats.slipstreamActivationDistance, rayColor);
    }
    private void HandleAccelerationState()
    {
        switch (currentState)
        {
            case CarState.Accelerating:
                // --- 슬립스트림 여부에 따라 목표 속도 변경 ---
                float targetSpeed = isSlipstream ? (carStats.maxSpeed + carStats.slipstreamMaxSpeed) : carStats.maxSpeed;
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, carStats.acceleration * Time.deltaTime);
                // ------------------------------------------
                break;
            case CarState.Decelerating:
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, carStats.decelerationAfterCrash * Time.deltaTime);
                break;
        }
    }
    // -------------------------

    // 왼쪽으로 이동
    public void MoveLeft()
    {
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
            _currentLaneIndex++;
            _targetPosition.x = laneXPositions[_currentLaneIndex];
        }
    }
    
    // --- 슬립스트림 감지 로직 추가 ---
    private void CheckForSlipstream()
    {
        // 차의 바로 앞에서 앞 방향으로 Ray를 쏴서 다른 차가 있는지 확인
        RaycastHit hit;
        if (Physics.Raycast(rayCastPoint.position, rayCastPoint.forward, out hit, carStats.slipstreamActivationDistance, carStats.otherCarLayer))
        {
            // Ray에 다른 차가 감지되면 슬립스트림 상태로 설정
            Debug.Log("앞 차 감지! 거리: " + hit.distance); 
            isSlipstream = true;
        }
        else
        {
            // 감지되지 않으면 슬립스트림 해제
            isSlipstream = false;
        }
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
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            // 1. 내 차의 상태를 '감속 중'으로 변경
            currentState = CarState.Decelerating;
            isSlipstream = false;
            
            // 2. 카메라를 흔들어 충격 효과를 줍니다. (0.3초 동안 0.5 강도로)
            if (mainCamera != null)
            {
                mainCamera.StartShake(0.3f, 0.5f);
            }
            // 3. 부딪힌 상대방 차량에 물리적 힘을 가합니다.
            Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
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
        currentState = CarState.Accelerating;
        _carShakeCoroutine = null;
        Debug.Log("충격 효과 종료. 다시 가속합니다!");
    }
    // ------------------------------------------------
}