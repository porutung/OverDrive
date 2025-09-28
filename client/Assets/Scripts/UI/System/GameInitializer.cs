using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameInitializer : MonoBehaviour
{
    [SerializeField][Bind("CanvasManager")] CanvasManager _canvasManager;
    [SerializeField][Bind("UIManager")] UIManager _uiManager;
    [SerializeField][Bind("AssetLoader")]  AssetLoader _assetLoader;

    private void Reset()
    {
        Bind.DoUpdate(this);    
    }

    private void Awake()
    {                
        ServiceLocator.Register(_canvasManager);        
        ServiceLocator.Register(_uiManager);        
        ServiceLocator.Register(_assetLoader);
    }

    async UniTask Start()
    {
        SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
    }
}