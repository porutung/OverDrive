using System;
using UnityEngine;

public class CameraConnector : MonoBehaviour
{
    private Camera _camera;
    private CameraService _cameraService;
    private CanvasService _canvasService;
    private void Awake()
    {
        TryGetComponent(out _camera);
        if (_camera == null)
        {
            Debug.LogError($"Not Find Base Camera {gameObject.name}");
            return;
        }        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cameraService = ServiceLocator.Get<CameraService>();
        _canvasService = ServiceLocator.Get<CanvasService>();
        
        if (_camera != null)
        {
            // baseCamera에 Stack Overlay Camera 등록.
            _cameraService.SetupCameraStack(_camera);
            
            // WorldUI 카메라 등록.
            _canvasService.SetWorldCarmera(_camera);
        }
        else
        {
            Debug.LogError($"Not Find Base Camera {gameObject.name}");
        }
    }
}
