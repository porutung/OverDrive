using UnityEngine;

public class OtherCar : MonoBehaviour
{
    public float speed; // 각 차량의 속도
    public PlayerCarController playerCar;
    void Update()
    {        
        if (playerCar == null) 
            return;

        if (playerCar.CurrentState == PlayerCarController.CarState.OutOfFuel)
            return;
            
        float relativeSpeed = playerCar.currentSpeed - this.speed;
        transform.Translate(Vector3.back * relativeSpeed * Time.deltaTime, Space.World);

        // 화면 밖으로 나가면 스스로 파괴
        if (transform.position.z < playerCar.transform.position.z - 20) // 임의의 파괴 지점
        {
            Destroy(gameObject);
        }
    }
}