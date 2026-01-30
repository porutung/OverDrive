using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadService : MonoBehaviour
{
    // 다음으로 이동할 씬 이름을 저장할 정적 변수
    public static string NextSceneName { get; private set; }
    private const string LOADING_SCENE_NAME = "LoadingScene";
    public void LoadScene(string sceneName)
    {
        NextSceneName = sceneName;
        SceneManager.LoadScene(LOADING_SCENE_NAME);
    }
}
