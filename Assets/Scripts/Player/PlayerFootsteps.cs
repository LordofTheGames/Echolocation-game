using UnityEngine;

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

public class PlayerFootsteps : MonoBehaviour
    {
        [Header("Audio Settings")]
        public AudioClip footstepSound;
        public AudioSource audioSource;

        [Header("Echo Settings")]
        public float footSeparation = 1f;   // Distance from center to foot
        public float echoAngle = 360f;      // 360 for a full ripple around the foot

        [Header("Movement Profiles")]
        public MoveSettings crouch = new MoveSettings { name = "Crouch", maxSpeed = 3f, stepDistance = 1.2f, volume = 2f, echoRays = 1000, maxDistance = 5f, volForMonster = 0f };
        public MoveSettings walk = new MoveSettings { name = "Walk", maxSpeed = 6f, stepDistance = 2.0f, volume = 5f, echoRays = 3000, maxDistance = 10f, volForMonster = 25f };
        public MoveSettings sprint = new MoveSettings { name = "Sprint", maxSpeed = 12f, stepDistance = 3.5f, volume = 10f, echoRays = 5000, maxDistance = 20f, volForMonster = 50f };


        private Vector3 lastPos;
        private float distanceTraveled;
        private MoveSettings currentSettings;
        private float smoothedSpeed;
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
            // Calculate REAL distance moved this frame
            Vector3 currentPos = transform.position;
            float moveDistance = Vector3.Distance(new Vector3(currentPos.x, 0, currentPos.z), new Vector3(lastPos.x, 0, lastPos.z));
            float rawSpeed = moveDistance / Time.deltaTime;

            // Smooth speed for profile selection
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, rawSpeed, Time.deltaTime * 8f);

            // Select Profile
            if (smoothedSpeed <= crouch.maxSpeed) currentSettings = crouch;
            else if (smoothedSpeed <= walk.maxSpeed + 0.5f) currentSettings = walk;
            else currentSettings = sprint;

            // Check for water
            if (waterInteraction != null && waterInteraction.inWater)
            {
                distanceTraveled = 0f; // Reset distance so it doesn't build up while in water (stops this player footstep effect from happening in water)
            }
            else if (moveDistance > 0.001f) // Accumulate Distance/normal behaviour
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
            GlobalEchoSystem.Ping(this.gameObject, footPos, transform.forward, echoAngle, 0.3f, currentSettings.echoRays, currentSettings.maxDistance, currentSettings.volForMonster, true);
        }
    }