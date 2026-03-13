using UnityEngine;

[SelectionBase]
public class Breakable : MonoBehaviour
{
    public enum BreakableType { Rock, Glass }

    [SerializeField] GameObject box;
    [SerializeField] GameObject brokenBox;
    [SerializeField] AudioClip breakSound;
    [SerializeField] BreakableType breakableType = BreakableType.Rock;
    [SerializeField] MicInput micInput;
    [SerializeField] float minVolumeToBreak = 0.4f;
    [SerializeField] float minPitchToBreak = 100f;
    [SerializeField] float holdTimeToBreak = 1.2f;

    BoxCollider bc;
    AudioSource _audioSource;
    bool _hasBroken;
    float _holdTimer;

    private void Awake()
    {
        box.SetActive(true);
        brokenBox.SetActive(false);
        bc = GetComponent<BoxCollider>();
        if (breakSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (_hasBroken || micInput == null) return;

        bool conditionMet = false;

        if (breakableType == BreakableType.Rock && micInput.volume >= minVolumeToBreak)
            conditionMet = true;
        else if (breakableType == BreakableType.Glass && micInput.pitchHz >= minPitchToBreak)
            conditionMet = true;

        if (conditionMet)
        {
            _holdTimer += Time.deltaTime;
            if (_holdTimer >= holdTimeToBreak)
                Break();
        }
        else
        {
            _holdTimer = 0f;
        }
    }

    private void Break()
    {
        _hasBroken = true;
        box.SetActive(false);
        brokenBox.SetActive(true);
        bc.enabled = false;
        if (breakSound != null && _audioSource != null)
            _audioSource.PlayOneShot(breakSound);
    }
}
