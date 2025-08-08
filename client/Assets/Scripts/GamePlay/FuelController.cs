using UnityEngine;

public class FuelController : MonoBehaviour
{
    [Header("회전 속도")]
    [Tooltip("초당 회전할 각도")]
    public float rotationSpeed = 50f;
    
    [SerializeField] float addFuelAmount = 10f;

    public float AddFuelAmount { get{ return  addFuelAmount; } }    
    
    void Update()
    {
        // Y축(Vector3.up)을 기준으로 초당 rotationSpeed만큼 회전시킵니다.
        // Time.deltaTime을 곱해 컴퓨터 성능과 관계없이 일정한 속도를 유지합니다.
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }
    private void OnTriggerEnter(Collider other)
    {
        // "Player" 태그를 가진 오브젝트와 충돌했는지 확인
        if (other.CompareTag("Player"))
        {
            PlayerCarController player = other.GetComponent<PlayerCarController>();
            if (player != null)
            {
                // 플레이어의 연료를 채워주는 함수 호출
                player.AddFuel(addFuelAmount);
                // 아이템 파괴
                Destroy(gameObject);
            }
        }
    }
}
