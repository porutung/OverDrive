using UnityEngine;
using System.Collections;
public class ChaseCamera : MonoBehaviour
{
    [Header("타겟 및 컨트롤러")]
    public Transform target; // 따라갈 자동차의 Transform
    public PlayerCarController playerCarController; // 플레이어 차량 컨트롤러 참조

    [Header("카메라 기본 추적 설정")]
    [SerializeField] private float followSmoothTime = 0.1f;
    [SerializeField] private float horizontalSmoothTime = 5.0f;

    [Header("일반 주행 시 설정")]
    [SerializeField] private Vector3 normalOffset = new Vector3(0, 2f, -5f);
    [SerializeField] private float normalFOV = 75f;

    [Header("슬립스트림 시 설정")]
    [SerializeField] private Vector3 slipstreamOffset = new Vector3(0, 1.8f, -4.5f);
    [SerializeField] private float slipstreamFOV = 90f;

    [Header("효과 전환 속도")]
    [SerializeField] private float fovChangeSpeed = 5f;
    [SerializeField] private float offsetChangeSpeed = 5f;

    // 내부 계산용 변수들
    private Camera mainCamera;
    private Vector3 _velocity = Vector3.zero;
    private float _cameraTargetX;
    private Vector3 _currentOffset;
    private Coroutine _shakeCoroutine;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (target != null)
        {
            _cameraTargetX = target.position.x;
            _currentOffset = normalOffset;
            transform.position = target.position + _currentOffset;
            mainCamera.fieldOfView = normalFOV;
        }
    }

    void LateUpdate()
    {
        if (target == null || playerCarController == null) return;

        // 1. 슬립스트림 상태에 따라 목표 FOV와 Offset 결정
        bool isSlipstreaming = playerCarController.IsInSlipstream(); // isSlipstream을 메서드로 호출
        float targetFOV = isSlipstreaming ? slipstreamFOV : normalFOV;
        Vector3 targetOffset = isSlipstreaming ? slipstreamOffset : normalOffset;

        // 2. 현재 FOV와 Offset을 목표 값으로 부드럽게 변경
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
        _currentOffset = Vector3.Lerp(_currentOffset, targetOffset, Time.deltaTime * offsetChangeSpeed);

        // 3. 차를 부드럽게 따라가는 최종 목표 위치 계산
        _cameraTargetX = Mathf.Lerp(_cameraTargetX, target.position.x, Time.deltaTime * horizontalSmoothTime);
        Vector3 desiredPosition = target.position + target.TransformDirection(_currentOffset);
        desiredPosition.x = _cameraTargetX;

        // 4. 최종 위치로 카메라 이동
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, followSmoothTime);

        // 5. 항상 차를 바라보도록 설정
        transform.LookAt(target);
    }

    // --- 쉐이크 시작 함수 ---
    public void StartShake(float duration, float magnitude)
    {
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    // --- 실제 쉐이크 로직 ---
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.position += new Vector3(x, y, 0) * Time.deltaTime * 20;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}