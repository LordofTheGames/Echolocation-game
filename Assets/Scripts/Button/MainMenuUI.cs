using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
	public void OnStartGameClicked()
	{
		Invoke(nameof(LoadGameScene), 3f);
	}

	private void LoadGameScene()
	{
		SceneManager.LoadScene("MVP");
	}
}