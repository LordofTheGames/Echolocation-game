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
        SceneManager.LoadScene("MVP");
    }

    public void OnExitClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}