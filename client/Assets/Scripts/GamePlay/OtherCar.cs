using UnityEngine;

public class OtherCar : MonoBehaviour
{
    public float speed;
    public float _currentSpeed; // 각 차량의 속도
    public PlayerCarController playerCar;
    void Update()
    {        
        if (playerCar == null) 
            return;

        if (playerCar.CurrentState == PlayerCarController.CarState.OutOfFuel)
            return;

        float targetSpeed = speed;
        if (playerCar.IsBoosting())
        {
            targetSpeed += playerCar.carStats.boostMaxSpeed;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, playerCar.carStats.acceleration * Time.deltaTime);
        }
        else if (playerCar.IsNitroBoosting())
        {
            targetSpeed += playerCar.carStats.nitroBoostSpeed;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, playerCar.carStats.acceleration * Time.deltaTime);
        }
        else
        {
            _currentSpeed = targetSpeed;
        }
                
        transform.Translate(Vector3.back * _currentSpeed * Time.deltaTime, Space.World);

        // 화면 밖으로 나가면 스스로 파괴
        if (transform.position.z < playerCar.transform.position.z - 20) // 임의의 파괴 지점
        {
            Destroy(gameObject);
        }
    }
}