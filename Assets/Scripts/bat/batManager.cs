using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BatFlockManager : MonoBehaviour
{
    public GameObject batPrefab;
    public Transform pointA;
    public Transform pointB;

    public BatMovement centerBat;

    public int count = 25;
    public float spawnRadius = 0.5f;

    public LayerMask obstacleMask;
    public float agentRadius = 0.35f;

    public float minSpeed = 5.5f;
    public float maxSpeed = 7.5f;

    //maximum distance which another bat is considered a neighbor
    public float neighborRadius = 5.0f;
    //distance which bats begin pushing away from each other
    public float separationRadius = 0.35f;
    public float separationWeight = 1.2f;
    // force applied to the bats for seperation
    public float maxSeparationForce = 2f;
    //weight of the alignment steering force
    //higher values make bats match the average direction of nearby bats more strongly
    public float alignmentWeight = 2.8f;
    public float cohesionWeight = 5.5f;

    // weight of the force that pulls bats toward the route target.
    public float routeWeight = 1.2f;
    // distance from the current route point required before switching to the other route point
    public float waypointReach = 1.2f;

    //How far ahead a bat checks for obstacles
    public float lookAhead = 6.0f;
    // Weight of the obstacle avoidance steering force.
    public float obstacleWeight = 8.0f;
    // When bat encounters a wall, to what extent is it allowed to deviate from the current direction left, right, up and down
    public float sideAngle = 90f;

    public float turnSpeed = 7.0f;
    public float acceleration = 8.0f;
    public float flightHeightOffset = 1.5f;
    // Amplitude of subtle vertical bobbing motion
    public float bobAmp = 0.04f;
    public float bobFreq = 1.4f;
    //Amplitude of random motion noise added to bat flight.
    public float noiseAmp = 0.06f;
    public float noiseFreq = 0.8f;
    // The angle at which the body leans when turning
    public float bankAngle = 25f;

    // Controls how much the bat looks toward its current movement direction
    // versus its intended steering direction.
    [Range(0.5f, 1f)] public float lookIntentBlend = 0.82f;

    //The extent to which the head lifts or presses up and down when bat rises or falls
    [Range(0f, 25f)] public float pitchFromClimb = 12f;
    //When bat turns, will it slightly "press its head down/sink a little"?
    [Range(0f, 15f)] public float turnDipAngle = 6f;

    public float modelForwardOffsetY = 90f;

    public bool depenetrateAfterMove = true;
    //A certain safe distance/buffer thickness retained during a collision
    public float skin = 0.05f;

    public Transform playerCam;
    public float obscureTriggerDistance = 15f;
    public float obscureDistance = 1.5f;

    InputAction scareBatsAction;
    bool isObscuring;
    float scaredUntil;
    public bool IsObscuring => isObscuring;

    public Vector3 GetObscureTargetPosition()
    {
        if (!playerCam)
        {
            return centerBat != null ? centerBat.transform.position : transform.position;
        }
        return playerCam.position + playerCam.forward * obscureDistance;
    }

    [HideInInspector] public readonly List<BatMovement> agents = new();

    void Update()
    {
        if (!centerBat || !playerCam) return;
        float distance = Vector3.Distance(playerCam.position, centerBat.transform.position);
        if (!isObscuring && Time.time > scaredUntil && distance < obscureTriggerDistance)
            isObscuring = true;
    }

    void Start()
    {
        scareBatsAction = InputSystem.actions.FindAction("ScareBats");
        if (scareBatsAction != null)
        {
            scareBatsAction.performed += OnScareBats;
            scareBatsAction.Enable();
        }

        if (!pointA || !pointB) return;

        agents.Clear();

        if (centerBat != null)
        {
            centerBat.transform.position = pointA.position;
            // random value which is given to each bat
            centerBat.Init(this, Random.value * 9999f);
            agents.Add(centerBat);
        }

        int needSpawn = Mathf.Max(0, count - agents.Count);
        if (needSpawn <= 0 || !batPrefab) return;

        // random gereration of bats within a sphere around the spawn point
        for (int i = 0; i < needSpawn; i++)
        {
            Vector3 pos = pointA.position + Random.insideUnitSphere * spawnRadius;
            float spawnHeight = (pointA.position.y + pointB.position.y) * 0.5f + flightHeightOffset;
            pos.y = spawnHeight + Random.Range(-0.2f, 0.2f);

            var go = Instantiate(batPrefab, pos, Quaternion.identity);

            var agent = go.GetComponent<BatMovement>();
            if (!agent) agent = go.AddComponent<BatMovement>();

            agent.Init(this, Random.value * 9999f);
            agents.Add(agent);
        }
    }

    void OnScareBats(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isObscuring = false;
            scaredUntil = Time.time + 4f;
        }
    }

    void OnDestroy()
    {
        if (scareBatsAction != null)
        {
            scareBatsAction.performed -= OnScareBats;
            scareBatsAction.Disable();
        }
    }
}