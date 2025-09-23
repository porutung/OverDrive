using UnityEngine;
using UnityEngine.SceneManagement;
[UIPrefab("Prefab/UI/GameOver")]
public class GameOverViewModel : ViewModel_Base
{
    public override void RequestClose()
    {
        base.RequestClose();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
