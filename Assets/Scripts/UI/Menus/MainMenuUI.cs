using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
	public void OnStartGameClicked()
	{
		Invoke(nameof(LoadGameScene), 0.5f);
	}

	private void LoadGameScene()
	{
		SceneManager.LoadScene("MicCalibration");
	}
}