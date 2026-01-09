using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks; // 리플렉션을 위해 추가
using UnityEngine;

public abstract class UI_ControllerBase : MonoBehaviour
{
    // 경로를 캐싱하기 위한 딕셔너리는 그대로 사용
    private readonly Dictionary<Type, string> _uiPrefabPathsCache = new Dictionary<Type, string>();

    protected CanvasService _canvasManager;
    protected AssetLoader _assetLoader;

    protected virtual void Start()
    {
        _canvasManager = ServiceLocator.Get<CanvasService>();
        _assetLoader = ServiceLocator.Get<AssetLoader>();
    }

    public abstract UniTask<T> Show<T>() where T : ViewModel_Base, new();
    public abstract UniTask<T> Show<T>(T model) where T : ViewModel_Base;
    
    // 경로를 가져오는 책임을 별도의 메서드로 분리
    private string GetPrefabPath<T>() where T : ViewModel_Base
    {
        var type = typeof(T);

        // 1. 캐시에 경로가 이미 있는지 확인
        if (_uiPrefabPathsCache.TryGetValue(type, out var path))
        {
            return path;
        }

        // 2. 캐시에 없다면, 리플렉션을 이용해 Attribute에서 경로를 찾아옴
        var attribute = type.GetCustomAttribute<UIPrefabAttribute>();
        if (attribute != null)
        {
            // 3. 찾은 경로를 캐시에 저장하고 반환
            _uiPrefabPathsCache[type] = attribute.Path;
            return attribute.Path;
        }
        else
        {
            Debug.LogError($"[UI_ControllerBase] UIPrefabAttribute not found on {type.Name}.");
            return null;
        }
    }

    protected async UniTask<View_Base<T>> CreateView<T>(Transform parent) where T : ViewModel_Base, new()
    {
        // GetPrefabPath 메서드를 통해 경로를 가져옴
        string path = string.Format($"Assets/Bundles/{GetPrefabPath<T>()}.prefab");
        if (string.IsNullOrEmpty(path)) 
            return null;

        var viewInstance = await _assetLoader.InstantiateAsync(path, parent);
            
        var view = viewInstance.GetComponent<View_Base<T>>();
        if (view == null)
        {
            Debug.LogError($"[UI_ControllerBase] View instance not found on {viewInstance.GetType().Name}., Components {typeof(T).Name}");
        }
        
        var viewModel = new T();
        view.Initialize(viewModel);
        
        return view;
    }

    protected async UniTask<View_Base<T>> CreateView<T>(T viewModel, Transform parent) where T : ViewModel_Base
    {
        // GetPrefabPath 메서드를 통해 경로를 가져옴
        string path = string.Format($"Assets/Bundles/{GetPrefabPath<T>()}.prefab");
        if (string.IsNullOrEmpty(path)) 
            return null;

        var viewInstance = await _assetLoader.InstantiateAsync(path, parent);
        var view = viewInstance.GetComponent<View_Base<T>>();
        view.Initialize(viewModel); // new T() 대신 전달받은 viewModel 사용
        
        return view;
    }
}