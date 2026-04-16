using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingTrigger : MonoBehaviour
{
    public float waitBeforeFadeTime = 1;
    public float fadeTime = 5;
    public Image whiteScreenImage;
    public TMP_Text text1;
    public TMP_Text text2;
    private bool fading = false;

    void Start()
    {
        Color c = whiteScreenImage.color;
        c.a = 0;
        whiteScreenImage.color = c;
        Color ct1 = text1.color;
        ct1.a = 0;
        text1.color = ct1;
        Color ct2 = text2.color;
        ct2.a = 0;
        text2.color = ct2;
    }

    void OnTriggerEnter(Collider other)
    {
        if (fading == false && other.CompareTag("Player"))
        {
            fading = true;
            StartCoroutine(fade());
        }
    }

    private IEnumerator fade()
    {
        yield return new WaitForSeconds(waitBeforeFadeTime);
        // float timer = 0f;
        Color c = whiteScreenImage.color;
        Color ct1 = text1.color;
        Color ct2 = text2.color;

        Time.timeScale = 0;

        float timeInitial = Time.realtimeSinceStartup;
        float pauseEndTime = Time.realtimeSinceStartup + fadeTime;
        while (Time.realtimeSinceStartup < pauseEndTime)
        {
            float time = Time.realtimeSinceStartup - timeInitial;
            c.a = time / fadeTime;
            ct1.a = time / fadeTime;
            ct2.a = time / fadeTime;
            whiteScreenImage.color = c;
            text1.color = ct1;
            text2.color = ct2;
            yield return 0;
        }
        c.a = 1;
        ct1.a = 1;
        ct2.a = 1;
        whiteScreenImage.color = c;
        text1.color = ct1;
        text2.color = ct2;
        SceneManager.LoadScene("GameVictory");
    }
}
