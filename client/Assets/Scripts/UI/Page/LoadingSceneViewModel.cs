using Cysharp.Threading.Tasks;
using UnityEngine;

[UIPrefab("Prefabs/UI/LoadingSceneView")]
public class LoadingSceneViewModel : ViewModel_Base
{
    private LoadingSceneModel _model;

    public float CurrentProgress => _model.Progress;
    public string CurrentText => _model.LoadingText;

    // View가 구독할 이벤트
    public event System.Action<float> OnProgressUpdate;
    public event System.Action<string> OnTextUpdate;

    public LoadingSceneViewModel()
    {
        _model = new LoadingSceneModel();
        
        // Model의 이벤트를 ViewModel 이벤트로 연결
        _model.OnProgressChanged += (progress) => OnProgressUpdate?.Invoke(progress);
        _model.OnTextChanged += (text) => OnTextUpdate?.Invoke(text);
    }

    protected override void OnDispose()
    {
        // 이벤트 해제
        _model.OnProgressChanged -= (progress) => OnProgressUpdate?.Invoke(progress);
        _model.OnTextChanged -= (text) => OnTextUpdate?.Invoke(text);
    }

    // View에서 Start 시점에 호출
    public void StartLoading()
    {
        // UniTask 실행 (Forget으로 비동기 실행을 기다리지 않고 넘어감)
        _model.LoadNextSceneAsync().Forget();
    }
}
