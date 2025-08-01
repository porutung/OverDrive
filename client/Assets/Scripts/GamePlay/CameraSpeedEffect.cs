using UnityEngine;

public class CameraSpeedEffect : MonoBehaviour
{
    [Header("카메라 설정")]
    [SerializeField] private Camera mainCamera; // 메인 카메라 참조
    [SerializeField] private PlayerCarController playerCarController; // 플레이어 차량 컨트롤러 참조

    [Header("FOV 설정")]
    [SerializeField] private float normalFOV = 75f; // 일반 주행 시 FOV
    [SerializeField] private float slipstreamFOV = 80f; // 슬립스트림 시 목표 FOV
    [SerializeField] private float fovChangeSpeed = 5f; // FOV 변경 속도

    // (선택 사항) 카메라 위치 오프셋 설정
    // [Header("카메라 위치 오프셋")]
     //[SerializeField] private Vector3 normalOffset = new Vector3(0, 5, -10); // 일반 주행 시 카메라 상대 위치
     [SerializeField] private Vector3 normalOffset = new Vector3(0, 2f, -5f); // 차와의 기본 거리
     [SerializeField] private Vector3 slipstreamOffset = new Vector3(0, 1.2f, -3.5f); // 슬립스트림 시 카메라 상대 위치
     [SerializeField] private float offsetChangeSpeed = 5f; // 오프셋 변경 속도


    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // 메인 카메라가 할당되지 않았다면 'MainCamera' 태그를 가진 카메라를 찾습니다.
        }
        if (playerCarController == null)
        {
            // 플레이어 차량 컨트롤러가 할당되지 않았다면 부모 또는 씬에서 찾을 수 있습니다.
            // 예시: this.transform.parent.GetComponent<PlayerCarController>();
            Debug.LogError("PlayerCarController가 할당되지 않았습니다. Inspector에서 할당해주세요.");
        }

        // 초기 FOV 설정
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = normalFOV;
        }
        // 초기 위치 설정 (선택 사항)
        transform.localPosition = normalOffset;
    }

    void Update()
    {
        if (mainCamera == null || playerCarController == null)
        {
            return;
        }

        // 현재 목표 FOV 결정
        float targetFOV = normalFOV;
        if(playerCarController.isSlipstream == true)
        //if (playerCarController.currentSpeed >= playerCarController.slipstreamSpeed - 0.5f) // 슬립스트림 속도에 도달했는지 확인 (오차 범위)
        {
            targetFOV = slipstreamFOV;
        }
        // else if (playerCarController.currentSpeed <= playerCarController.normalSpeed + 0.5f) // 일반 속도에 도달했는지 확인 (오차 범위)
        // {
        //     targetFOV = normalFOV;
        // }


        // FOV를 목표 FOV로 부드럽게 변경
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);

        // (선택 사항) 카메라 위치 오프셋 변경
         Vector3 targetOffset = normalOffset;
         //if (playerCarController.currentSpeed >= playerCarController.slipstreamSpeed - 0.5f)
         if(playerCarController.isSlipstream == true)
         {
             targetOffset = slipstreamOffset;
         }
         transform.localPosition = Vector3.Lerp(transform.localPosition, targetOffset, Time.deltaTime * offsetChangeSpeed);

         
        // (선택 사항) 카메라가 플레이어를 따라가도록 설정
        // 이 스크립트를 카메라 오브젝트에 붙이고, 카메라를 플레이어의 자식으로 두지 않았다면
        //transform.position = playerCarController.transform.position + transform.rotation * currentOffset;
    }
}