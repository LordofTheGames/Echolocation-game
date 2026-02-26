using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class SonicGrenade : MonoBehaviour
{
    [Header("First impact audio")]
    public AudioClip impactClip;
    [Range(0f, 3f)] public float explosionVolume = 1.5f;
    [Range(0f, 2f)] public float tinnitusVolume = 1f;
    public float tinnitusDelayAfterExplosion = 0.15f;

    [Header("Screen and camera")]
    [Range(0.4f, 1.5f)] public float screenFlashDuration = 0.75f;
    [Range(0.1f, 1f)] public float cameraShakeDuration = 0.35f;
    [Range(0.02f, 0.4f)] public float cameraShakeIntensity = 0.18f;

    [Header("360 echo burst")]
    public float echoPingOffset = 0.3f;
    [Range(5000, 50000)] public int echoNumRays = 25000;
    [Range(15f, 60f)] public float echoMaxDistance = 35f;
    public float echoMonsterVolume = 150f;

    public float minDelayAfterArm = 0.08f;

    private bool armed;
    private float armTime = -999f;
    private bool hasTriggered;

    public void Arm()
    {
        armed = true;
        hasTriggered = false;
        armTime = Time.time;
        enabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!armed || hasTriggered) return;
        if (Time.time - armTime < minDelayAfterArm) return;

        hasTriggered = true;
        ContactPoint contact = collision.contacts[0];
        Vector3 spawnPoint = contact.point + contact.normal * echoPingOffset;

        if (impactClip != null)
            AudioSource.PlayClipAtPoint(impactClip, contact.point, explosionVolume);

        StartCoroutine(PlayTinnitusDelayed(contact.point));

        if (ScreenEffectsManager.Instance != null)
        {
            ScreenEffectsManager.Instance.ScreenFlash(screenFlashDuration);
            ScreenEffectsManager.Instance.CameraShake(cameraShakeDuration, cameraShakeIntensity);
        }

        if (GlobalEchoSystem.Instance != null)
        {
            GlobalEchoSystem.Ping(
                gameObject,
                spawnPoint,
                Vector3.forward,
                360f,
                1f,
                echoNumRays,
                echoMaxDistance,
                echoMonsterVolume
            );
        }
    }

    private IEnumerator PlayTinnitusDelayed(Vector3 position)
    {
        if (tinnitusDelayAfterExplosion > 0f)
            yield return new WaitForSeconds(tinnitusDelayAfterExplosion);
        if (impactClip != null)
            AudioSource.PlayClipAtPoint(impactClip, position, tinnitusVolume);
    }
}
