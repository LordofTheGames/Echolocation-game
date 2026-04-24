using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResetCrazyEffect : MonoBehaviour
{

    public AudioClip swallowSound;
    public float swallowVolume = 1;
    public float fadeInTime = 1.0f; 
    public float fadeOutTime = 1.0f; 
    public float waitTime = 1.0f;

    private PlayerMovement pm;
    private CharacterController cc;
    private CrazyTimer ct;
    private AudioSource audioSource; 
    private Image blackScreenImage; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player"); 
        pm = player.GetComponent<PlayerMovement>();
        cc = player.GetComponent<CharacterController>();
        ct = player.GetComponent<CrazyTimer>();

        blackScreenImage = GameObject.Find("PillBoxScreenImage").GetComponent<Image>();
        Color c = blackScreenImage.color;
        c.a = 0;
        blackScreenImage.color = c;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // Heard equally in both ears since it's coming from the player
    }

    public void ResetEffect()
    {
        StartCoroutine(resetEffectCoroutine());
    }

    private IEnumerator resetEffectCoroutine()
    {
        pm.enabled = false;
        cc.enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);

        audioSource.PlayOneShot(swallowSound, swallowVolume);
        if (ct != null) ct.FadeOutSounds();

        //fade to black
        float timer = 0f;
        Color c = blackScreenImage.color;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            c.a = timer / fadeInTime;
            blackScreenImage.color = c;
            yield return null; 
        }
        c.a = 1;
        blackScreenImage.color = c;

        Time.timeScale = 0;

        // wait at black screen
        float pauseEndTime = Time.realtimeSinceStartup + waitTime;
        while (Time.realtimeSinceStartup < pauseEndTime)
        {
            yield return 0;
        }

        Time.timeScale = 1;
        if (ct != null) ct.ResetEffect();
        pm.enabled = true;
        cc.enabled = true;

        //fade back in
        timer = 0f;
        c = blackScreenImage.color;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            c.a = 1f - (timer / fadeOutTime); 
            blackScreenImage.color = c;
            yield return null;
        }
        c.a = 0;
        blackScreenImage.color = c;
    }
}
