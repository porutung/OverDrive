using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private PageController _pageController;
    private PopupController _popupController;
    private ToastController _toastController;

    private void Start()
    {
        // 같은 게임 오브젝트에 있는 Controller 컴포넌트들을 가져옴
        _pageController = GetComponent<PageController>();
        _popupController = GetComponent<PopupController>();
        _toastController = GetComponent<ToastController>();
    }

    // --- Public API ---

    public async UniTask<T> ShowPage<T>() where T : ViewModel_Base, new()
    {
        return await _pageController.Show<T>();
    }

    public async UniTask<T> ShowPage<T>(T viewModel) where T : ViewModel_Base
    {
        return await _pageController.Show(viewModel);
    }

    public async UniTask<T> ShowPopup<T>() where T : ViewModel_Base, new()
    {
        return await _popupController.Show<T>();
    }

    public void ShowToast(string message)
    {
        _toastController.Show(message);
    }
}