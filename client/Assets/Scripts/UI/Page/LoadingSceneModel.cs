using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneModel
{
    public float Progress { get; private set; }
    public string LoadingText { get; private set; }

    public event Action<float> OnProgressChanged;
    public event Action<string> OnTextChanged;
    public event Action OnLoadingComplete;

    // 비동기 로딩 시작
    public async UniTask LoadNextSceneAsync()
    {
        string targetScene = SceneLoadService.NextSceneName;

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("이동할 씬 이름이 설정되지 않았습니다.");
            return;
        }

        // 1. 비동기 로딩 시작 (바로 넘어가지 않게 설정)
        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        float timer = 0.0f;
        float fakeDuration = 2.0f; // 최소 로딩 시간 (초) - 연출용

        // 2. 로딩 진행 루프
        while (!op.isDone)
        {
            timer += Time.deltaTime;

            // 실제 로딩 진행률 (0.9에서 멈춤)
            float realProgress = op.progress;
            
            // 연출용 가짜 진행률 (시간에 비례)
            float fakeProgress = Mathf.Clamp01(timer / fakeDuration);

            // 둘 중 더 낮은 값을 사용하여 진행바가 너무 빨리 차지 않게 함
            // 단, 로딩이 다 끝났다면(0.9) 가짜 진행률을 따라감
            float finalProgress = (realProgress < 0.9f) ? realProgress : fakeProgress;

            // 값 업데이트 및 알림
            Progress = finalProgress;
            OnProgressChanged?.Invoke(Progress);
            
            LoadingText = $"Loading... {(int)(Progress * 100)}%";
            OnTextChanged?.Invoke(LoadingText);

            // 3. 로딩 완료 조건: 실제 로딩도 90% 넘고, 연출 시간도 지났을 때
            if (op.progress >= 0.9f && timer >= fakeDuration)
            {
                OnTextChanged?.Invoke("Touch to Start"); // 혹은 자동 시작
                OnLoadingComplete?.Invoke();
                
                // 여기서 바로 넘길지, 사용자 입력을 기다릴지 결정
                // 지금은 0.5초 뒤 자동 전환
                await UniTask.Delay(500);
                
                op.allowSceneActivation = true;
                break;
            }

            await UniTask.Yield(); // 다음 프레임까지 대기
        }
    }
}
