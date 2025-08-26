/*
 * 싱글톤 제너릭. 
 */
using System;

public sealed class Singleton<T> where T : class, new()
{
    private static Lazy<T> _instance = new Lazy<T>(() => new T());    // readonly 제거하여 재할당 가능하도록 수정

    public static T Instance { get { return _instance.Value; } }

    public static void DestroyInstance()
    {
        _instance = new Lazy<T>(() => new T());    // 새로운 인스턴스로 초기화
    }

    private Singleton()
    {
        
    }
}
