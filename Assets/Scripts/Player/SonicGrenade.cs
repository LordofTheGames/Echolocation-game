using System.Collections;
using UnityEngine;
using Unity.Behavior;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class SonicGrenade : MonoBehaviour
{
    [Header("First impact audio")]
    public AudioClip impactClip;
    [Range(0f, 5f)] public float impactVolume = 1f;
    public AudioClip ringingClip;
    [Range(0f, 5f)] public float ringingVolume = 1f;

    [Header("Screen")]
    [Range(0.4f, 2f)] public float screenFlashDuration = 1.1f;

    [Header("360 echo burst")]
    public float echoPingOffset = 0.3f;
    [Range(5000, 100000)] public int echoNumRays = 100000;
    [Range(15f, 100000f)] public float echoMaxDistance = 100000f;
    public float echoMonsterVolume = 1000f;

    public float minDelayAfterArm = 0.08f;

    private bool armed;
    private float armTime = -999f;
    private bool hasTriggered;

    private AudioSource audioSource;
    private BehaviorGraphAgent agent;

    public void Start()
    {
        agent = GameObject.Find("Monster").GetComponent<BehaviorGraphAgent>();
        GameObject player = GameObject.Find("Player");
        if (audioSource == null)
            audioSource = player.AddComponent<AudioSource>();
    }

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
            // AudioSource.PlayClipAtPoint(impactClip, contact.point, impactVolume);
            audioSource.PlayOneShot(impactClip, impactVolume);
        if (ringingClip != null)
            audioSource.PlayOneShot(ringingClip, ringingVolume);

        if (ScreenEffectsManager.Instance != null)
            ScreenEffectsManager.Instance.ScreenFlash(screenFlashDuration);

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

        agent.BlackboardReference.SetVariableValue("sonicGrenadeTriggered", true);
        agent.BlackboardReference.SetVariableValue("sonicGrenadePosition", transform.position);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Unregister item when destroyed
        if (GlobalEchoSystem.Instance != null)
        {
            foreach (Collider col in GetComponentsInChildren<Collider>())
            {
                GlobalEchoSystem.Instance.UnregisterCollider(col);
            }
        }
    }
}
