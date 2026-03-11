using UnityEngine;

[SelectionBase]
public class Breakable : MonoBehaviour
{
    [SerializeField] GameObject box;
    [SerializeField] GameObject brokenBox;
    [SerializeField] AudioClip breakSound;
    BoxCollider bc;
    AudioSource _audioSource;

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

    private void OnMouseDown()
    {
        Break();
    }

    private void Break()
    {
        box.SetActive(false);
        brokenBox.SetActive(true);
        bc.enabled = false;
        if (breakSound != null && _audioSource != null)
            _audioSource.PlayOneShot(breakSound);
    }
}
