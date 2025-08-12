using UnityEngine;
using System.Collections;

public class AdvancedChaseCamera : MonoBehaviour
{
    // 카메라 시점 종류를 정의하는 enum
    public enum CameraView { ThirdPerson, QuarterView }
    private CameraView _currentView = CameraView.ThirdPerson;

    [Header("타겟 및 컨트롤러")]
    public Transform target;
    public PlayerCarController playerCarController;

    [Header("3인칭 시점 설정")]
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0, 2f, -5f);
    [SerializeField] private float thirdPersonFOV = 75f;
    
    // --- 1인칭(나이트로) 시점 설정 추가 ---
    [Header("1인칭 시점 (나이트로)")]
    [Tooltip("운전석 시점의 위치 오프셋")]
    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0, 0.7f, 0.5f);
    [Tooltip("1인칭일 때의 시야각 (속도감 강조)")]
    [SerializeField] private float firstPersonFOV = 100f;

    [Header("쿼터뷰 시점 설정")]
    [SerializeField] private Vector3 quarterViewOffset = new Vector3(0, 8f, -7f); // 더 높고 멀리
    [SerializeField] private float quarterViewRotationX = 45f; // 아래를 더 많이 보도록 회전
    [SerializeField] private float quarterViewFOV = 60f; // 쿼터뷰는 보통 FOV가 더 낮음

    [Header("슬립스트림 시 효과")]
    [SerializeField] private float fovBoostAmount = 15f; // 부스트 시 FOV 증가량

    [Header("효과 전환 속도")]
    [SerializeField] private float transitionSpeed = 5f;

    // 내부 계산용 변수들
    private Camera mainCamera;
    private Vector3 _velocity = Vector3.zero;
    private Coroutine _shakeCoroutine;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        // 시작 시점을 3인칭으로 설정
        transform.position = target.position + thirdPersonOffset;
        transform.rotation = Quaternion.Euler(10, 0, 0); // 3인칭 초기 회전값
        mainCamera.fieldOfView = thirdPersonFOV;
    }

    // --- 시점 전환을 위한 public 함수 (UI 버튼에서 호출) ---
    public void ToggleCameraView()
    {
        _currentView = (_currentView == CameraView.ThirdPerson) ? CameraView.QuarterView : CameraView.ThirdPerson;
        Debug.Log($"카메라 시점 변경: {_currentView}");
    }

    void LateUpdate()
    {
        if (target == null || playerCarController == null) return;

        // 1. 현재 시점과 부스트 상태에 따라 목표 값 결정
        bool isBoosting = playerCarController.IsBoosting() || playerCarController.IsInSlipstream();
        bool isNitroActive = playerCarController.IsNitroBoosting();
        
        Vector3 targetOffset;
        Quaternion targetRotation;
        float targetFOV;

        if (_currentView == CameraView.QuarterView)
        {
            // 쿼터뷰일 때의 목표 값
            targetOffset = quarterViewOffset;
            targetRotation = Quaternion.Euler(quarterViewRotationX, 0, 0);
            targetFOV = isBoosting ? quarterViewFOV + fovBoostAmount : quarterViewFOV;
        }
        else // ThirdPerson
        {
            if (isNitroActive)
            {
                // 3인칭 상태에서 나이트로 사용 시 -> 1인칭으로 전환
                targetOffset = firstPersonOffset;
                targetRotation = target.rotation; // 차의 정면 방향을 그대로 따라감
                targetFOV = firstPersonFOV;
                transform.position = Vector3.Lerp(transform.position, target.position + target.TransformDirection(targetOffset), Time.deltaTime * transitionSpeed * 2f); // 더 빠르게 전환
            }
            else
            {
                // 일반 3인칭 상태
                targetOffset = thirdPersonOffset;
                targetRotation = Quaternion.LookRotation(target.position - transform.position);
                targetFOV = isBoosting ? thirdPersonFOV + fovBoostAmount : thirdPersonFOV;
                
                // 부드러운 좌우 추적 로직 (기존과 동일)
                float targetX = Mathf.Lerp(transform.position.x, target.position.x, Time.deltaTime * 5f);
                Vector3 desiredPosition = target.position + targetOffset;
                desiredPosition.x = targetX;
                transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, 0.1f);
            }
        }

        // 2. 최종 목표 값을 향해 부드럽게 전환
        if (_currentView == CameraView.QuarterView)
        {
            transform.position = Vector3.Lerp(transform.position, target.position + targetOffset, Time.deltaTime * transitionSpeed);
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);
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