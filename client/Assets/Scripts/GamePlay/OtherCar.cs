using UnityEngine;

public class OtherCar : MonoBehaviour
{
    public float speed; // 각 차량의 속도
    public PlayerCarController playerCar;
    void Update()
    {
        
        if (playerCar == null) return;

        // 플레이어 속도와 내 속도의 차이만큼, 즉 '상대 속도'로 뒤로 움직입니다.
        //float relativeSpeed = playerCar.currentSpeed - speed;
        transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);

        // 화면 밖으로 나가면 스스로 파괴
        if (transform.position.z < -20) // 임의의 파괴 지점
        {
            Destroy(gameObject);
        }
    }
}