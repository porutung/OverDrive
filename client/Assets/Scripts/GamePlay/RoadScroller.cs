using System.Collections.Generic;
using UnityEngine;
public class RoadScroller : MonoBehaviour
{
    [Header("오브젝트 연결")]
    [SerializeField] private PlayerCarController playerCar;
    [SerializeField] private List<Transform> roadList; // 도로들의 Transform 리스트

    [Header("도로 설정")]
    [SerializeField] private float scrollLength = 50f; // 도로 하나의 길이

    private float _totalRoadLength; // 전체 도로들의 총 길이

    void Start()
    {
        if (roadList == null || roadList.Count == 0)
        {
            Debug.LogError("도로 리스트가 비어있습니다. Inspector에서 할당해주세요.");
            return;
        }
        
        // 전체 도로 길이를 미리 계산 (도로 길이 * 도로 개수)
        _totalRoadLength = scrollLength * roadList.Count;
    }

    void Update()
    {
        if (playerCar == null) return;

        // 플레이어의 현재 속도에 맞춰 모든 도로를 뒤로 이동
        float scrollSpeed = playerCar.currentSpeed;
        foreach (Transform road in roadList)
        {
            road.Translate(Vector3.back * scrollSpeed * Time.deltaTime, Space.World);
        }

        // 가장 앞에 있는 도로를 찾습니다.
        Transform firstRoad = roadList[0];
        foreach (Transform road in roadList)
        {
            if (road.position.z < firstRoad.position.z)
            {
                firstRoad = road;
            }
        }

        // 가장 앞에 있는 도로가 플레이어 뒤로 충분히 이동했는지 확인
        if (firstRoad.position.z < playerCar.transform.position.z - scrollLength)
        {
            Debug.Log($"Road '{firstRoad.name}'를 맨 뒤로 재배치합니다.");

            // 가장 뒤에 있는 도로를 찾습니다.
            Transform lastRoad = roadList[0];
            foreach (Transform road in roadList)
            {
                if (road.position.z > lastRoad.position.z)
                {
                    lastRoad = road;
                }
            }

            // 가장 앞에 있던 도로를, 가장 뒤에 있는 도로의 바로 뒤에 정확히 갖다 붙입니다.
            // += 연산이 아닌, 정확한 위치를 계산하여 오차를 없애는 방식입니다.
            firstRoad.position = new Vector3(
                lastRoad.position.x,
                lastRoad.position.y,
                lastRoad.position.z + scrollLength
            );
        }
    }
}
