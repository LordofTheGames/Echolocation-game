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

    private Image flashImage;
    private Coroutine flashRoutine;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        CreateFlashOverlay();
    }

    private void CreateFlashOverlay()
    {
        var go = new GameObject("ScreenFlash_Canvas");
        go.transform.SetParent(transform);

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
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

        float holdTime = duration * 0.55f;
        float fadeTime = duration * 0.45f;
        flashImage.color = new Color(1f, 1f, 1f, 1f);

        float elapsed = 0f;
        while (elapsed < holdTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            flashImage.color = new Color(1f, 1f, 1f, 1f - t);
            yield return null;
        }

        flashImage.color = new Color(1f, 1f, 1f, 0f);
        flashRoutine = null;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}
