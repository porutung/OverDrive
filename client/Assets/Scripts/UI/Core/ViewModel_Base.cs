using System;
using UnityEngine;

public abstract class ViewModel_Base : IDisposable
{
    // 나중에 모든 ViewModel에 필요한 공통 기능이 있다면 여기에 추가합니다.
    // 예를 들어, public virtual void OnShow() {}
    // public virtual void OnHide() {}
    public event Action OnRequestClose;
    
    public virtual void RequestClose()
    {
        OnRequestClose?.Invoke();
    }
    
    /// <summary>
    /// 이 ViewModel이 더 이상 필요 없을 때 호출될 정리 메서드
    /// </summary>
    public void Dispose()
    {
        OnDispose();
    }
    
    /// <summary>
    /// 자식 클래스에서 실제 정리 로직을 구현할 가상 메서드
    /// </summary>
    protected virtual void OnDispose()
    {
        // 자식 클래스에서 재정의하여 사용
    }
}