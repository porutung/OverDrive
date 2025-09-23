using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ToastController : UI_ControllerBase
{
    [SerializeField] private float _toastDuration = 2.0f; // 토스트 지속 시간

    private Queue<string> _toastQueue = new Queue<string>();
    private bool _isShowingToast = false;

    // Show<T>는 사용하지 않으므로 빈 상태로 재정의
    public override UniTask<T> Show<T>()
    {
        return UniTask.FromResult<T>(null);
    }
    // Show<T>(T viewModel)는 사용하지 않으므로 빈 상태로 재정의
    public override UniTask<T> Show<T>(T viewModel)
    {
        return UniTask.FromResult<T>(null);
    }

    /// <summary>
    /// 토스트 메시지를 표시하도록 요청합니다.
    /// </summary>
    public void Show(string message)
    {
        _toastQueue.Enqueue(message);
        if (!_isShowingToast)
        {
            ProcessToastQueue().Forget();
        }
    }

    private async UniTaskVoid ProcessToastQueue()
    {
        _isShowingToast = true;

        var canvas = _canvasManager.GetCanvas(CanvasManager.ECanvasType.Overlay);

        while (_toastQueue.Count > 0)
        { 
            string message = _toastQueue.Dequeue();
            
            // CreateView가 비동기이므로 await으로 대기
            var view = await CreateView<ToastViewModel>(canvas.transform);
            
            if (view != null)
            {
                view.ViewModel.Message = message;
                view.transform.SetAsLastSibling();

                // yield return new WaitForSeconds 대신 UniTask.Delay 사용
                await UniTask.Delay(TimeSpan.FromSeconds(_toastDuration));

                // Destroy 대신 AssetLoader를 통해 인스턴스 해제
                _assetLoader.ReleaseInstance(view.gameObject);
            }
        }

        _isShowingToast = false;
    }
}