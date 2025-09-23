using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PopupController : UI_ControllerBase
{
    private List<GameObject> _popupStack = new List<GameObject>();

    // RegisterUIPrefabs 메서드는 이제 완전히 필요 없어졌습니다!

    public override async UniTask<T> Show<T>() 
    {
        var canvas = _canvasManager.GetCanvas(CanvasManager.ECanvasType.Overlay);
        var view = await CreateView<T>(canvas.transform);

        if (view == null) 
            return null; // 생성 실패 시

        _popupStack.Add(view.gameObject);
        view.transform.SetAsLastSibling();

        view.ViewModel.OnRequestClose += () => {
            _popupStack.Remove(view.gameObject);
            _assetLoader.ReleaseInstance(view.gameObject);
        };

        return view.ViewModel as T;
    }

    public override async UniTask<T> Show<T>(T viewModel)
    {
        var canvas = _canvasManager.GetCanvas(CanvasManager.ECanvasType.Overlay);
        var view = await CreateView<T>(viewModel, canvas.transform);

        if (view == null) 
            return null; // 생성 실패 시

        _popupStack.Add(view.gameObject);
        view.transform.SetAsLastSibling();

        view.ViewModel.OnRequestClose += () => {
            _popupStack.Remove(view.gameObject);
            _assetLoader.ReleaseInstance(view.gameObject);
        };

        return view.ViewModel as T;
    }
}