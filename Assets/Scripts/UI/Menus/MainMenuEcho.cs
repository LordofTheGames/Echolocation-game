using UnityEngine;
using System.Collections;

public class MainMenuEcho : MonoBehaviour
{
    private Vector3 pos;
    private AudioSource audioSource;
    public AudioClip clickerSound;
    public float soundEffectVolume = 1;
    public float minWaitSecs = 6;
    public float maxWaitSecs = 10;
    public float pulseDuration = 5;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // Heard equally in both ears 

        pos.z = transform.position.z;

        StartCoroutine(EmitEcho());
    }

    IEnumerator EmitEcho()
    {
        yield return new WaitForSeconds(3);
        while (true)
        {
            pos.x = Random.Range(-12f, 12f);
            pos.y = Random.Range(-7f, 7f);
            transform.position = pos;

            GlobalEchoSystem.PingCustomDuration(gameObject, transform.position, pulseDuration);
            audioSource.PlayOneShot(clickerSound, soundEffectVolume);

            float secs = Random.Range(minWaitSecs, maxWaitSecs);
            yield return new WaitForSeconds(secs);
        }
    }
}
