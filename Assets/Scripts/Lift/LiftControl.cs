using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LiftControl : MonoBehaviour
{
    [Header("Lift Settings")]
    [SerializeField] private float maxHeight = 10f;
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private bool startAtBottom = true;
    [SerializeField] private float speed = 3f;

    [Header("QTE (Uplift Only)")]
    [SerializeField] private float qteInterval = 3f;
    [SerializeField] private float qteTimeLimit = 1f;
    [SerializeField] private int maxConsecutiveFails = 3;
    [SerializeField] private TMP_FontAsset qteFont;
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private Color circleColor = new(1f, 0.3f, 0.2f, 0.9f);

    private static readonly KeyCode[] QteKeys = { KeyCode.J, KeyCode.K, KeyCode.L };

    private Transform LiftBody;
    private bool moving = false;
    private Vector3 startPos;
    private Vector3 endPos;
    private CharacterController playerController = null;
    private DetectObjectOutline outlineScript;
    private InputAction interactAction;

    private float currentSpeed;
    private float nextQteTime;
    private KeyCode? activeQteKey;
    private float qteStartTime;
    private int consecutiveFails;
    private GameObject qteCanvas;
    private TextMeshProUGUI qtePromptText;
    private Image qteCountdownRing;
    private RectTransform qteCountdownRect;

    private void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        LiftBody = transform.parent.GetChild(0);
        startPos = LiftBody.position;
        startPos.y += minHeight;
        endPos = LiftBody.position;
        endPos.y += maxHeight;
        outlineScript = GameObject.FindGameObjectWithTag("Player").GetComponent<DetectObjectOutline>();
        outlineScript.ignoreLiftChain = true;
        currentSpeed = speed;
        CreateQteUI();
    }

    private void CreateQteUI()
    {
        qteCanvas = new GameObject("LiftQTE_Canvas");
        var canvas = qteCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        qteCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        qteCanvas.AddComponent<GraphicRaycaster>();

        var panel = new GameObject("QTE_Panel");
        panel.transform.SetParent(qteCanvas.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(400f, 400f);
        panelRect.anchoredPosition = Vector2.zero;

        var ringGo = new GameObject("QTE_CountdownRing");
        ringGo.transform.SetParent(panel.transform, false);
        var ringRect = ringGo.AddComponent<RectTransform>();
        ringRect.anchorMin = Vector2.zero;
        ringRect.anchorMax = Vector2.one;
        ringRect.offsetMin = Vector2.zero;
        ringRect.offsetMax = Vector2.zero;
        qteCountdownRect = ringRect;

        qteCountdownRing = ringGo.AddComponent<Image>();
        qteCountdownRing.sprite = circleSprite;
        qteCountdownRing.color = circleColor;

        var textGo = new GameObject("QTE_Prompt");
        textGo.transform.SetParent(panel.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(320f, 120f);
        textRect.anchoredPosition = Vector2.zero;

        qtePromptText = textGo.AddComponent<TextMeshProUGUI>();
        qtePromptText.font = qteFont;
        qtePromptText.alignment = TextAlignmentOptions.Center;
        qtePromptText.fontSize = 72;
        qtePromptText.color = Color.white;

        qteCanvas.SetActive(false);
    }

    private void ShowQte(KeyCode key)
    {
        activeQteKey = key;
        qteStartTime = Time.time;
        if (qtePromptText != null)
            qtePromptText.text = key.ToString();
        if (qteCountdownRect != null)
            qteCountdownRect.localScale = Vector3.one;
        if (qteCanvas != null)
            qteCanvas.SetActive(true);
    }

    private void UpdateQteCountdownVisual()
    {
        if (qteCountdownRect == null) return;
        float elapsed = Time.time - qteStartTime;
        float remaining = 1f - Mathf.Clamp01(elapsed / qteTimeLimit);
        qteCountdownRect.localScale = Vector3.one * remaining;
    }

    private void HideQte()
    {
        activeQteKey = null;
        if (qteCanvas != null) qteCanvas.SetActive(false);
    }

    private void OnQteSuccess()
    {
        consecutiveFails = 0;
        currentSpeed = speed;
        HideQte();
        nextQteTime = Time.time + qteInterval;
    }

    private void OnQteFail()
    {
        currentSpeed = 0f;
        consecutiveFails++;
        HideQte();
        nextQteTime = Time.time + qteInterval;
        if (consecutiveFails >= maxConsecutiveFails)
            SceneManager.LoadScene("GameOver");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<CharacterController>();
            outlineScript.ignoreLiftChain = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = null;
            outlineScript.ignoreLiftChain = true;
        }
    }

    private void Update()
    {
        if (playerController != null && !moving && interactAction.WasPressedThisFrame())
        {
            outlineScript.ignoreLiftChain = true;
            moving = true;
            if (startAtBottom)
                nextQteTime = Time.time + qteInterval;
        }
        if (moving)
        {
            if (startAtBottom && activeQteKey.HasValue)
            {
                UpdateQteCountdownVisual();
                KeyCode expected = activeQteKey.Value;
                bool hit = (expected == KeyCode.J && Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame) ||
                           (expected == KeyCode.K && Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame) ||
                           (expected == KeyCode.L && Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame);
                if (hit)
                {
                    OnQteSuccess();
                }
                else
                {
                    bool wrongKey = Keyboard.current != null &&
                        ((Keyboard.current.jKey.wasPressedThisFrame && expected != KeyCode.J) ||
                         (Keyboard.current.kKey.wasPressedThisFrame && expected != KeyCode.K) ||
                         (Keyboard.current.lKey.wasPressedThisFrame && expected != KeyCode.L));
                    if (wrongKey || (Time.time - qteStartTime) >= qteTimeLimit)
                        OnQteFail();
                }
            }
            else if (startAtBottom && !activeQteKey.HasValue && Time.time >= nextQteTime)
            {
                ShowQte(QteKeys[Random.Range(0, QteKeys.Length)]);
            }

            float moveAmount = (startAtBottom ? currentSpeed : speed) * Time.deltaTime;
            Vector3 newPos = LiftBody.position;

            if (startAtBottom)
            {
                newPos.y += moveAmount;
                if (newPos.y >= endPos.y)
                {
                    newPos = endPos;
                    LiftBody.position = newPos;
                    this.transform.position = newPos;
                    HideQte();
                    Invoke("loadVictoryScreen", 1f); // wait 1 second before loading victory screen
                    return;
                }
            }
            else
            {
                newPos.y -= moveAmount;
                if (newPos.y <= startPos.y)
                {
                    newPos = startPos;
                    LiftBody.position = newPos;
                    this.transform.position = newPos;
                    moving = false;
                    startAtBottom = !startAtBottom;
                    outlineScript.ignoreLiftChain = false;
                    return;
                }
            }

            Vector3 platformMovement = newPos - LiftBody.position;
            LiftBody.position = newPos;
            this.transform.position = newPos;
            if (playerController != null) playerController.Move(platformMovement);
        }
    }

    private void loadVictoryScreen()
    {
        SceneManager.LoadScene("GameVictory");
    }

    private void OnDrawGizmos()
    {
        Transform LiftBody = transform.parent.GetChild(0);
        Vector3 startPos = LiftBody.position;
        startPos.y += minHeight;
        Vector3 endPos = LiftBody.position;
        endPos.y += maxHeight;
        Gizmos.DrawLine(startPos, endPos);
    }
}
