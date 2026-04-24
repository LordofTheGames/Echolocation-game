using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelectUI : MonoBehaviour
{
    private void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnEasyClicked()
    {
        Invoke(nameof(LoadGameScene), 0.5f);
    }

    public void OnNormalClicked()
    {
        Invoke(nameof(LoadGameScene), 0.5f);
    }

    public void OnHardClicked()
    {
        Invoke(nameof(LoadGameScene), 0.5f);
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene("FINAL");
    }
}