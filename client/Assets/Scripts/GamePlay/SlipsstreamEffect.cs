using UnityEngine;
using UnityEngine.Rendering; // Volume 관련 네임스페이스
using UnityEngine.Rendering.Universal; // URP의 포스트 프로세싱 효과 네임스페이스
public class SlipsstreamEffect : MonoBehaviour
{
    [Tooltip("효과를 제어할 Global Volume")]
    public Volume postProcessVolume;

    [Tooltip("슬립스트림 시 모션 블러 강도")]
    [Range(0f, 1f)]
    public float slipstreamBlurIntensity = 0.5f;

    [Tooltip("효과가 켜지고 꺼지는 속도")]
    public float effectLerpSpeed = 5f;

    // 제어할 모션 블러 효과를 저장할 변수
    private MotionBlur _motionBlur;
    // 목표로 하는 블러 강도
    private float _targetIntensity = 0f;
    
    [SerializeField] private PlayerCarController _playerCar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Volume 프로파일에서 MotionBlur 컴포넌트를 찾아옵니다.
        // TryGet()는 해당 오버라이드가 없으면 false를 반환하여 안전합니다.
        postProcessVolume.profile.TryGet(out _motionBlur);
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerCar.isSlipstream)
        {
            // 슬립스트림 중이면 목표 강도를 설정값으로
            _targetIntensity = slipstreamBlurIntensity;
        }
        else
        {
            // 슬립스트림이 아니면 목표 강도를 0으로
            _targetIntensity = 0f;
        }
        
        // 현재 블러 강도를 목표 강도까지 부드럽게 변화시킵니다.
        if (_motionBlur != null)
        {
            _motionBlur.intensity.value = Mathf.Lerp(
                _motionBlur.intensity.value, 
                _targetIntensity, 
                Time.deltaTime * effectLerpSpeed
            );
        }
    }
}
