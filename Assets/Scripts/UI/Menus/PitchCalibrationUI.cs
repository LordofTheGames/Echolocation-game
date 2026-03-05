using UnityEngine;
using UnityEngine.SceneManagement;

public class PitchCalibrationUI : MonoBehaviour
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