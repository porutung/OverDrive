/*
 * Monobehaviour를 상속 받는 Singleton Generic
 */
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();
    private static bool applicationIsQuitting = false;
    private static bool _is_load = false;
    protected static bool _is_initialized = false;    
    public static T Instance
    {
        get
        {
            if (applicationIsQuitting) {
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T) FindFirstObjectByType(typeof(T));

                    if (FindObjectsByType(typeof(T), FindObjectsSortMode.None).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton of type " + typeof(T) + ".");
                        return null;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "[Singleton] " + typeof(T).ToString();
                        
#if UNITY_EDITOR
                        if (Application.isPlaying == false)
                        {
                            // 플레이 모드가 아닌 상태에서 Singleton을 호출해서 객체가 생성되는 경우 저장되지 않도록 함
                            singleton.hideFlags = HideFlags.DontSave;
                        }
#endif
                        if (_is_load == false)
                        {
                            DontDestroyOnLoad(singleton);  // 씬 전환 시에도 유지
                            _is_load = true;
                        }                        
                        Debug.Log("[Singleton] An instance of " + typeof(T) + " is needed in the scene, so '" + singleton + "' was created with DontDestroyOnLoad.");
                    } 
                    else {
                        Debug.Log("[Singleton] Using instance already created: " + _instance.gameObject.name);
                        if (_is_load == false)
                        {
                            _is_load = true;
                            DontDestroyOnLoad(_instance.gameObject);    
                        }                        
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton could be destroyed before any other
    /// object asks for it. So we need to quit the application.
    /// </summary>
    protected virtual void OnDestroy () {
        applicationIsQuitting = true;
        _instance = null;
        _is_load = false;
        _is_initialized = false;
    }
}