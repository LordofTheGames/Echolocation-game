using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public void OnRestartClicked()
    {
        SceneManager.LoadScene("MVP");
    }
}