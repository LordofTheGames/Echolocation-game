using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffectsManager : MonoBehaviour
{
    private static ScreenEffectsManager _instance;
    public static ScreenEffectsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ScreenEffectsManager>();
                if (_instance == null)
                {
                    var go = new GameObject("ScreenEffectsManager");
                    _instance = go.AddComponent<ScreenEffectsManager>();
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Screen flash defaults")]
    [SerializeField] [Range(0.2f, 2f)] private float defaultFlashDuration = 0.75f;

    [Header("Camera shake defaults")]
    [SerializeField] [Range(0.1f, 2f)] private float defaultShakeDuration = 0.4f;
    [SerializeField] [Range(0.01f, 0.5f)] private float defaultShakeIntensity = 0.15f;

    private Canvas overlayCanvas;
    private Image flashImage;
    private Coroutine flashRoutine;
    private Coroutine shakeRoutine;
    private Transform cameraTransform;
    private Vector3 cameraLocalPosition;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            cameraTransform = mainCam.transform;
            cameraLocalPosition = cameraTransform.localPosition;
        }
        CreateFlashOverlay();
    }

    private void CreateFlashOverlay()
    {
        var go = new GameObject("ScreenFlash_Canvas");
        go.transform.SetParent(transform);

        overlayCanvas = go.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 32767;
        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.AddComponent<GraphicRaycaster>();

        var panel = new GameObject("FlashPanel");
        panel.transform.SetParent(go.transform, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        flashImage = panel.AddComponent<Image>();
        flashImage.color = new Color(1f, 1f, 1f, 0f);
        flashImage.raycastTarget = false;

        go.SetActive(true);
    }

    public void ScreenFlash(float duration = -1f)
    {
        if (duration < 0f) duration = defaultFlashDuration;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        if (flashImage == null) yield break;

        float half = duration * 0.5f;
        flashImage.color = new Color(1f, 1f, 1f, 1f);

        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            flashImage.color = new Color(1f, 1f, 1f, 1f - t);
            yield return null;
        }

        flashImage.color = new Color(1f, 1f, 1f, 0f);
        flashRoutine = null;
    }

    public void CameraShake(float duration = -1f, float intensity = -1f)
    {
        if (duration < 0f) duration = defaultShakeDuration;
        if (intensity < 0f) intensity = defaultShakeIntensity;
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(duration, intensity));
    }

    private IEnumerator ShakeRoutine(float duration, float intensity)
    {
        if (cameraTransform == null)
        {
            shakeRoutine = null;
            yield break;
        }

        float elapsed = 0f;
        cameraLocalPosition = cameraTransform.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float decay = 1f - (elapsed / duration);
            Vector3 offset = new Vector3(
                (Random.value - 0.5f) * 2f * intensity * decay,
                (Random.value - 0.5f) * 2f * intensity * decay,
                (Random.value - 0.5f) * 2f * intensity * decay
            );
            cameraTransform.localPosition = cameraLocalPosition + offset;
            yield return null;
        }

        cameraTransform.localPosition = cameraLocalPosition;
        shakeRoutine = null;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}
