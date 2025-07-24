using System.Collections;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] otherCarPrefabs; // 장애물 차량 프리팹 배열
    [SerializeField] private float[] laneXPositions = { -2.5f, 0f, 2.5f }; // 차선 위치
    [SerializeField] private float spawnZPosition = 100f; // 스폰될 z축 위치
    [SerializeField] private float minSpawnInterval = 1f; // 최소 스폰 간격
    [SerializeField] private float maxSpawnInterval = 3f; // 최대 스폰 간격
    [SerializeField] private float minCarSpeed = 10f; // 최소 차량 속도
    [SerializeField] private float maxCarSpeed = 18f; // 최대 차량 속도

    void Start()
    {
        StartCoroutine(SpawnCarsRoutine());
    }

    private IEnumerator SpawnCarsRoutine()
    {
        while (true)
        {
            // 랜덤한 시간 간격으로 대기
            float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(spawnInterval);

            // 스폰
            SpawnCar();
        }
    }

    private void SpawnCar()
    {
        // 랜덤한 차량 프리팹과 차선을 선택
        GameObject prefabToSpawn = otherCarPrefabs[Random.Range(0, otherCarPrefabs.Length)];
        float spawnXPosition = laneXPositions[Random.Range(0, laneXPositions.Length)];
        
        Vector3 spawnPosition = new Vector3(spawnXPosition, 0.5f, spawnZPosition); // y는 차량 높이에 맞게 조절

        // 차량 생성
        GameObject spawnedCar = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        // 생성된 차량의 속도를 랜덤하게 설정
        float carSpeed = Random.Range(minCarSpeed, maxCarSpeed);
        spawnedCar.GetComponent<OtherCar>().speed = carSpeed;
    }
}