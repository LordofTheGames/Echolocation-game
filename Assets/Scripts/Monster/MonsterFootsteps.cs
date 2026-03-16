using UnityEngine;
using Unity.Behavior;

public class MonsterFootsteps : MonoBehaviour
{
    [System.Serializable]
    public struct MoveSettings
    {
        public string name;
        public float maxSpeed;       
        public float stepDistance;   
        [Range(0f, 30f)] public float volume; 
        public int echoRays;            // Number of rays for the effect
        [Range(0f, 50f)]
        public float maxDistance;       // Max Distance rays travel
        public float volForMonster;     // The "volume" the monster hears

    }

    [Header("Audio Settings")]
    public AudioClip footstepSound;
    public AudioSource audioSource;

    [Header("Echo Settings")]
    public float footSeparation = 1f;   // Distance from center to foot
    public float echoAngle = 360f;      // 360 for a full ripple around the foot

    [Header("Movement Profiles")]
    public MoveSettings patrol = new MoveSettings { name = "Patrol", maxSpeed = 0f, stepDistance = 3f, volume = 2f, echoRays = 500, maxDistance = 5f, volForMonster = 0f };
    public MoveSettings investigate = new MoveSettings { name = "Investigate", maxSpeed = 0f, stepDistance = 4f, volume = 5f, echoRays = 2000, maxDistance = 10f, volForMonster = 0f };
    public MoveSettings chase = new MoveSettings { name = "Chase", maxSpeed = 0f, stepDistance = 5f, volume = 10f, echoRays = 5000, maxDistance = 20f, volForMonster = 0f };
    public MoveSettings runAway = new MoveSettings { name = "Run Away", maxSpeed = 0f, stepDistance = 5f, volume = 10f, echoRays = 5000, maxDistance = 20f, volForMonster = 0f };


    private Vector3 lastPos;
    private float distanceTraveled;
    private MoveSettings currentSettings;
    private float smoothedSpeed; 
    private bool isRightFoot = false; // Toggle for left/right steps

    private MonsterMovementType movementType;
    private BehaviorGraphAgent agent;

    void Start()
    {
        lastPos = transform.position;
        currentSettings = patrol;
        agent = GetComponent<BehaviorGraphAgent>();
    }

    void Update()
    {
        // Calculate REAL distance moved this frame
        Vector3 currentPos = transform.position;
        float moveDistance = Vector3.Distance(new Vector3(currentPos.x, 0, currentPos.z), new Vector3(lastPos.x, 0, lastPos.z));
        // float rawSpeed = moveDistance / Time.deltaTime;

        // Smooth speed for profile selection
        // smoothedSpeed = Mathf.Lerp(smoothedSpeed, rawSpeed, Time.deltaTime * 8f);

        // Select Profile
        // if (smoothedSpeed <= crouch.maxSpeed) currentSettings = crouch;
        // else if (smoothedSpeed <= walk.maxSpeed + 0.5f) currentSettings = walk;
        // else currentSettings = sprint;
        agent.BlackboardReference.GetVariableValue("MonsterMovementType", out movementType);
        switch (movementType)
        {
            case MonsterMovementType.Patrolling:
                currentSettings = patrol;
                break;
            case MonsterMovementType.Investigating:
                currentSettings = investigate;
                break;
            case MonsterMovementType.Chasing:
                currentSettings = chase;
                break;
            case MonsterMovementType.RunningAway:
                currentSettings = runAway;
                break;
        }

        if (moveDistance > 0.001f) // Accumulate Distance/normal behaviour
        {
            distanceTraveled += moveDistance;

            if (distanceTraveled >= currentSettings.stepDistance)
            {
                PlayStep();
                distanceTraveled -= currentSettings.stepDistance; 
            }
        }
        
        lastPos = currentPos;
    }

    void PlayStep()
    {
        // Audio
        if (footstepSound != null)
        {
            audioSource.pitch = Random.Range(0.92f, 1.08f);
            audioSource.PlayOneShot(footstepSound, currentSettings.volume);
        }

        // Calculate Foot Position
        isRightFoot = !isRightFoot; // Toggle foot
        float dirMultiplier = isRightFoot ? 1f : -1f;
        Vector3 footPos = transform.position + (transform.right * footSeparation * dirMultiplier);
        
        // Ensure the echo spawns at ground level (optional, assumes pivot is at feet)
        footPos.y = transform.position.y; 

        // Trigger Echo
        // We use footPos as origin, and transform.forward for direction (though if angle is 360, direction doesn't matter)
        GlobalEchoSystem.Ping(this.gameObject, footPos, transform.forward, echoAngle, 0.3f, currentSettings.echoRays, currentSettings.volForMonster, true);
    }
}
