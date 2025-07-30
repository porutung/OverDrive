using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    [Header("차선 설정")]
    [SerializeField] private float[] laneXPositions = { -2.5f, 0f, 2.5f }; // 3개 차선의 x좌표
    [SerializeField] private float laneChangeSpeed = 15f; // 차선 변경 속도
    private int _currentLaneIndex = 1; // 현재 차선 (가운데에서 시작)

    [Header("속도 및 슬립스트림")]
    public bool isSlipstream = false;
    public float normalSpeed = 20f; // 기본 속도
    public float slipstreamSpeed = 30f; // 슬립스트림 시 속도
    
    [SerializeField] private float slipstreamDistance = 10f; // 슬립스트림 발동 거리
    [SerializeField] private LayerMask otherCarLayer; // 다른 차들의 레이어
    public float currentSpeed; // 현재 속도 (RoadScroller가 참조)
    [SerializeField] private float accelerationRate = 5f; // <--- 새롭게 추가된 부분: 속도 가감 속도
    
    [Header("틸트(기울기) 효과")]
    [SerializeField] private float tiltAngle = 15f; // 최대 기울기 각도
    [SerializeField] private float tiltSpeed = 10f; // 기울기 변화 속도
    
    // [수정] 이 부분을 '틸트'에서 '요'로 변경합니다.
    [Header("요(Yaw) 회전 효과")]
    [SerializeField] private float yawAngle = 10f; // 차선 변경 시 최대 회전 각도
    [SerializeField] private float rotationSpeed = 10f; // 회전 변화 속도
    
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    
    void Start()
    {
        currentSpeed = normalSpeed;
        _targetPosition = transform.position;
        _targetPosition.x = laneXPositions[_currentLaneIndex];
        _targetRotation = transform.rotation;
    }

    void Update()
    {
        CheckForSlipstream();
        UpdatePosition();
        UpdateTilt();
       // [수정] 호출하는 메서드 이름을 변경합니다.
       //UpdateYawRotation();
    }

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

    public void SpeedUp()
    {
        normalSpeed += 10 * Time.deltaTime;
    }

    public void SpeedDown()
    {
        normalSpeed -= 10 * Time.deltaTime;
    }
    // 슬립스트림 체크
    private void CheckForSlipstream()
    {
        float targetSpeed; // 이번 프레임의 목표 속도
        // 차 앞에서 Ray를 쏴서 다른 차가 있는지 확인합니다.
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.forward, out hit, slipstreamDistance, otherCarLayer))
        {
            // 다른 차가 감지되면 슬립스트림 속도를 목표로 설정
            targetSpeed = slipstreamSpeed;
            isSlipstream = true;
        }
        else
        {
            // 감지되지 않으면 기본 속도를 목표로 설정
            targetSpeed = normalSpeed;
            isSlipstream = false;
        }
        
        // 현재 속도를 목표 속도로 부드럽게 가속/감속합니다.
        // Mathf.MoveTowards는 현재 값(currentSpeed)을 목표 값(targetSpeed)으로
        // 최대 이동량(accelerationRate * Time.deltaTime)만큼 이동시킵니다.
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate * Time.deltaTime);
    }

    // 위치를 부드럽게 업데이트
    private void UpdatePosition()
    {
        // 현재 위치에서 목표 위치로 부드럽게 이동 (Lerp)
        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * laneChangeSpeed);
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
}