
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraService : MonoBehaviour
{
    [System.Serializable] // 인스펙터 디버그 모드에서 리스트 내용을 보고 싶다면 추가
    struct OverlayCameraData
    {
        public RegistOverlayCamera registScript;
        public Camera camera;

        public OverlayCameraData(RegistOverlayCamera script, Camera cam)
        {
            this.registScript = script;
            this.camera = cam;
        }
    }

    private List<OverlayCameraData> _overlayCameras = new List<OverlayCameraData>();
    
    
    //[등록] 오버레이 카메라가 생성될 때 호출
    public void RegisterOverlayCamera(RegistOverlayCamera script, Camera cam)
    {
        if (!_overlayCameras.Any(x => x.registScript == script))
        {
            _overlayCameras.Add(new OverlayCameraData{ registScript = script, camera = cam});
        }
    }
    
    //[해제] 오버레이 카메라가 사라질 때 호출.
    public void UnregisterOverlayCamera(RegistOverlayCamera script)
    {
        _overlayCameras.RemoveAll(x => x.registScript == script);
    }
    
    //[적용] 메인 카메라가 요청하면 스택을 쌓아줌(baseCamrea가 메인 카메라)
    public void SetupCameraStack(Camera baseCamera)
    {
        var cameraData = baseCamera.GetUniversalAdditionalCameraData();
        if (cameraData == null)
        {
            return;
        }
        
        // 1. 리스트를 StackDepth(숫자) 오름차순으로 정렬
        // (낮은 숫자 = 먼저 그려짐 = 밑에 깔림 / 높은 숫자 = 나중에 그려짐 = 위에 뜸)
        var sortedList = _overlayCameras.OrderBy(x => x.registScript.GetStackDepth()).ToList();

        // 2. 순서대로 스택에 추가
        foreach (var info in sortedList)
        {
            // 중복 체크 후 추가
            if (info.camera != null && !cameraData.cameraStack.Contains(info.camera))
            {
                cameraData.cameraStack.Add(info.camera);
            }
        }
        
        
        
    }
}
