using UnityEngine;

public class GlassBreakReaction : MonoBehaviour
{
    [SerializeField] private Breakable targetBreakable;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rocksFallingClip;
    [SerializeField] private float rocksFallingVolume = 1f;

    [SerializeField] private GameObject[] rockBlockers;

    private bool hasTriggered = false;

    private void OnEnable()
    {
        Breakable.OnBroken += HandleGlassBroken;
    }

    private void OnDisable()
    {
        Breakable.OnBroken -= HandleGlassBroken;
    }

    private void Start()
    {
        foreach (GameObject blocker in rockBlockers)
        {
            if (blocker != null)
                blocker.SetActive(false);
        }
    }

    private void HandleGlassBroken(Breakable broken)
    {
        if (hasTriggered) return;

        if (broken != targetBreakable) return;

        hasTriggered = true;

        if (audioSource != null && rocksFallingClip != null)
        {
            audioSource.PlayOneShot(rocksFallingClip, rocksFallingVolume);
        }

        foreach (GameObject blocker in rockBlockers)
        {
            if (blocker != null)
                blocker.SetActive(true);
        }
    }
}