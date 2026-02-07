using UnityEngine;

public enum WaterRippleType
{
    Footsteps,
    Wade
}

public class WaterInteraction : MonoBehaviour
{
    public WaterRippleType waterRippleType = WaterRippleType.Footsteps;
    public ParticleSystem ripple;
    public float stepDistance = 0.5f;
    public float footSpacing = 0.3f;
    public float forwardOffset = 0.1f; 

    public float rippleSize = 1f;
    public float rippleLifetime = 2f;

    private CharacterController cc;
    private Vector3 lastPos;
    private float distanceTraveled;
    private bool inWater;
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
        if (waterRippleType == WaterRippleType.Footsteps)
        {
            lastPos = transform.position;
        }
        else if (waterRippleType == WaterRippleType.Wade)
        {
            playerPos = transform.position;
        }
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
            ripple.transform.position = transform.position;

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
            if (ripple.gameObject.activeSelf != inWater) 
                ripple.gameObject.SetActive(inWater);
        }
        else if (waterRippleType == WaterRippleType.Wade)
        {
            // Toggle ripple object based on water status
            if (inWater) ripple.gameObject.SetActive(true);
            else ripple.gameObject.SetActive(false);
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

        Vector3 sideOffset = transform.right * (footSpacing * 0.5f) * sideDir;
        Vector3 fwdOffset = transform.forward * forwardOffset;
        Vector3 spawnPos = triggerPos + sideOffset + fwdOffset;
        
        if (waterHit.collider != null) spawnPos.y = waterHit.point.y + 0.01f;

        var emitParams = new ParticleSystem.EmitParams
        {
            position = spawnPos,
            startSize = rippleSize,
            startLifetime = rippleLifetime,
            startColor = Color.white
        };

        ripple.Emit(emitParams, 1);
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
            Vector3 forward = ripple.transform.eulerAngles;
            forward.y = Start;
            ripple.transform.eulerAngles = forward;

            for (int i = Start; i < End; i += Delta)
            {
                var emitParams = new ParticleSystem.EmitParams
                {
                    position = transform.position + ripple.transform.forward * 1.15f,
                    velocity = ripple.transform.forward * Speed,
                    startSize = Size,
                    startLifetime = Lifetime,
                    startColor = Color.white
                };
                ripple.Emit(emitParams, 1);

                ripple.transform.Rotate(Vector3.up * Delta, Space.World);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (waterRippleType == WaterRippleType.Wade){
            if (((1 << other.gameObject.layer) & waterLayer) != 0)
            {
                var emitParams = new ParticleSystem.EmitParams
                {
                    position = transform.position,
                    velocity = Vector3.zero,
                    startSize = 5,
                    startLifetime = 0.1f,
                    startColor = Color.white
                };
                ripple.Emit(emitParams, 1);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (waterRippleType == WaterRippleType.Wade){
            if (((1 << other.gameObject.layer) & waterLayer) != 0)
            {
                var emitParams = new ParticleSystem.EmitParams
                {
                    position = transform.position,
                    velocity = Vector3.zero,
                    startSize = 5,
                    startLifetime = 0.1f,
                    startColor = Color.white
                };
                ripple.Emit(emitParams, 1);
            }
        }
    }
}