using UnityEngine;

public enum SurfaceType
{
    Default,
    Water,
    Metal,
    Wood,
    Dirt,
    Lab
}

[System.Serializable]
public struct MoveSettings
{
    public string name;
    public float stepDistance;
    [Range(0f, 1f)] public float volume;
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
    public AudioClip waterFootstep;
    public AudioClip metalFootstep;
    public AudioClip woodFootstep;
    public AudioClip dirtFootstep;
    public AudioClip labFootstep;

    [Header("Echo Settings")]
    public float footSeparation = 1f;   // Distance from center to foot
    public float echoAngle = 360f;      // 360 for a full ripple around the foot

    [Header("Raycast Settings (check surface type)")]
    public LayerMask groundLayer;

    [Header("Movement - Default")]
    public MoveSettings crouch = new MoveSettings { name = "Crouch", stepDistance = 2f, volume = 0.2f, echoRays = 700, maxDistance = 6f, volForMonster = 0f };
    public MoveSettings walk = new MoveSettings { name = "Walk", stepDistance = 4f, volume = 0.5f, echoRays = 2000, maxDistance = 12f, volForMonster = 25f };
    public MoveSettings sprint = new MoveSettings { name = "Sprint", stepDistance = 5f, volume = 1f, echoRays = 5000, maxDistance = 30f, volForMonster = 50f };
    
    [Header("Movement - Water")]
    public MoveSettings waterCrouch = new MoveSettings { name = "Water Crouch", stepDistance = 2f, volume = 0.07f, echoRays = 1000, maxDistance = 8f, volForMonster = 10f };
    public MoveSettings waterWalk = new MoveSettings { name = "Water Walk", stepDistance = 4f, volume = 0.15f, echoRays = 2700, maxDistance = 15f, volForMonster = 35f };
    public MoveSettings waterSprint = new MoveSettings { name = "Water Sprint", stepDistance = 5f, volume = 0.35f, echoRays = 7000, maxDistance = 35f, volForMonster = 70f };

    [Header("Movement - Metal")]
    public MoveSettings metalCrouch = new MoveSettings { name = "Metal Crouch", stepDistance = 2f, volume = 0.2f, echoRays = 700, maxDistance = 6f, volForMonster = 0f };
    public MoveSettings metalWalk = new MoveSettings { name = "Metal Walk", stepDistance = 4f, volume = 0.5f, echoRays = 2000, maxDistance = 12f, volForMonster = 25f };
    public MoveSettings metalSprint = new MoveSettings { name = "Metal Sprint", stepDistance = 5f, volume = 1f, echoRays = 5000, maxDistance = 30f, volForMonster = 50f };

    [Header("Movement - Wood")]
    public MoveSettings woodCrouch = new MoveSettings { name = "Wood Crouch", stepDistance = 2f, volume = 0.2f, echoRays = 700, maxDistance = 6f, volForMonster = 0f };
    public MoveSettings woodWalk = new MoveSettings { name = "Wood Walk", stepDistance = 4f, volume = 0.5f, echoRays = 2000, maxDistance = 12f, volForMonster = 25f };
    public MoveSettings woodSprint = new MoveSettings { name = "Wood Sprint", stepDistance = 5f, volume = 1f, echoRays = 5000, maxDistance = 30f, volForMonster = 50f };

    [Header("Movement - Dirt")]
    public MoveSettings dirtCrouch = new MoveSettings { name = "Dirt Crouch", stepDistance = 2f, volume = 0.2f, echoRays = 700, maxDistance = 6f, volForMonster = 0f };
    public MoveSettings dirtWalk = new MoveSettings { name = "Dirt Walk", stepDistance = 4f, volume = 0.5f, echoRays = 2000, maxDistance = 12f, volForMonster = 25f };
    public MoveSettings dirtSprint = new MoveSettings { name = "Dirt Sprint", stepDistance = 5f, volume = 1f, echoRays = 5000, maxDistance = 30f, volForMonster = 50f };

    [Header("Movement - Lab")]
    public MoveSettings labCrouch = new MoveSettings { name = "Lab Crouch", stepDistance = 2f, volume = 0.2f, echoRays = 700, maxDistance = 6f, volForMonster = 0f };
    public MoveSettings labWalk = new MoveSettings { name = "Lab Walk", stepDistance = 4f, volume = 0.5f, echoRays = 2000, maxDistance = 12f, volForMonster = 25f };
    public MoveSettings labSprint = new MoveSettings { name = "Lab Sprint", stepDistance = 5f, volume = 1f, echoRays = 5000, maxDistance = 30f, volForMonster = 50f };

