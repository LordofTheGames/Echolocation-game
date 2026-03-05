using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, 
ISelectHandler, IDeselectHandler, ISubmitHandler
{
    [Header("Hover Effect Settings")]
    [SerializeField] private float hoverScale = 1.2f;
    
    [SerializeField] private float animationDuration = 0.2f;
    
    [Header("Click Sound Settings")]
    [SerializeField] private AudioClip clickSound;
    
    [SerializeField] [Range(0f, 1f)] private float clickSoundVolume = 1f;

    private Vector3 originalScale;
    private RectTransform rectTransform;
    private Coroutine scaleCoroutine;
    private AudioSource audioSource;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalScale = rectTransform.localScale;
        }
        else
        {
            originalScale = transform.localScale;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = clickSoundVolume;
            audioSource.spatialBlend = 0f;
        }
    }
    
    private void Start()
    {
        if (audioSource != null)
        {
            audioSource.volume = clickSoundVolume;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        Selection();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Selection();
    }

    private void Selection()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        Vector3 targetScale = originalScale * hoverScale;
        scaleCoroutine = StartCoroutine(ScaleTo(targetScale));
    }

    public void OnDeselect(BaseEventData eventdata)
    {
        Deselection();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Deselection();
    }

    private void Deselection()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickSound();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        PlayClickSound();
    }
    
    public void TestClickSound()
    {
        PlayClickSound();
    }

    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f;
                }
            }
            
            if (audioSource != null)
            {
                audioSource.PlayOneShot(clickSound, clickSoundVolume);
            }
        }
        else if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClickSound(null, clickSoundVolume);
        }
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale;
        if (rectTransform != null)
        {
            startScale = rectTransform.localScale;
        }
        else
        {
            startScale = transform.localScale;
        }

        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / animationDuration;
            
            t = EaseOutBack(t);
            
            Vector3 currentScale = Vector3.Lerp(startScale, targetScale, t);
            
            if (rectTransform != null)
            {
                rectTransform.localScale = currentScale;
            }
            else
            {
                transform.localScale = currentScale;
            }
            
            yield return null;
        }
        
        if (rectTransform != null)
        {
            rectTransform.localScale = targetScale;
        }
        else
        {
            transform.localScale = targetScale;
        }
        
        scaleCoroutine = null;
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private void OnDisable()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
        
        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
        }
        else
        {
            transform.localScale = originalScale;
        }
    }
}
