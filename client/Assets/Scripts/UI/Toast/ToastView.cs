using TMPro;
using UnityEngine;

public class ToastView : View_Base<ToastViewModel>
{
    [SerializeField] private TextMeshProUGUI _messageText;

    protected override void BindViewModel()
    {
        // ViewModel의 Message 데이터를 Text 컴포넌트에 바인딩
        _messageText.text = ViewModel.Message;
    }

    protected override void UnbindViewModel() { }
}