using System;
using Cysharp.Threading.Tasks; // UniTask 사용
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class AssetLoader : MonoBehaviour
{
   // 로드된 에셋 핸들 캐싱
    private readonly Dictionary<string, AsyncOperationHandle> _loadAssetHandles = new Dictionary<string, AsyncOperationHandle>();
    // 인스턴스화된 핸들 캐싱
    private readonly Dictionary<GameObject, AsyncOperationHandle> _instanceHandles = new Dictionary<GameObject, AsyncOperationHandle>();

    #region 비동기 (Asynchronous) API

    /// <summary>
    /// 에셋을 메모리에 미리 로드합니다. (인스턴스화 X)
    /// </summary>
    public async UniTask<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object
    {
        if (_loadAssetHandles.TryGetValue(key, out var handle))
        {
            return await handle.Convert<T>().ToUniTask();
        }

        var newHandle = Addressables.LoadAssetAsync<T>(key);
        _loadAssetHandles.Add(key, newHandle);
        return await newHandle.ToUniTask();
    }

    /// <summary>
    /// 키(주소)를 사용하여 에셋을 비동기로 인스턴스화합니다.
    /// </summary>
    public async UniTask<GameObject> InstantiateAsync(string key, Transform parent = null)
    {
        var handle = Addressables.InstantiateAsync(key, parent);
        GameObject instance = await handle.ToUniTask();
        
        _instanceHandles.Add(instance, handle);
        return instance;
    }
    
    /// <summary>
    /// 콜백 방식으로 에셋을 로드합니다.
    /// </summary>
    public void LoadAssetAsync<T>(string key, Action<T> onCompleted) where T : UnityEngine.Object
    {
        LoadAssetAsync<T>(key).ContinueWith(asset => onCompleted?.Invoke(asset)).Forget();
    }

    #endregion

    #region 동기 (Synchronous) API

    /// <summary>
    /// 에셋을 동기 방식으로 로드합니다. (UI 스레드 차단)
    /// </summary>
    /// <remarks>성능 저하를 유발할 수 있으므로 게임 플레이 중 사용에 주의하세요.</remarks>
    public T LoadAssetSync<T>(string key) where T : UnityEngine.Object
    {
        if (_loadAssetHandles.TryGetValue(key, out var handle))
        {
            return handle.WaitForCompletion() as T;
        }

        var newHandle = Addressables.LoadAssetAsync<T>(key);
        _loadAssetHandles.Add(key, newHandle);
        return newHandle.WaitForCompletion();
    }

    #endregion

    #region 메모리 해제 (Release) API

    /// <summary>
    /// 로드했던 에셋을 메모리에서 해제합니다.
    /// </summary>
    public void ReleaseAsset(string key)
    {
        if (_loadAssetHandles.TryGetValue(key, out var handle))
        {
            _loadAssetHandles.Remove(key);
            Addressables.Release(handle);
        }
    }

    /// <summary>
    /// 인스턴스화된 게임 오브젝트를 해제합니다.
    /// </summary>
    public void ReleaseInstance(GameObject instance)
    {
        if (instance != null && _instanceHandles.TryGetValue(instance, out var handle))
        {
            _instanceHandles.Remove(instance);
            Addressables.ReleaseInstance(handle);
        }
    }

    #endregion
}
