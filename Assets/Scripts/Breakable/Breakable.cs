using System.Collections;
using UnityEngine;

[SelectionBase]
public class Breakable : MonoBehaviour
{
    [SerializeField] private GameObject intactObject;
    [SerializeField] private GameObject brokenObject;
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private float breakSoundVolume = 1f;

    [Range(0, 1)]
    [SerializeField] private float minRelativeVolume = 0.5f;

    [Range(0, 1)]
    [SerializeField] private float minRelativePitch = 0.5f;

    [SerializeField] private float holdTimeToBreak = 1.2f;
    [SerializeField] private float maxDistanceToMic = 5f;
    [SerializeField] private float secondsUntilHideBrokenVisual = 5f;

    [SerializeField] private AudioSource audioSource;

    private bool hasBroken;
    private float holdTimer;

    private GameObject player;
    private MicInput micInput;

    public bool HasBroken => hasBroken;

    private void Awake()
    {
        micInput = GameObject.Find("MicInput").GetComponent<MicInput>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (intactObject != null) intactObject.SetActive(true);
        if (brokenObject != null) brokenObject.SetActive(false);
    }

    private void Update()
    {
        if (hasBroken || micInput == null || player == null) return;

        if (Vector3.Distance(transform.position, player.transform.position) > maxDistanceToMic)
        {
            holdTimer = 0f;
            return;
        }

        if (micInput.relativePitch >= minRelativePitch && micInput.relativeVolume >= minRelativeVolume)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTimeToBreak)
            {
                Break();
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private void Break()
    {
        if (hasBroken) return;

        hasBroken = true;

        if (intactObject != null) intactObject.SetActive(false);
        if (brokenObject != null) brokenObject.SetActive(true);

        if (audioSource != null && breakSound != null)
        {
            audioSource.PlayOneShot(breakSound, breakSoundVolume);
        }

        StartCoroutine(HideBrokenVisualLater());
    }

    private IEnumerator HideBrokenVisualLater()
    {
        if (secondsUntilHideBrokenVisual <= 0f) yield break;

        yield return new WaitForSeconds(secondsUntilHideBrokenVisual);

        if (brokenObject != null)
        {
            brokenObject.SetActive(false);
        }
    }
}
