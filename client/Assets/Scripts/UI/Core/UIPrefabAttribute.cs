using System;

/// <summary>
/// ViewModel 클래스에 연결될 UI 프리팹의 리소스 경로를 지정하는 Attribute입니다.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class UIPrefabAttribute : Attribute
{
    public string Path { get; }

    public UIPrefabAttribute(string path)
    {
        Path = path;
    }
}