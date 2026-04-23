using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

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

    private List<Vector3> _blueDotWorldPositions = new List<Vector3>();
    private List<RectTransform> _blueDotRects = new List<RectTransform>();

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
        var kb = Keyboard.current ?? InputSystem.GetDevice<Keyboard>();
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

        PickupItem targetKey = null;
        var pickups = FindObjectsByType<PickupItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var p in pickups)
        {
            if (p != null && p.gameObject.activeInHierarchy && p.PickupItemType == ItemType.Key)
            {
                targetKey = p;
                break;
            }
        }

        // Ensure the key marker UI exists
        if (_keyMarker == null)
            _keyMarker = BuildKeyIcon(_panelRect);

        _keyMarker.SetAsLastSibling(); // Ensure Key stays on top
        _keyMarker.gameObject.SetActive(targetKey != null);

        if (targetKey != null)
        {
            float panelHalfHeight = _panelRect.rect.height * 0.5f;
            float panelHalfWidth = _panelRect.rect.width * 0.5f;

            // Position the Key at the top of the map
            float keyY = panelHalfHeight - keyMapTopPadding - (keyMarkerSize.y * 0.5f);
            Vector2 keyUIPos = new Vector2(0f, keyY);
            _keyMarker.anchoredPosition = keyUIPos;

            // Calculate Map Scale
            float fullPanelHeight = _panelRect.rect.height;
            float pixelsPerUnit = fullPanelHeight / Mathf.Max(0.001f, playerTravelToKeyDistance);

            // Update blue dots
            for (int i = 0; i < _blueDotWorldPositions.Count; i++)
            {
                // Get offset from the key, exactly like the player does
                Vector3 dotWorldOffset = _blueDotWorldPositions[i] - targetKey.transform.position;
                Vector2 dotUiOffset = new Vector2(dotWorldOffset.x, dotWorldOffset.z);
                Vector2 dotFinalPos = keyUIPos + (dotUiOffset * pixelsPerUnit);

                // Clamp to panel boundaries so they don't bleed off the map
                dotFinalPos.x = Mathf.Clamp(dotFinalPos.x, -panelHalfWidth + 8f, panelHalfWidth - 8f);
                dotFinalPos.y = Mathf.Clamp(dotFinalPos.y, -panelHalfHeight + 8f, panelHalfHeight - 8f);

                // Apply the position and ensure it is visible
                _blueDotRects[i].anchoredPosition = dotFinalPos;
                _blueDotRects[i].gameObject.SetActive(true);
            }

            // Get the 3D world offset (Player minus Key)
            Vector3 worldOffset = player.position - targetKey.transform.position;
            Vector2 uiOffset = new Vector2(worldOffset.x, worldOffset.z);
            Vector2 finalPlayerPos = keyUIPos + (uiOffset * pixelsPerUnit);

            // Clamp the player dot
            finalPlayerPos.x = Mathf.Clamp(finalPlayerPos.x, -panelHalfWidth + (playerArrowSize.x * 0.5f), panelHalfWidth - (playerArrowSize.x * 0.5f));
            finalPlayerPos.y = Mathf.Clamp(finalPlayerPos.y, -panelHalfHeight + (playerArrowSize.y * 0.5f), panelHalfHeight - (playerArrowSize.y * 0.5f));

            // Update Player Dot UI
            _playerDot.rectTransform.anchoredPosition = finalPlayerPos;
            _playerDot.rectTransform.localEulerAngles = new Vector3(0f, 0f, -player.eulerAngles.y);
            _playerDot.gameObject.SetActive(true);
            _playerDot.transform.SetAsLastSibling(); // Ensure Player stays on top
        }
        else
        {
            if (_playerDot != null) _playerDot.gameObject.SetActive(false);
            
            for (int i = 0; i < _blueDotRects.Count; i++)
            {
                if (_blueDotRects[i] != null) 
                    _blueDotRects[i].gameObject.SetActive(false);
            }
        }
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
        img.color = Color.yellow;
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

    public void AddStaticBlueDot(Vector3 worldPosition)
    {
        _blueDotWorldPositions.Add(worldPosition);

        // Create the UI element
        var go = new GameObject("BlueDot");
        go.transform.SetParent(_panelRect, false);
        
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(16f, 16f); // Slightly smaller than the key
        
        var img = go.AddComponent<Image>();
        img.sprite = GetPlayerDotSprite();
        img.color = new Color(0.2f, 0.6f, 1f, 1f); // Bright blue
        img.raycastTarget = false;

        // Push it to the back of the UI hierarchy so the player and key render on top of it
        go.transform.SetAsFirstSibling();

        _blueDotRects.Add(rt);
        
        // If the map is currently open, immediately refresh so it shows up
        if (_visible)
        {
            RefreshMarkers();
        }
        else
        {
            go.SetActive(false); // Hide it until the map is opened
        }
    }
}
