using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    [Header("타겟 설정")]
    public Transform target; // 따라갈 자동차의 Transform

    [Header("카메라 오프셋")]
    public Vector3 offset = new Vector3(0, 2f, -5f); // 차와의 기본 거리

    [Header("추적 속도")]
    [Tooltip("전후/상하 추적의 부드러움 (값이 작을수록 빨리 따라감)")]
    public float followSmoothTime = 0.1f;
    
    [Tooltip("좌우(차선 변경) 추적의 부드러움 (값이 클수록 천천히 따라감)")]
    public float horizontalSmoothTime = 5.0f;

    // 내부 계산용 변수들
    private Vector3 _velocity = Vector3.zero;
    private float _cameraTargetX;

    void Start()
    {
        if (target == null) return;
        
        // 시작할 때의 카메라 X축 목표 위치를 타겟의 현재 위치로 초기화
        _cameraTargetX = target.position.x;
    }

    // 모든 물리 업데이트가 끝난 후 호출되어 카메라 떨림 방지
    void LateUpdate()
    {
        if (target == null) return;

        // 1. 좌우(X축) 목표 위치를 부드럽게 계산합니다.
        // 현재 카메라의 목표 X위치를 실제 타겟의 X위치로 서서히 이동시킵니다.
        _cameraTargetX = Mathf.Lerp(_cameraTargetX, target.position.x, Time.deltaTime * horizontalSmoothTime);

        // 2. 전후/상하(Z, Y축)를 포함한 기본 목표 위치를 계산합니다.
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        
        // 3. 계산된 좌우 목표 위치(_cameraTargetX)를 최종 목표 위치에 반영합니다.
        desiredPosition.x = _cameraTargetX;

        // 4. 최종 목표 위치로 카메라 전체를 부드럽게 이동시킵니다.
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, followSmoothTime);
        
        // 5. 항상 차를 바라보도록 설정합니다.
        //transform.LookAt(target);
    }
}