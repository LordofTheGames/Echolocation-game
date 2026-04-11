using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
	public AudioClip gameStartSound;

	public void OnStartGameClicked()
	{
		AudioSource source = GetComponent<AudioSource>();
		source.PlayOneShot(gameStartSound, 1);
		StartCoroutine(FadeOutMusic(source, 3.6f));
		Invoke(nameof(LoadGameScene), 3.6f);
	}

	private void LoadGameScene()
	{
		SceneManager.LoadScene("BETA");
	}

	IEnumerator FadeOutMusic(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            
            audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            
            yield return null; 
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        
        audioSource.volume = startVolume; 
    }
}