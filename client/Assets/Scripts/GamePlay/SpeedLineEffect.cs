using System.Collections;
using UnityEngine;

public class SpeedLineEffect : MonoBehaviour
{
    [Header("오브젝트 연결")]
    [SerializeField] private PlayerCarController playerCarController;
    [SerializeField] private GameObject speedLinePrefab;

    [Header("효과 설정")]
    [SerializeField] private float spawnRate = 0.05f; // 더 많은 선을 위해 생성 간격 줄이기
    [SerializeField] private float lineLifetime = 0.4f;
    [SerializeField] private float lineLength = 5f;
    [Tooltip("차가 중심일 때, 선이 생성될 반경")]
    [SerializeField] private float spawnRadius = 3f;

    private Coroutine _spawnCoroutine;

    void Update()
    {
        // PlayerCarController에서 부스트 상태를 가져오는 public 메서드가 필요합니다.
        // 예: public bool IsBoosting() { return _isBoosting; }
        bool isBoosting = playerCarController.IsBoosting() || playerCarController.IsNitroBoosting();
        if ( isBoosting && _spawnCoroutine == null)
        {
            _spawnCoroutine = StartCoroutine(SpawnLines());
        }
        else if (!isBoosting && _spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnLines()
    {
        while (true)
        {
            // 1. 속도선 프리팹을 월드 공간에 생성
            GameObject lineObj = Instantiate(speedLinePrefab);
            LineRenderer line = lineObj.GetComponent<LineRenderer>();

            // 2. 차 주변의 랜덤한 원통형 위치 결정
            Vector2 randomCirclePoint = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCirclePoint.x, randomCirclePoint.y, 0);

            // 3. 라인 렌더러의 시작점과 끝점 설정 (차의 앞쪽에서 뒤쪽으로)
            line.SetPosition(0, spawnPos + transform.forward * (lineLength / 2));
            line.SetPosition(1, spawnPos - transform.forward * (lineLength / 2));

            // 4. 이 선을 애니메이션하고 파괴하는 별도의 코루틴 시작
            StartCoroutine(AnimateLine(line.transform));
            
            yield return new WaitForSeconds(spawnRate);
        }
    }

    private IEnumerator AnimateLine(Transform lineTransform)
    {
        float timer = 0f;
        LineRenderer line = lineTransform.GetComponent<LineRenderer>();
        Color startColor = line.startColor;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        while (timer < lineLifetime)
        {
            // 1. 선이 배경과 함께 뒤로 스크롤되도록 위치 이동
            lineTransform.Translate(Vector3.back * playerCarController.currentSpeed * Time.deltaTime, Space.World);

            // 2. 시간이 지남에 따라 선을 투명하게 만들어 사라지게 함
            float fade = 1 - (timer / lineLifetime);
            line.startColor = new Color(startColor.r, startColor.g, startColor.b, startColor.a * fade);
            line.endColor = new Color(startColor.r, startColor.g, startColor.b, startColor.a * fade);

            timer += Time.deltaTime;
            yield return null;
        }
        
        // 애니메이션이 끝나면 오브젝트 파괴
        Destroy(lineTransform.gameObject);
    }
}