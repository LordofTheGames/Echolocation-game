using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    public static SoundManager Instance
    {
        get { return _instance; }
    }

    [Header("Default Sound Settings")]
    [SerializeField] private AudioClip defaultClickSound;
    
    [SerializeField] [Range(0f, 1f)] private float defaultClickVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    public void PlayButtonClickSound(AudioClip customSound = null, float volume = -1f)
    {
        AudioClip soundToPlay = customSound != null ? customSound : defaultClickSound;
        float volumeToUse = volume >= 0f ? volume : defaultClickVolume;

        if (soundToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundToPlay, volumeToUse);
        }
    }

    public void SetDefaultClickSound(AudioClip sound)
    {
        defaultClickSound = sound;
    }

    public void SetDefaultClickVolume(float volume)
    {
        defaultClickVolume = Mathf.Clamp01(volume);
    }
}
