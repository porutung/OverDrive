using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
public class GameInitializer : MonoBehaviour
{
    [SerializeField][Bind("CanvasService")] CanvasService _canvasManager;
    [SerializeField][Bind("UiService")] UIService _uiManager;
    [SerializeField][Bind("AssetLoader")]  AssetLoader _assetLoader;
    [SerializeField][Bind("SceneLoadService")] SceneLoadService _sceneLoadService;
    [SerializeField][Bind("CameraService")] CameraService _cameraService;
    private void Reset()
    {
        Bind.DoUpdate(this);    
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        // 1. 이미 씬에 GameInitializer가 있는지 확인 (InitScene에서 시작했을 경우 중복 방지)
        var existing = FindFirstObjectByType<GameInitializer>();
        if (existing != null) 
            return;

        // 2. Resources 폴더에서 프리팹 로드
        var prefab = Resources.Load<GameInitializer>("ServiceLocator");
        if (prefab == null)
        {
            Debug.LogError("CRITICAL: Resources 폴더에 'GameInitializer' 프리팹이 없습니다!");
            return;
        }

        // 3. 프리팹 생성 및 파괴 방지 설정
        var instance = Instantiate(prefab);
        instance.name = "[System] GameInitializer"; // 이름 깔끔하게
        DontDestroyOnLoad(instance.gameObject);
        
        // 4. (선택사항) 여기서 강제로 Awake 로직이 돌게 되지만, 
        // 확실하게 하기 위해 초기화 함수가 있다면 호출해 줄 수도 있습니다.
    }
    private void Awake()
    {                
        ServiceLocator.Register(_canvasManager);        
        ServiceLocator.Register(_uiManager);        
        ServiceLocator.Register(_assetLoader);
        ServiceLocator.Register(_sceneLoadService);
        ServiceLocator.Register(_cameraService);        
    }

    void Start()
    {
        _sceneLoadService.LoadScene("MainScene");
    }
}