using UnityEngine;
using UnityEngine.UI;

public class GameOverView : View_Base<GameOverViewModel>
{
    [SerializeField][Bind("RetryButton")] Button _retryButton;
    /// <summary>
    /// ViewModel의 데이터(프로퍼티, 이벤트)를 View의 컴포넌트(Text, Button 등)에 연결(바인딩)합니다.
    /// 이 메서드는 자식 클래스에서 반드시 구현해야 합니다.
    /// </summary>
    protected override void BindViewModel()
    {
        _retryButton.onClick.AddListener(() =>
        {
            ViewModel.RequestClose();
        });
    }

    /// <summary>
    /// ViewModel과의 연결(바인딩)을 해제합니다.
    /// 주로 메모리 누수 방지를 위해 이벤트 구독을 취소하는 코드가 들어갑니다.
    /// </summary>
    protected override void UnbindViewModel()
    {
        _retryButton.onClick.RemoveAllListeners();
    }
    
}
