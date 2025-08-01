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
    [Tooltip("슬립스트림 시 도달할 최고 속도")]
    public float slipstreamMaxSpeed = 30f;  // 최대 속도에서 증가할 증가속도
    [Tooltip("슬립스트림이 발동되는 앞 차와의 최대 거리")]
    public float slipstreamActivationDistance = 15f;
    [Tooltip("앞 차를 감지할 때 사용할 레이어")]
    public LayerMask otherCarLayer;
    // ---------------------------
}