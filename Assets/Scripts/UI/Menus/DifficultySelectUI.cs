using System.Collections;
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
        StartCoroutine(LoadGameScene("EASY"));

    }

    public void OnNormalClicked()
    {
        StartCoroutine(LoadGameScene("NORMAL"));
    }

    public void OnHardClicked()
    {
        StartCoroutine(LoadGameScene("HARD"));
    }

    IEnumerator LoadGameScene(string scene)
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(scene);
    }
}