    private Vector3 lastPos;
    private Vector3 currentPos;
    private float distanceTraveled;
    private MoveSettings currentSettings;
    private bool isRightFoot = false;
    [SerializeField] private float teleportDistanceThreshold = 3f;

    void Start()
    {
        currentPos = transform.position;
        currentPos.y = 0;
        lastPos = currentPos;
        
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        currentSettings = walk;
    }

    void Update()
    {
        CheckSurface();

        lastPos = currentPos;
        currentPos = transform.position;
        currentPos.y = 0;
        float moveDistance = Vector3.Distance(currentPos, lastPos);

        if (moveDistance > teleportDistanceThreshold)
        {
            distanceTraveled = 0f;
            return;
        }

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

    private void CheckSurface()
    {
        // In Scripts/Water/DetectPlayer.cs, surface type is set to water when player enters water, and set back to previous value when player leaves
        if (currentSurface == SurfaceType.Water) return;

        Vector3 rayStart = transform.position + (Vector3.up * 0.5f); // Start slightly above player's pivot

        // Cast a ray from slightly above the bottom of the player, shooting downwards
        // The 3f distance ensures it reaches the ground even if the player is bouncing/hovering slightly
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 3f, groundLayer))
        {
            string surfaceTag = hit.collider.tag;

            switch (surfaceTag)
            {
                case "Metal":
                    currentSurface = SurfaceType.Metal;
                    break;
                case "Wood":
                    currentSurface = SurfaceType.Wood;
                    break;
                case "Dirt":
                    currentSurface = SurfaceType.Dirt;
                    break;
                case "Lab":
                    currentSurface = SurfaceType.Lab;
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
                if (CurrentState == MoveState.CROUCH) currentSettings = metalCrouch;
                else if (CurrentState == MoveState.WALK) currentSettings = metalWalk;
                else if (CurrentState == MoveState.SPRINT) currentSettings = metalSprint;
                break;
            case SurfaceType.Wood:
                if (CurrentState == MoveState.CROUCH) currentSettings = woodCrouch;
                else if (CurrentState == MoveState.WALK) currentSettings = woodWalk;
                else if (CurrentState == MoveState.SPRINT) currentSettings = woodSprint;
                break;
            case SurfaceType.Dirt:
                if (CurrentState == MoveState.CROUCH) currentSettings = dirtCrouch;
                else if (CurrentState == MoveState.WALK) currentSettings = dirtWalk;
                else if (CurrentState == MoveState.SPRINT) currentSettings = dirtSprint;
                break;
            case SurfaceType.Lab:
                if (CurrentState == MoveState.CROUCH) currentSettings = labCrouch;
                else if (CurrentState == MoveState.WALK) currentSettings = labWalk;
                else if (CurrentState == MoveState.SPRINT) currentSettings = labSprint;
                break;
            default:
                if (CurrentState == MoveState.CROUCH) currentSettings = crouch;
                else if (CurrentState == MoveState.WALK) currentSettings = walk;
                else if (CurrentState == MoveState.SPRINT) currentSettings = sprint;
                break;
        }
    }

    void PlayStep()
    {
        AudioClip clipToPlay;
        switch (currentSurface)
        {
            case SurfaceType.Water:
                clipToPlay = waterFootstep;
                break;
            case SurfaceType.Dirt:
                clipToPlay = dirtFootstep;
                break;
            case SurfaceType.Wood:
                clipToPlay = woodFootstep;
                break;
            case SurfaceType.Metal:
                clipToPlay = metalFootstep;
                break;
            case SurfaceType.Lab:
                clipToPlay = labFootstep;
                break;
            default:
                clipToPlay = defaultFootstep;
                break;
        }

        audioSource.pitch = Random.Range(0.92f, 1.08f);
        audioSource.PlayOneShot(clipToPlay, currentSettings.volume);

        // Calculate Foot Position
        isRightFoot = !isRightFoot; 
        float dirMultiplier = isRightFoot ? 1f : -1f;
        Vector3 footPos = transform.position + (transform.right * footSeparation * dirMultiplier);
        footPos.y = transform.position.y;

        // Calculate visual volume based on old max distance and loss per meter (2)
        float visualVolume = currentSettings.maxDistance * 2f;
        // Trigger Echo
        GlobalEchoSystem.Ping(this.gameObject, footPos, transform.forward, echoAngle, 0.3f, currentSettings.echoRays, visualVolume, currentSettings.volForMonster, true);
    }
}