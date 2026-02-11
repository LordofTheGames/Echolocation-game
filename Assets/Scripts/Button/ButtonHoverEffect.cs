using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Effect Settings")]
    [Tooltip("Scale multiplier on hover")]
    [SerializeField] private float hoverScale = 1.2f;
    
    [Tooltip("Animation duration in seconds")]
    [SerializeField] private float animationDuration = 0.2f;
    
    private Vector3 originalScale;
    private RectTransform rectTransform;
    private Coroutine scaleCoroutine;

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
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        Vector3 targetScale = originalScale * hoverScale;
        scaleCoroutine = StartCoroutine(ScaleTo(targetScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
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
