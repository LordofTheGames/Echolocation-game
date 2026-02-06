using UnityEngine;

public class WaterInteraction : MonoBehaviour
{
    public ParticleSystem ripple;
    public LayerMask waterLayer;
    public float stepDistance;
    public float footSpacing; 

    public float rippleSize;
    public float rippleLifetime;

    private CharacterController cc;
    private Vector3 lastPos;
    private float distanceTraveled;
    private bool inWater;
    private bool wasGrounded;
    private bool isRightFoot;
    private RaycastHit waterHit;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 currentPos = transform.position;
        float moveStep = Vector3.Distance(new Vector3(currentPos.x, 0, currentPos.z), new Vector3(lastPos.x, 0, lastPos.z));
        lastPos = currentPos;

        CheckWater();
        if (!wasGrounded && cc.isGrounded && inWater)
        {
            CreateFootstep(); 
            distanceTraveled = 0;
        }

        if (cc.isGrounded && inWater)
        {
            if (moveStep > 0.001f) 
            {
                distanceTraveled += moveStep;
                if (distanceTraveled >= stepDistance)
                {
                    CreateFootstep();
                    distanceTraveled = 0; 
                }
            }
            else
            {
                distanceTraveled = stepDistance;
            }
        }
        else
        {
            distanceTraveled = stepDistance;
        }
        wasGrounded = cc.isGrounded;
        Shader.SetGlobalVector("_Player", transform.position);
    }

    void CheckWater()
    {
        float startHeight = cc.height * 0.5f;
        inWater = Physics.Raycast(transform.position + Vector3.up * startHeight, Vector3.down, out waterHit, cc.height * 2.5f, waterLayer, QueryTriggerInteraction.Collide);
        
        if (ripple.gameObject.activeSelf != inWater) 
            ripple.gameObject.SetActive(inWater);
    }

    void CreateFootstep()
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

        Vector3 offset = transform.right * (footSpacing * 0.5f) * sideDir;
        Vector3 spawnPos = transform.position + offset;
        
        if (waterHit.collider != null) spawnPos.y = waterHit.point.y;

        var emitParams = new ParticleSystem.EmitParams
        {
            position = spawnPos,
            velocity = Vector3.zero,
            startSize = rippleSize,
            startLifetime = rippleLifetime,
            startColor = Color.white,
            rotation3D = Vector3.zero
        };

        ripple.Emit(emitParams, 1);
        isRightFoot = !isRightFoot;
    }
}