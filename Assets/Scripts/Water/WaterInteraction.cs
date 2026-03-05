using UnityEngine;

public enum WaterRippleType
{
    Footsteps,
    Wade
}

public class WaterInteraction : MonoBehaviour
{
    public WaterRippleType waterRippleType = WaterRippleType.Footsteps;
    public ParticleSystem StepsRipplePrefab;
    public ParticleSystem WadeRipplePrefab;
    public float stepDistance = 3f;
    public float footSpacing = 1f;
    public float forwardOffset = 2.3f; 

    public float rippleSize = 1.5f;
    public float rippleLifetime = 0.5f;

    [Header("Audio Settings")]
    public AudioClip footstepWaterSound; 
    public float volume = 0.8f;

    [Header("Movement Profiles")]
    public MoveSettings crouch = new MoveSettings { name = "Crouch", maxSpeed = 3f, stepDistance = 1.2f, volume = 2f, echoRays = 1000, maxDistance = 5f, volForMonster = 0f };
    public MoveSettings walk = new MoveSettings { name = "Walk", maxSpeed = 6f, stepDistance = 2.0f, volume = 5f, echoRays = 3000, maxDistance = 10f, volForMonster = 25f };
    public MoveSettings sprint = new MoveSettings { name = "Sprint", maxSpeed = 12f, stepDistance = 3.5f, volume = 10f, echoRays = 5000, maxDistance = 20f, volForMonster = 50f };
    public float echoAngle = 360f;      // 360 for a full ripple around the foot
    private MoveSettings currentSettings;
    private float smoothedSpeed; 

    private ParticleSystem StepsRipple;
    private ParticleSystem WadeRipple;
    private CharacterController cc;
    private Vector3 lastPos;
    private float distanceTraveled;
    public bool inWater;
    private bool isRightFoot;
    private RaycastHit waterHit;
    private LayerMask waterLayer;

    // ------ Variables for Wade ripple type only
    private Vector3 playerPos;
    private float velocityXZ;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        waterLayer = LayerMask.GetMask("Water");
        lastPos = transform.position;
        playerPos = transform.position;
        WadeRipple = Instantiate(WadeRipplePrefab);
        StepsRipple = Instantiate(StepsRipplePrefab);
        currentSettings = walk;
    }

    void Update()
    {
        if (waterRippleType == WaterRippleType.Footsteps)
        {
            CheckWater();
            Vector3 currentPos = transform.position;
            float moveStep = Vector3.Distance(new Vector3(currentPos.x, 0, currentPos.z), new Vector3(lastPos.x, 0, lastPos.z));

            if (inWater)
            {
                if (moveStep > 0.001f)
                {
                    distanceTraveled += moveStep;

                    if (distanceTraveled >= stepDistance)
                    {
                        float overflow = distanceTraveled - stepDistance;
                        float ratio = 1.0f - (overflow / moveStep);
                        Vector3 exactStepPos = Vector3.Lerp(lastPos, currentPos, ratio);

                        CreateFootstep(exactStepPos);
                        distanceTraveled = 0;
                    }
                }
            }
            else
            {
                distanceTraveled = 0;
            }

            lastPos = currentPos;
            Shader.SetGlobalVector("_Player", transform.position);
        }
        else if (waterRippleType == WaterRippleType.Wade)
        {
            // Calculate Velocity (Exactly like your original script)
            velocityXZ = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(playerPos.x, 0, playerPos.z));
            playerPos = transform.position;
            WadeRipple.transform.position = transform.position;

            // Global Shader Variable
            Shader.SetGlobalVector("_Player", transform.position);

            CheckWater();
            HandleWadeRipples();
        }
    }

    void CheckWater()
    {
        inWater = playerIsInWater();
        if (waterRippleType == WaterRippleType.Footsteps){
            if (StepsRipple.gameObject.activeSelf != inWater) 
                StepsRipple.gameObject.SetActive(inWater);
        }
        else if (waterRippleType == WaterRippleType.Wade)
        {
            // Toggle ripple object based on water status
            if (inWater) WadeRipple.gameObject.SetActive(true);
            else WadeRipple.gameObject.SetActive(false);
        }
    }

    bool playerIsInWater(){
        float height = cc.height + cc.radius;
        return Physics.Raycast(transform.position + Vector3.up * height, Vector3.down, height * 2, waterLayer);
    }

    void CreateFootstep(Vector3 triggerPos)
    {
        float sideDir;
        if (isRightFoot)
        {
            sideDir = 1f; 
        }
        else
        {
            sideDir = -1f; 
        }

        Vector3 position = transform.position;
        Vector3 currentPos = new Vector3(position.x, 0, position.z);
        Vector3 prevPos = new Vector3(lastPos.x, 0, lastPos.z);
        Vector3 diff = currentPos - prevPos;
        Vector3 fwdOffset = Vector3.Normalize(diff) * forwardOffset;

        Vector3 sideOffset = transform.right * (footSpacing * 0.5f) * sideDir;
        Vector3 spawnPos = triggerPos + sideOffset + fwdOffset;
        
        if (waterHit.collider != null) spawnPos.y = waterHit.point.y + 0.01f;

        var emitParams = new ParticleSystem.EmitParams
        {
            position = spawnPos,
            startSize = rippleSize,
            startLifetime = rippleLifetime,
            startColor = Color.white
        };

        StepsRipple.Emit(emitParams, 1);

        if (footstepWaterSound != null)
        {
            AudioSource.PlayClipAtPoint(footstepWaterSound, spawnPos, volume);
        }
        isRightFoot = !isRightFoot;
    }

    void HandleWadeRipples()
    {
        if (waterRippleType == WaterRippleType.Wade){
            if (inWater && velocityXZ > 0.025f && Time.renderedFrameCount % 3 == 0)
            {
                int y = (int)transform.eulerAngles.y;
                CreateRipple(y - 100, y + 100, 3, 5f, 2.65f, 3f);
            }
        }
    }

    void CreateRipple(int Start, int End, int Delta, float Speed, float Size, float Lifetime)
    {
        if (waterRippleType == WaterRippleType.Wade){
            Vector3 forward = WadeRipple.transform.eulerAngles;
            forward.y = Start;
            WadeRipple.transform.eulerAngles = forward;

            for (int i = Start; i < End; i += Delta)
            {
                WadeRipple.Emit(transform.position + WadeRipple.transform.forward * 1.15f, WadeRipple.transform.forward * Speed, Size, Lifetime, Color.white);
                WadeRipple.transform.Rotate(Vector3.up * Delta, Space.World);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (waterRippleType == WaterRippleType.Wade){
            if (((1 << other.gameObject.layer) & waterLayer) != 0)
            {
                WadeRipple.Emit(transform.position, Vector3.zero, 5, 0.1f, Color.white);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (waterRippleType == WaterRippleType.Wade){
            if (((1 << other.gameObject.layer) & waterLayer) != 0)
            {
                WadeRipple.Emit(transform.position, Vector3.zero, 5, 0.1f, Color.white);
            }
        }
    }
}