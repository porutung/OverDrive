using UnityEngine;

public class RoadScroller : MonoBehaviour
{
    [SerializeField] private PlayerCarController playerCar; // 플레이어 참조
    
    [Tooltip("이 도로 프리팹 하나의 길이")]
    [SerializeField] private float scrollLength = 50f;
    
    [Tooltip("장면에 배치된 총 도로의 개수")]
    [SerializeField] private int numberOfRoads = 2;

    private float _teleportDistance;

    void Start()
    {
        // 순간이동할 거리를 미리 계산해 둡니다. (도로 길이 * 도로 개수)
        _teleportDistance = scrollLength * numberOfRoads;
    }

    void Update()
    {
        if (playerCar == null) return;

        // 플레이어의 현재 속도에 맞춰 뒤로 이동
        float scrollSpeed = playerCar.currentSpeed;
        transform.Translate(Vector3.back * scrollSpeed * Time.deltaTime, Space.World);

        // 도로가 플레이어보다 일정 거리 이상 뒤로 가면
        if (transform.position.z < playerCar.transform.position.z - scrollLength)
        {
            // 전체 도로 길이만큼 앞으로 순간이동!
            transform.position += new Vector3(0, 0, _teleportDistance);
        }
    }
}