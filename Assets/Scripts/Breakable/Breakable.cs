using UnityEngine;

[SelectionBase]
public class Breakable : MonoBehaviour
{

    [SerializeField] GameObject IntactObject;
    [SerializeField] GameObject BrokenObject;
    [SerializeField] AudioClip breakSound;
    [SerializeField] float breakSoundVolume = 1f;
    [Range(0,1)]
    [SerializeField] float minRelativeVolume = 0.5f;
    [Range(0,1)]
    [SerializeField] float minRelativePitch = 0.5f;
    [SerializeField] float holdTimeToBreak = 1.2f;
    [SerializeField] float maxDistanceToMic = 5f;
    [SerializeField] float secondsUntilDestroy = 2f;

    [SerializeField] AudioSource audioSource;
    bool _hasBroken;
    float _holdTimer;

    private GameObject player;
    private MicInput micInput;

    private void Start()
    {
        micInput = GameObject.Find("MicInput").GetComponent<MicInput>();
        player = GameObject.FindGameObjectWithTag("Player");
        IntactObject.SetActive(true);
        BrokenObject.SetActive(false);
        GlobalEchoSystem system = GameObject.Find("GlobalEchoSystem").GetComponent<GlobalEchoSystem>();
    }

    private void Update()
    {
        if (_hasBroken || micInput == null) return;

        if (Vector3.Distance(transform.position, player.transform.position) > maxDistanceToMic)
        {
            _holdTimer = 0f;
            return;
        }

        if (micInput.relativePitch >= minRelativePitch && micInput.relativeVolume >= minRelativeVolume)
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
        IntactObject.SetActive(false);
        BrokenObject.SetActive(true);
        audioSource.PlayOneShot(breakSound, breakSoundVolume);

        Destroy(gameObject, secondsUntilDestroy);
    }
}
