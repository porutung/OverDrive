using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    [Header("차선 설정")]
    [SerializeField] private float[] laneXPositions = { -2.5f, 0f, 2.5f }; // 3개 차선의 x좌표
    [SerializeField] private float laneChangeSpeed = 15f; // 차선 변경 속도
    private int _currentLaneIndex = 1; // 현재 차선 (가운데에서 시작)

    [Header("속도 및 슬립스트림")]
    public float normalSpeed = 20f; // 기본 속도
    [SerializeField] private float slipstreamSpeed = 30f; // 슬립스트림 시 속도
    [SerializeField] private float slipstreamDistance = 10f; // 슬립스트림 발동 거리
    [SerializeField] private LayerMask otherCarLayer; // 다른 차들의 레이어
    public float currentSpeed; // 현재 속도 (RoadScroller가 참조)

    [Header("틸트(기울기) 효과")]
    [SerializeField] private float tiltAngle = 15f; // 최대 기울기 각도
    [SerializeField] private float tiltSpeed = 10f; // 기울기 변화 속도
    
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

    // 슬립스트림 체크
    private void CheckForSlipstream()
    {
        // 차 앞에서 Ray를 쏴서 다른 차가 있는지 확인합니다.
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.forward, out hit, slipstreamDistance, otherCarLayer))
        {
            // 다른 차가 감지되면 슬립스트림 속도로 변경
            currentSpeed = slipstreamSpeed;
        }
        else
        {
            // 감지되지 않으면 기본 속도로 복귀
            currentSpeed = normalSpeed;
        }
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
}