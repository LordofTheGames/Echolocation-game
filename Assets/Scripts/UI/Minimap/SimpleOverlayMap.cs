using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SimpleOverlayMap : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float worldHalfExtent = 100f;
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.65f);

    private Canvas _canvas;
    private RectTransform _panelRect;
    private RectTransform _keysRoot;
    private Image _playerDot;

    private readonly List<RectTransform> _keyIconPool = new List<RectTransform>();

    private static Sprite s_whiteSprite;
    private static Sprite s_playerDotSprite;

    private bool _visible;

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
        _panelRect.anchorMin = new Vector2(1f / 6f, 1f / 6f);
        _panelRect.anchorMax = new Vector2(5f / 6f, 5f / 6f);
        _panelRect.offsetMin = Vector2.zero;
        _panelRect.offsetMax = Vector2.zero;

        var panelImage = panelGo.AddComponent<Image>();
        panelImage.sprite = GetWhiteSprite();
        panelImage.color = backgroundColor;
        panelImage.raycastTarget = false;

        var keysGo = new GameObject("Keys");
        keysGo.transform.SetParent(panelGo.transform, false);
        _keysRoot = keysGo.AddComponent<RectTransform>();
        StretchFull(_keysRoot);

        var dotGo = new GameObject("PlayerDot");
        dotGo.transform.SetParent(panelGo.transform, false);
        var dotRect = dotGo.AddComponent<RectTransform>();
        dotRect.anchorMin = dotRect.anchorMax = new Vector2(0.5f, 0.5f);
        dotRect.pivot = new Vector2(0.5f, 0.5f);
        dotRect.sizeDelta = new Vector2(14f, 14f);
        dotRect.anchoredPosition = Vector2.zero;

        _playerDot = dotGo.AddComponent<Image>();
        _playerDot.sprite = GetPlayerDotSprite();
        _playerDot.color = Color.white;
        _playerDot.raycastTarget = false;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
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
        var keyPositions = new List<Vector3>(4);
        foreach (var p in pickups)
        {
            if (p == null || !p.gameObject.activeInHierarchy)
                continue;
            if (p.PickupItemType != ItemType.Key)
                continue;
            keyPositions.Add(p.transform.position);
        }

        float halfPx = Mathf.Min(_panelRect.rect.width, _panelRect.rect.height) * 0.5f - 24f;
        if (halfPx < 8f)
            halfPx = 8f;

        float scale = halfPx / Mathf.Max(0.001f, worldHalfExtent);

        EnsureKeyPool(keyPositions.Count);
        for (int i = 0; i < _keyIconPool.Count; i++)
        {
            bool use = i < keyPositions.Count;
            _keyIconPool[i].gameObject.SetActive(use);
            if (!use)
                continue;

            Vector3 w = keyPositions[i] - player.position;
            var ui = new Vector2(w.x, w.z) * scale;
            float maxR = halfPx;
            if (ui.sqrMagnitude > maxR * maxR)
                ui = ui.normalized * maxR;

            _keyIconPool[i].anchoredPosition = ui;
        }
    }

    private void EnsureKeyPool(int count)
    {
        while (_keyIconPool.Count < count)
        {
            var icon = BuildKeyIcon(_keysRoot);
            _keyIconPool.Add(icon);
        }
    }

    private RectTransform BuildKeyIcon(RectTransform parent)
    {
        var go = new GameObject("KeyDot");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(14f, 14f);
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
