using System.Collections;
using UnityEngine;

public class TimedEchoEmitter : MonoBehaviour
{
    public AudioClip pingSound;

    [Range(0f, 30f)]
    public float soundVolume = 1f;

    public float initialDelay = 0.2f;

    public float pingInterval = 2f;

    public int maxPings = 5;

    public float angle = 360f;
    public float uniformity = 1f;
    public int numRays = 8000;
    public float maxDistance = 25f;
    public float monsterVolume = 100f;

    public float minDelayAfterThrow = 0.05f;

    private bool armed;
    private Coroutine pingRoutine;
    private bool armOnNextCollision;
    private float armOnNextCollisionTime;

    public void SetArmOnNextCollision()
    {
        armOnNextCollision = true;
        armOnNextCollisionTime = Time.time;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!armOnNextCollision || armed) return;
        if (Time.time - armOnNextCollisionTime < minDelayAfterThrow) return;
        armOnNextCollision = false;
        Arm();
    }

    public void Arm()
    {
        Transform root = transform.root;
        if (root != null && !root.gameObject.activeInHierarchy)
        {
            root.gameObject.SetActive(true);
        }
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        if (!enabled)
        {
            enabled = true;
        }

        if (armed)
            return;

        armed = true;

        if (pingRoutine != null)
        {
            StopCoroutine(pingRoutine);
        }

        pingRoutine = StartCoroutine(PingLoop());
    }

    private IEnumerator PingLoop()
    {
        if (initialDelay > 0f)
        {
            yield return new WaitForSeconds(initialDelay);
        }

        int emitted = 0;

        while (armed && emitted < maxPings)
        {
            Vector3 position = transform.position;

            if (pingSound != null)
            {
                AudioSource.PlayClipAtPoint(pingSound, position, soundVolume);
            }

            if (GlobalEchoSystem.Instance != null)
            {
                GlobalEchoSystem.Ping(gameObject, position, Vector3.forward, angle, uniformity, numRays, maxDistance, monsterVolume);
            }

            emitted++;

            if (emitted >= maxPings)
            {
                break;
            }

            yield return new WaitForSeconds(pingInterval);
        }

        armed = false;
        pingRoutine = null;
    }

    private void OnDisable()
    {
        if (pingRoutine != null)
        {
            StopCoroutine(pingRoutine);
            pingRoutine = null;
        }

        armed = false;
        armOnNextCollision = false;
    }
}

