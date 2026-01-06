using System;
using UnityEngine;

/// <summary>
/// 모든 View의 기반이 되는 최상위 클래스입니다.
/// 제네릭을 사용하여 자신과 연결될 ViewModel의 타입을 지정합니다.
/// </summary>
/// <typeparam name="T">이 View와 바인딩될 ViewModel의 타입</typeparam>
public abstract class View_Base<T> : MonoBehaviour where T : ViewModel_Base
{
    // View가 참조할 ViewModel 인스턴스
    public T ViewModel { get; protected set; }

    /// <summary>
    /// UIManager가 ViewModel을 주입(Inject)하고 View를 초기화하기 위해 호출합니다.
    /// </summary>
    /// <param name="viewModel">이 View와 연결될 ViewModel</param>
    public void Initialize(T viewModel)
    {
        Bind.DoUpdate(this);
        
        ViewModel = viewModel;
        
        BindViewModel();
    }

    /// <summary>
    /// ViewModel의 데이터(프로퍼티, 이벤트)를 View의 컴포넌트(Text, Button 등)에 연결(바인딩)합니다.
    /// 이 메서드는 자식 클래스에서 반드시 구현해야 합니다.
    /// </summary>
    protected abstract void BindViewModel();

    /// <summary>
    /// ViewModel과의 연결(바인딩)을 해제합니다.
    /// 주로 메모리 누수 방지를 위해 이벤트 구독을 취소하는 코드가 들어갑니다.
    /// </summary>
    protected abstract void UnbindViewModel();

    /// <summary>
    /// 이 View(GameObject)가 파괴될 때 호출됩니다.
    /// </summary>
    protected virtual void OnDestroy()
    {
        // 메모리 누수를 방지하기 위해 ViewModel과의 연결을 반드시 해제합니다.
        UnbindViewModel();
    }

    private void Reset()
    {
        Bind.DoUpdate(this);
    }
}