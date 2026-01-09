using Cysharp.Threading.Tasks;
using UnityEngine;

public class PageController : UI_ControllerBase
{
    private GameObject _currentPage;

    public override async UniTask<T> Show<T>() 
    {
        // 1. 기존에 열려있는 페이지가 있다면 닫는다.
        if (_currentPage != null)
        {
            Destroy(_currentPage);
            _currentPage = null;
        }
        
        var canvas = _canvasManager.GetCanvas(CanvasService.ECanvasType.Overlay);
        var view = await CreateView<T>(canvas.transform);

        if (view == null) 
            return null;

        // 2. 새로 생성된 페이지를 현재 페이지로 저장
        _currentPage = view.gameObject;

        view.ViewModel.OnRequestClose += () => {
            if (_currentPage == view.gameObject)
            {
                _currentPage = null;
            }
            _assetLoader.ReleaseInstance(view.gameObject);
        };

        return view.ViewModel as T;
    }

    public override async UniTask<T> Show<T>(T viewModel)
    {
        // 1. 기존에 열려있는 페이지가 있다면 닫는다.
        if (_currentPage != null)
        {
            Destroy(_currentPage);
            _currentPage = null;
        }
        
        var canvas = _canvasManager.GetCanvas(CanvasService.ECanvasType.Overlay);
        var view = await CreateView(viewModel, canvas.transform);

        if (view == null) 
            return null;

        // 2. 새로 생성된 페이지를 현재 페이지로 저장
        _currentPage = view.gameObject;

        view.ViewModel.OnRequestClose += () => {
            if (_currentPage == view.gameObject)
            {
                _currentPage = null;
            }
            _assetLoader.ReleaseInstance(view.gameObject);
        };

        return view.ViewModel as T;
    }
    
}