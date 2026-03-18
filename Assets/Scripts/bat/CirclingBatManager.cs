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
    public float minSpeed = 5.5f;
    public float maxSpeed = 7.5f;

    public float neighborRadius = 5.0f;
    public float separationRadius = 0.35f;
    public float separationWeight = 1.2f;
    public float cohesionWeight = 5.5f;
    public float alignmentWeight = 2.8f;

    public float circleRadius = 1.2f;
    public float baseAngularSpeed = 24f;
    public float maxRadiusFromCenter = 2f;
    public float pullBackWeight = 4f;

    public LayerMask obstacleMask;
    public float agentRadius = 0.35f;
    public float lookAhead = 5f;
    public float obstacleWeight = 6f;

    public Transform playerCam;
    public float scareTriggerDistance = 10f;
    public string scareActionName = "ScareBats";
    public float escapeSpeedMultiplier = 1.5f;
    public float despawnDistance = 1.2f;

    [HideInInspector] public readonly List<CirclingBatMovement> agents = new();

    InputAction scareBatsAction;
    bool isScared;
    bool playerInRange;
    public bool IsScared => isScared;

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

        // If a center/leader bat is provided in the scene, initialise and register it first.
        if (centerBat != null)
        {
            centerBat.Init(this, Random.value * 9999f);
            if (!agents.Contains(centerBat))
                agents.Add(centerBat);
        }

        int spawnCount = Mathf.Max(0, count - agents.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            // Random position around center within a small sphere
            Vector3 offset = Random.insideUnitSphere * spawnRadius;
            offset.y *= 0.4f; // keep mostly flat
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

