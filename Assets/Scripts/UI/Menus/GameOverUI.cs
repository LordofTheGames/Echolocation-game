using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    private void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnRestartClicked()
    {
        Invoke(nameof(LoadGameScene), 0.5f);
    }

    public void OnExitClicked()
    {
        Invoke(nameof(LoadMainMenu), 0.5f);
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene("MVP");
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}