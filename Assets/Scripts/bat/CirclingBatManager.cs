using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CirclingBatManager : MonoBehaviour
{
    public GameObject batPrefab;
    public Transform centerPoint;
    public Transform endPoint;
    public CirclingBatMovement centerBat;

    public int count = 20;
    public float spawnRadius = 0.5f;
    public float minSpeed = 1.8f;
    public float maxSpeed = 3.2f;

    public float circleRadius = 1.2f;
    public float hoverHeightSpread = 1.6f;
    public float pullBackWeight = 4f;
    public float hardClampRadius = 2f;

    public LayerMask obstacleMask;
    public float agentRadius = 0.35f;
    public float skin = 0.05f;
    public bool depenetrateAfterMove = true;

    public float acceleration = 8f;

    public Transform playerCam;
    public float scareTriggerDistance = 10f;
    public string scareActionName = "ScareBats";
    public float escapeSpeedMultiplier = 1.5f;
    public float despawnDistance = 1.2f;

    [Range(0f, 1f)]
    [SerializeField] float minRelativeVolume = 0.35f;
    [Range(0f, 1f)]
    [SerializeField] float minRelativePitch = 0.35f;
    [SerializeField] float holdTimeToScare = 1.0f;

    [HideInInspector] public readonly List<CirclingBatMovement> agents = new();

    InputAction scareBatsAction;
    bool isScared;
    bool playerInRange;
    public bool IsScared => isScared;

    private MicInput micInput;
    float micHoldTimer;

    void Awake()
    {
        var micGo = GameObject.Find("MicInput");
        if (micGo != null)
            micInput = micGo.GetComponent<MicInput>();
    }

    void Start()
    {

        if (!string.IsNullOrEmpty(scareActionName))
        {
            scareBatsAction = InputSystem.actions.FindAction(scareActionName);
            if (scareBatsAction != null)
            {
                scareBatsAction.performed += OnScareBats;
                scareBatsAction.Enable();
            }
        }

        SpawnBats();
    }

    void SpawnBats()
    {
        agents.Clear();

        if (centerBat != null)
        {
            centerBat.Init(this, Random.value * 9999f);
            if (!agents.Contains(centerBat))
                agents.Add(centerBat);
        }

        int spawnCount = Mathf.Max(0, count - agents.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * spawnRadius;
            offset.y *= 0.4f;
            Vector3 pos = centerPoint.position + offset;

            var go = Instantiate(batPrefab, pos, Quaternion.identity, transform);
            var agent = go.GetComponent<CirclingBatMovement>();
            if (!agent) agent = go.AddComponent<CirclingBatMovement>();

            agent.Init(this, Random.value * 9999f);
            agents.Add(agent);
        }
    }

    void Update()
    {
        if (playerCam == null || centerPoint == null) return;

        float distance = Vector3.Distance(playerCam.position, centerPoint.position);
        playerInRange = distance <= scareTriggerDistance && !isScared;

        if (isScared || micInput == null) return;

        if (!playerInRange)
        {
            micHoldTimer = 0f;
            return;
        }

        if (micInput.relativePitch >= minRelativePitch && micInput.relativeVolume >= minRelativeVolume)
        {
            micHoldTimer += Time.deltaTime;
            if (micHoldTimer >= holdTimeToScare)
                isScared = true;
        }
        else
        {
            micHoldTimer = 0f;
        }
    }

    void OnScareBats(InputAction.CallbackContext context)
    {
        if (!context.performed || isScared || playerCam == null || centerPoint == null) return;

        if (!playerInRange) return;

        isScared = true;
    }

    public void NotifyAgentDespawn(CirclingBatMovement agent)
    {
        if (agent != null)
        {
            agents.Remove(agent);
            if (centerBat == agent) centerBat = null;
        }

        if (agents.Count == 0)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (scareBatsAction != null)
        {
            scareBatsAction.performed -= OnScareBats;
        }
    }
}
