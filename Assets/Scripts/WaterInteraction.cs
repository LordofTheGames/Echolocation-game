using UnityEngine;

public class WaterInteraction : MonoBehaviour
{
    public ParticleSystem ripple;
    public LayerMask waterLayer;
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

    void Start()
    {
        cc = GetComponent<CharacterController>();
        lastPos = transform.position;
    }

    void Update()
    {
        CheckWater();
        Vector3 currentPos = transform.position;
        float moveStep = Vector3.Distance(new Vector3(currentPos.x, 0, currentPos.z), new Vector3(lastPos.x, 0, lastPos.z));

        if (cc.isGrounded && inWater)
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

    void CheckWater()
    {
        float startHeight = cc.height * 0.5f;
        inWater = Physics.Raycast(transform.position + Vector3.up * startHeight, Vector3.down, out waterHit, cc.height * 2.5f, waterLayer, QueryTriggerInteraction.Collide);
        
        if (ripple.gameObject.activeSelf != inWater) 
            ripple.gameObject.SetActive(inWater);
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
}