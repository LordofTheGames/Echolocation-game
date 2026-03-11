using UnityEngine;

public enum SurfaceType
{
    Default,
    Water,
    Metal,
    Wood,
    Dirt
}

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
    [Header("Current State")]
    public SurfaceType currentSurface = SurfaceType.Default;
    public MoveState CurrentState = MoveState.WALK;

    [Header("Audio Component")]
    public AudioSource audioSource;

    [Header("Audio Clips by Surface")]
    public AudioClip defaultFootstep;
    public AudioClip metalFootstep;
    public AudioClip woodFootstep;
    public AudioClip dirtFootstep;

    [Header("Echo Settings")]
    public float footSeparation = 1f;   // Distance from center to foot
    public float echoAngle = 360f;      // 360 for a full ripple around the foot

    [Header("Movement - Default")]
    public MoveSettings crouch = new MoveSettings { name = "Crouch", stepDistance = 1.2f, volume = 2f, echoRays = 1000, maxDistance = 5f, volForMonster = 0f };
    public MoveSettings walk = new MoveSettings { name = "Walk", stepDistance = 2.0f, volume = 5f, echoRays = 3000, maxDistance = 10f, volForMonster = 25f };
    public MoveSettings sprint = new MoveSettings { name = "Sprint", stepDistance = 3.5f, volume = 10f, echoRays = 5000, maxDistance = 20f, volForMonster = 50f };
    
    [Header("Movement - Water")]
    public MoveSettings waterCrouch = new MoveSettings { name = "Water Crouch", stepDistance = 1.2f, volume = 2f, echoRays = 1000, maxDistance = 5f, volForMonster = 0f };
    public MoveSettings waterWalk = new MoveSettings { name = "Water Walk", stepDistance = 2.0f, volume = 5f, echoRays = 3000, maxDistance = 10f, volForMonster = 25f };
    public MoveSettings waterSprint = new MoveSettings { name = "Water Sprint", stepDistance = 3.5f, volume = 10f, echoRays = 5000, maxDistance = 20f, volForMonster = 50f };

    private Vector3 lastPos;
    private Vector3 currentPos;
    private float distanceTraveled;
    private MoveSettings currentSettings;
    private bool isRightFoot = false;

    private WaterInteraction waterInteraction;

    void Start()
    {
        currentPos = transform.position;
        currentPos.y = 0;
        lastPos = currentPos;
        
        waterInteraction = GetComponent<WaterInteraction>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        currentSettings = walk;
    }

    void Update()
    {
        lastPos = currentPos;
        currentPos = transform.position;
        currentPos.y = 0;
        float moveDistance = Vector3.Distance(currentPos, lastPos);

        if (waterInteraction.inWater) currentSurface = SurfaceType.Water;

        UpdateCurrentSettings();

        if (moveDistance > 0.001f) 
        {
            distanceTraveled += moveDistance;
            if (distanceTraveled >= currentSettings.stepDistance)
            {
                PlayStep();
                distanceTraveled -= currentSettings.stepDistance;
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y > 0.5f) // better than using layermask.all for checking if we're on the ground
        {
            string surfaceTag = hit.collider.tag;

            switch (surfaceTag)
            {
                // no water as that is handled by WaterInteraction
                case "Metal":
                    currentSurface = SurfaceType.Metal;
                    break;
                case "Wood":
                    currentSurface = SurfaceType.Wood;
                    break;
                case "Dirt":
                    currentSurface = SurfaceType.Dirt;
                    break;
                default:
                    currentSurface = SurfaceType.Default;
                    break;
            }
        }
    }

    private void UpdateCurrentSettings()
    {
        switch (currentSurface)
        {
            case SurfaceType.Water:
                if (CurrentState == MoveState.CROUCH) currentSettings = waterCrouch;
                else if (CurrentState == MoveState.WALK) currentSettings = waterWalk;
                else if (CurrentState == MoveState.SPRINT) currentSettings = waterSprint;
                break;

            case SurfaceType.Metal:
            case SurfaceType.Wood:
            case SurfaceType.Dirt:
            case SurfaceType.Default:
            default:
                if (CurrentState == MoveState.CROUCH) currentSettings = crouch;
                else if (CurrentState == MoveState.WALK) currentSettings = walk;
                else if (CurrentState == MoveState.SPRINT) currentSettings = sprint;
                break;
        }
    }

    void PlayStep()
    {
        if (currentSurface != SurfaceType.Water)
        {
            AudioClip clipToPlay = defaultFootstep;  
            switch (currentSurface)
            {
                case SurfaceType.Dirt:
                    clipToPlay = dirtFootstep;
                    break;
                case SurfaceType.Wood:
                    clipToPlay = woodFootstep;
                    break;
                case SurfaceType.Metal:
                    clipToPlay = metalFootstep;
                    break;
                case SurfaceType.Default:
                default:
                    clipToPlay = defaultFootstep;
                    break;
            }

            if (clipToPlay != null)
            {
                audioSource.pitch = Random.Range(0.92f, 1.08f);
                audioSource.PlayOneShot(clipToPlay, currentSettings.volume);
            }
        }

        // Calculate Foot Position
        isRightFoot = !isRightFoot; 
        float dirMultiplier = isRightFoot ? 1f : -1f;
        Vector3 footPos = transform.position + (transform.right * footSeparation * dirMultiplier);
        footPos.y = transform.position.y;

        // Trigger Echo
        GlobalEchoSystem.Ping(this.gameObject, footPos, transform.forward, echoAngle, 0.3f, currentSettings.echoRays, currentSettings.maxDistance, currentSettings.volForMonster, true);
    }
}