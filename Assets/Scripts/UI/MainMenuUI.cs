using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
	public void OnStartGameClicked()
	{
		SceneManager.LoadScene("MVP");
	}
}