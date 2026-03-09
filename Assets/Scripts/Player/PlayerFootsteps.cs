using UnityEngine;

[System.Serializable]
public struct MoveSettings
{
    public string name;
    public float stepDistance;
    [Range(0f, 30f)] public float volume;
    public int echoRays;            // Number of rays for the effect
    [Range(0f, 50f)]
    public float maxDistance;       // Max Distance rays travel
    public float volForMonster;     // The "volume" the monster hears
}

public enum MoveState
{
    CROUCH,
    WALK,
    SPRINT
}

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip footstepSound;
    public AudioSource audioSource;

    [Header("Echo Settings")]
    public float footSeparation = 1f;   // Distance from center to foot
    public float echoAngle = 360f;      // 360 for a full ripple around the foot

    [Header("Movement Profiles")]
    public MoveSettings crouch = new MoveSettings { name = "Crouch", stepDistance = 1.2f, volume = 2f, echoRays = 1000, maxDistance = 5f, volForMonster = 0f };
    public MoveSettings walk = new MoveSettings { name = "Walk", stepDistance = 2.0f, volume = 5f, echoRays = 3000, maxDistance = 10f, volForMonster = 25f };
    public MoveSettings sprint = new MoveSettings { name = "Sprint", stepDistance = 3.5f, volume = 10f, echoRays = 5000, maxDistance = 20f, volForMonster = 50f };
    public MoveSettings waterCrouch = new MoveSettings { name = "Water Crouch", stepDistance = 1.2f, volume = 2f, echoRays = 1000, maxDistance = 5f, volForMonster = 0f };
    public MoveSettings waterWalk = new MoveSettings { name = "Water Walk", stepDistance = 2.0f, volume = 5f, echoRays = 3000, maxDistance = 10f, volForMonster = 25f };
    public MoveSettings waterSprint = new MoveSettings { name = "Water Sprint", stepDistance = 3.5f, volume = 10f, echoRays = 5000, maxDistance = 20f, volForMonster = 50f };

    public MoveState CurrentState = MoveState.WALK;

    private Vector3 lastPos;
    private Vector3 currentPos;
    private float distanceTraveled;
    private MoveSettings currentSettings;
    private bool isRightFoot = false; // Toggle for left/right steps

    private WaterInteraction waterInteraction; // Private water interaction for detecting if in water


    void Start()
    {
        lastPos = transform.position;
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        currentSettings = walk;

        waterInteraction = GetComponent<WaterInteraction>(); // Get the reference
    }

    void Update()
    {
        lastPos = currentPos;
        currentPos = transform.position;
        currentPos.y = 0;
        float moveDistance = Vector3.Distance(currentPos, lastPos);

        // Check for water
        if (waterInteraction.inWater)
        {
            if (CurrentState == MoveState.CROUCH) currentSettings = waterCrouch;
            else if (CurrentState == MoveState.WALK) currentSettings = waterWalk;
            else if (CurrentState == MoveState.SPRINT) currentSettings = waterSprint;
        }
        else
        {
            if (CurrentState == MoveState.CROUCH) currentSettings = crouch;
            else if (CurrentState == MoveState.WALK) currentSettings = walk;
            else if (CurrentState == MoveState.SPRINT) currentSettings = sprint;
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
    }

    void PlayStep()
    {
        // Audio - there is separate script (waterInteraction) for water sounds
        if (footstepSound != null && !waterInteraction.inWater)
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
        GlobalEchoSystem.Ping(this.gameObject, footPos, transform.forward, echoAngle, 0.3f, currentSettings.echoRays, currentSettings.maxDistance, currentSettings.volForMonster, true);
    }
}