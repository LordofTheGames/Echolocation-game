using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SimpleOverlayMap : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector2 panelAnchorMin = new Vector2(0.28f, 0.1f);
    [SerializeField] private Vector2 panelAnchorMax = new Vector2(0.72f, 0.9f);
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.65f);
    [SerializeField] private Sprite playerDirectionSprite;
    [SerializeField] private Vector2 playerArrowSize = new Vector2(112f, 112f);
    [SerializeField] private Vector2 keyMarkerSize = new Vector2(22f, 22f);
    [SerializeField] private float playerMapBottomPadding = 28f;
    [SerializeField] private float keyMapTopPadding = 28f;
    [SerializeField] private float playerTravelToKeyDistance = 60f;

    private Canvas _canvas;
    private RectTransform _panelRect;
    private Image _playerDot;
    private RectTransform _keyMarker;

    private static Sprite s_whiteSprite;
    private static Sprite s_playerDotSprite;

    private bool _visible;
    private bool _hasDistanceReference;
    private float _distanceReference;

    private void Awake()
    {
        if (player == null)
        {
            var tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null)
                player = tagged.transform;
        }

        BuildUi();
        _visible = false;
        _canvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (PauseManager.IsPaused)
        {
            if (_visible)
                SetVisible(false);
            return;
        }

        if (WasMapTogglePressedThisFrame())
            SetVisible(!_visible);

        if (_visible)
            RefreshMarkers();
    }

    private static bool WasMapTogglePressedThisFrame()
    {
        var kb = Keyboard.current ?? UnityEngine.InputSystem.InputSystem.GetDevice<Keyboard>();
        bool fromNew = kb != null && kb.xKey.wasPressedThisFrame;
        bool fromOld = Input.GetKeyDown(KeyCode.X);
        return fromNew || fromOld;
    }

    private void SetVisible(bool on)
    {
        if (_visible == on)
            return;
        _visible = on;
        if (on)
        {
            _hasDistanceReference = false;
            _distanceReference = 0f;
        }
        if (_canvas != null)
            _canvas.gameObject.SetActive(on);
    }

    private void BuildUi()
    {
        var root = new GameObject("SimpleOverlayMapCanvas");
        root.transform.SetParent(transform, false);

        _canvas = root.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 40;

        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var panelGo = new GameObject("MapPanel");
        panelGo.transform.SetParent(root.transform, false);
        _panelRect = panelGo.AddComponent<RectTransform>();
        _panelRect.anchorMin = panelAnchorMin;
        _panelRect.anchorMax = panelAnchorMax;
        _panelRect.offsetMin = Vector2.zero;
        _panelRect.offsetMax = Vector2.zero;

        var panelImage = panelGo.AddComponent<Image>();
        panelImage.sprite = GetWhiteSprite();
        panelImage.color = backgroundColor;
        panelImage.raycastTarget = false;

        var dotGo = new GameObject("PlayerArrow");
        dotGo.transform.SetParent(panelGo.transform, false);
        var dotRect = dotGo.AddComponent<RectTransform>();
        dotRect.anchorMin = dotRect.anchorMax = new Vector2(0.5f, 0.5f);
        dotRect.pivot = new Vector2(0.5f, 0.5f);
        dotRect.sizeDelta = playerArrowSize;
        dotRect.anchoredPosition = Vector2.zero;

        _playerDot = dotGo.AddComponent<Image>();
        _playerDot.sprite = playerDirectionSprite;
        _playerDot.color = Color.white;
        _playerDot.raycastTarget = false;
        _playerDot.preserveAspect = true;
    }

    private Vector2 GetPlayerMapAnchorInPanel()
    {
        float h = _panelRect.rect.height;
        if (h < 2f)
            return Vector2.zero;
        float halfH = h * 0.5f;
        float y = -halfH + playerMapBottomPadding + playerArrowSize.y * 0.5f;
        return new Vector2(0f, y);
    }

    private Vector2 GetFixedKeyAnchorInPanel()
    {
        float h = _panelRect.rect.height;
        if (h < 2f)
            return Vector2.zero;
        float halfH = h * 0.5f;
        float y = halfH - keyMapTopPadding - keyMarkerSize.y * 0.5f;
        return new Vector2(0f, y);
    }

    private Vector2 GetPlayerProgressAnchorInPanel(float progress01, float horizontal01)
    {
        Vector2 from = GetPlayerMapAnchorInPanel();
        Vector2 to = GetFixedKeyAnchorInPanel();
        Vector2 p = Vector2.Lerp(from, to, Mathf.Clamp01(progress01));

        float halfW = _panelRect.rect.width * 0.5f;
        float horizontalLimit = Mathf.Max(0f, halfW - playerArrowSize.x * 0.5f - 12f);
        p.x = horizontal01 * horizontalLimit;
        return p;
    }

    private void RefreshMarkers()
    {
        if (player == null)
        {
            var tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null)
                player = tagged.transform;
        }

        if (player == null)
            return;

        var pickups = FindObjectsByType<PickupItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int keyCount = 0;
        float bestSqr = float.MaxValue;
        Vector3 nearestDelta = Vector3.zero;
        foreach (var p in pickups)
        {
            if (p == null || !p.gameObject.activeInHierarchy || p.PickupItemType != ItemType.Key)
                continue;
            keyCount++;
            Vector3 delta = player.position - p.transform.position;
            float sqr = delta.sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                nearestDelta = delta;
            }
        }

        float progress01 = 0f;
        float horizontal01 = 0f;
        if (keyCount > 0)
        {
            float nearestDistance = Mathf.Sqrt(bestSqr);
            if (!_hasDistanceReference)
            {
                _distanceReference = Mathf.Max(playerTravelToKeyDistance, nearestDistance);
                _hasDistanceReference = true;
            }

            float refD = Mathf.Max(0.001f, _distanceReference);
            progress01 = 1f - Mathf.Clamp01(nearestDistance / refD);
            horizontal01 = Mathf.Clamp(nearestDelta.x / refD, -1f, 1f);
        }

        if (_playerDot != null)
        {
            _playerDot.rectTransform.anchoredPosition = GetPlayerProgressAnchorInPanel(progress01, horizontal01);
            _playerDot.rectTransform.localEulerAngles = new Vector3(0f, 0f, -player.eulerAngles.y);
        }

        if (_keyMarker == null)
            _keyMarker = BuildKeyIcon(_panelRect);

        _keyMarker.SetAsFirstSibling();
        _keyMarker.gameObject.SetActive(keyCount > 0);
        if (keyCount > 0)
            _keyMarker.anchoredPosition = GetFixedKeyAnchorInPanel();
    }

    private RectTransform BuildKeyIcon(RectTransform parent)
    {
        var go = new GameObject("KeyDot");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = keyMarkerSize;
        rt.anchoredPosition = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.sprite = GetPlayerDotSprite();
        img.color = new Color(0.25f, 0.95f, 0.4f, 1f);
        img.raycastTarget = false;

        return rt;
    }

    private static Sprite GetWhiteSprite()
    {
        if (s_whiteSprite == null)
        {
            var tex = Texture2D.whiteTexture;
            s_whiteSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        return s_whiteSprite;
    }

    private static Sprite GetPlayerDotSprite()
    {
        if (s_playerDotSprite == null)
        {
            int r = 6;
            int s = r * 2 + 1;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var c = Color.clear;
            var w = Color.white;
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(r, r));
                    tex.SetPixel(x, y, d <= r - 0.5f ? w : c);
                }
            }

            tex.Apply();
            s_playerDotSprite = Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f), 100f);
        }

        return s_playerDotSprite;
    }
}
