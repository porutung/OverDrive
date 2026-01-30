using UnityEngine;

public class RegistOverlayCamera : MonoBehaviour
{
    [SerializeField]private int _stackDepth = 0;
    public int GetStackDepth() => _stackDepth;
    
    void Start()
    {
        TryGetComponent(out Camera cam);
        if (cam != null)
        {
            ServiceLocator.Get<CameraService>().RegisterOverlayCamera(this, cam);
        }
    }

    void OnDestroy()
    {
        ServiceLocator.Get<CameraService>().UnregisterOverlayCamera(this);
    }

    
}
