using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternSpawner : MonoBehaviour
{
    [Header("패턴 설정")]
    [Tooltip("실행할 장애물 패턴 리스트")]
    [SerializeField] private List<ObstaclePattern> patterns;
    
    [Header("패턴 소환 간격")]
    [SerializeField] private float minPatternInterval = 3f;
    [SerializeField] private float maxPatternInterval = 5f;

    [Header("소환 위치")]
    [SerializeField] private float[] laneXPositions = { -5.0f, -2.5f, 0f, 2.5f, 5.0f };
    [SerializeField] private float spawnZPosition = 100f;
    [SerializeField] private float ConstCarSpeed = 20f; // 최소 차량 속도    
    [SerializeField] private PlayerCarController playerCar;
    
    // --- 연료 보급 설정 추가 ---
    [Header("연료 보급 설정")]
    [Tooltip("소환할 연료 아이템 프리팹")]
    [SerializeField] private List<ObstaclePattern> fuelItems;
    [Tooltip("연료 아이템이 소환되는 시간 간격(초)")]
    [SerializeField] private float fuelSpawnInterval = 70f;
    
    private float accumWaitTime = 0f;
    private PoolService _poolService;
    // -------------------------
    void Start()
    {
        _poolService = ServiceLocator.Get<PoolService>(); 
        StartCoroutine(SpawnPatternRoutine());
    }

    // 전체 패턴을 소환하는 것을 관리하는 코루틴
    private IEnumerator SpawnPatternRoutine()
    {
        while (true)
        {
            if (playerCar.CurrentState == PlayerCarController.CarState.OutOfFuel)
            {
                yield break;
            }                
            
            // 다음 패턴이 나올 때까지 랜덤 시간 동안 대기
            float waitTime = Random.Range(minPatternInterval, maxPatternInterval);
            
            yield return new WaitForSeconds(waitTime);
            
            accumWaitTime += waitTime;
            
            ObstaclePattern randomPattern = null;
            if (accumWaitTime >= fuelSpawnInterval)
            {                
                accumWaitTime = 0f;
                // 연료 아이템 소환
                randomPattern = fuelItems[Random.Range(0, fuelItems.Count)];                                
            }
            else
            {
                // 패턴 리스트에서 무작위로 하나를 선택(자동차)
                randomPattern = patterns[Random.Range(0, patterns.Count)];                                           
            }
                
            // 선택된 패턴을 실행
            yield return StartCoroutine(ExecutePattern(randomPattern));
        }
    }

    // 개별 패턴의 세부 내용을 실행하는 코루틴
    private IEnumerator ExecutePattern(ObstaclePattern pattern)
    {
        Debug.Log($"패턴 '{pattern.name}' 실행!");
        // 패턴에 정의된 모든 소환 이벤트를 순서대로 실행
        foreach (SpawnEvent spawnEvent in pattern.spawnEvents)
        {
            // 정의된 시간만큼 대기
            yield return new WaitForSeconds(spawnEvent.timeOffset);
            
            // 소환 위치 계산
            Vector3 spawnPosition = new Vector3(laneXPositions[spawnEvent.laneIndex], 0.0f, spawnZPosition);

            // 차량 생성 (오브젝트 풀링을 사용한다면 여기서 GetFromPool 호출)
            //GameObject spawnedCar = Instantiate(spawnEvent.carPrefab, spawnPosition, Quaternion.identity);
            GameObject spawnedCar = _poolService.Get(spawnEvent.carPrefab);
            spawnedCar.transform.position = spawnPosition;
            
            
            var otherCar = spawnedCar.GetComponent<OtherCar>();
            otherCar.playerCar = playerCar;
            otherCar.speed = ConstCarSpeed;
        }
    }
 
}