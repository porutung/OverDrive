using UnityEngine;

[CreateAssetMenu(fileName = "NewCarStats", menuName = "Racing/Car Stats")]
public class CarStats : ScriptableObject
{
    [Header("주행 성능")]
    public float maxSpeed = 100f; // 최고 속도
    [Tooltip("최고 속도까지 도달하는 가속도 (이 값이 높을수록 '제로백'이 빠름)")]
    public float acceleration = 15f; // 가속도

    [Header("충돌 패널티")]
    [Tooltip("충돌 후 속력이 0으로 줄어드는 감속도")]
    public float decelerationAfterCrash = 25f; // 충돌 후 감속도
    
    // --- 슬립스트림 데이터 추가 ---
    [Header("슬립스트림")]    
    [Tooltip("슬립스트림이 발동되는 앞 차와의 최대 거리")]
    public float slipstreamActivationDistance = 15f;
    [Tooltip("앞 차를 감지할 때 사용할 레이어")]
    public LayerMask otherCarLayer;
    // ---------------------------
    // --- 부스트 데이터 추가 ---
    [Header("아슬하게 피하기 부스트")]
    [Tooltip("부스트가 지속되는 기본 시간")]
    public float boostBaseDuration = 1.0f;
    [Tooltip("가장 아슬하게 피했을 때 추가되는 최대 시간")]
    public float boostBonusDuration = 0.5f;
    [Tooltip("부스트 시의 최고 속도")]
    public float boostMaxSpeed = 50f;
    
    // --- 연료 데이터 추가 ---
    [Header("연료 시스템")]
    [Tooltip("최대 연료량")]
    public float maxFuel = 100f;
    [Tooltip("초당 연료 소모량")]
    public float fuelConsumptionRate = 1f;
    
    // --- Nitro 부스트 데이터 추가 ---
    [Header("Nitro 부스트")]
    [Tooltip("Nitro 부스트를 활성화하는 데 필요한 연속 콤보 횟수")]
    public int nitroComboRequirement = 10;
    [Tooltip("Nitro 부스트의 지속 시간")]
    public float nitroBoostDuration = 5f;
    [Tooltip("Nitro 부스트 시의 최고 속도")]
    public float nitroBoostSpeed = 100f;
    [Tooltip("Nitro 부스트 시 상대 차량에게 가할 힘의 크기")]
    public float nitroCollisionForce = 100f;
    
}