using System.Collections.Generic;
using UnityEngine;

// 개별 차량의 소환 정보를 담을 작은 클래스
[System.Serializable]
public class SpawnEvent
{
    [Tooltip("소환할 장애물 차량 프리팹")]
    public GameObject carPrefab;
    [Tooltip("소환될 차선 인덱스 (0:왼쪽, 1:중앙, 2:오른쪽)")]
    public int laneIndex;
    [Tooltip("이전 차량이 소환된 후 몇 초 뒤에 소환할지")]
    public float timeOffset;
}

// 스크립터블 오브젝트로 패턴 자체를 정의
[CreateAssetMenu(fileName = "NewObstaclePattern", menuName = "Racing/Obstacle Pattern")]
public class ObstaclePattern : ScriptableObject
{
    public List<SpawnEvent> spawnEvents;
}