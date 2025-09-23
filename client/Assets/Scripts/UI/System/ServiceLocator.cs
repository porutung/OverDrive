using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 서비스(매니저)들을 등록하고 어디서든 접근할 수 있게 해주는 전역 클래스입니다.
/// </summary>
public static class ServiceLocator
{
    // 서비스들을 타입별로 저장하는 딕셔너리
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    /// <summary>
    /// 서비스를 등록합니다.
    /// </summary>
    /// <typeparam name="T">등록할 서비스의 타입</typeparam>
    /// <param name="service">등록할 서비스 인스턴스</param>
    public static void Register<T>(T service)
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} is already registered. Overwriting.");
            _services[type] = service;
        }
        else
        {
            _services.Add(type, service);
            Debug.Log($"[ServiceLocator] Service of type {type.Name} registered.");
        }
    }

    /// <summary>
    /// 등록된 서비스를 가져옵니다.
    /// </summary>
    /// <typeparam name="T">가져올 서비스의 타입</typeparam>
    /// <returns>등록된 서비스 인스턴스</returns>
    public static T Get<T>()
    {
        var type = typeof(T);
        if (!_services.TryGetValue(type, out var service))
        {
            Debug.LogError($"[ServiceLocator] Service of type {type.Name} not found.");
            return default; // 또는 throw new Exception($"Service of type {type.Name} not found.");
        }
        return (T)service;
    }

    /// <summary>
    /// 모든 서비스를 초기화(제거)합니다.
    /// </summary>
    public static void Clear()
    {
        _services.Clear();
        Debug.Log("[ServiceLocator] All services cleared.");
    }
}