using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSceneView : View_Base<LoadingSceneViewModel>
{
    [Header("UI Components")]
    // 기존 Bind 시스템 활용
    [Bind("ProgressBar")] [SerializeField] private Slider _progressBar;
    [Bind("LoadingText")] [SerializeField] private TextMeshProUGUI _loadingText;

    void Start()
    {
        Initialize(new LoadingSceneViewModel());
    }
    protected override void BindViewModel()
    {
        // ViewModel 이벤트 구독
        ViewModel.OnProgressUpdate += UpdateProgressBar;
        ViewModel.OnTextUpdate += UpdateLoadingText;

        // 로딩 시작 요청
        ViewModel.StartLoading();
    }

    protected override void UnbindViewModel()
    {
        // 이벤트 구독 해제
        ViewModel.OnProgressUpdate -= UpdateProgressBar;
        ViewModel.OnTextUpdate -= UpdateLoadingText;
    }

    private void UpdateProgressBar(float value)
    {
        if (_progressBar != null)
        {
            _progressBar.value = value;
        }
    }

    private void UpdateLoadingText(string text)
    {
        if (_loadingText != null)
        {
            _loadingText.text = text;
        }
    }
}
