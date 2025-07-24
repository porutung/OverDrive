using UnityEngine;

public class OtherCar : MonoBehaviour
{
    public float speed; // 각 차량의 속도

    void Update()
    {
        // 자신의 속도로 앞으로 전진 (플레이어 기준으로는 뒤로 가는 것처럼 보임)
        transform.Translate(Vector3.back * speed * Time.deltaTime);

        // 화면 밖으로 나가면 스스로 파괴
        if (transform.position.z < -20) // 임의의 파괴 지점
        {
            Destroy(gameObject);
        }
    }
}