using UnityEngine;

public class WaterInteraction : MonoBehaviour
{
    public ParticleSystem ripple;
    
    [Header("Settings")]
    public LayerMask waterLayer = LayerMask.GetMask("Water");
    public LayerMask groundLayer = LayerMask.GetMask("Ground");

    private CharacterController cc;
    private Vector3 playerPos;
    private float velocityXZ;
    private bool inWater;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        playerPos = transform.position;
    }

    void Update()
    {
        // Calculate Velocity (Exactly like your original script)
        velocityXZ = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(playerPos.x, 0, playerPos.z));
        playerPos = transform.position;
        ripple.transform.position = transform.position;

        // Global Shader Variable
        Shader.SetGlobalVector("_Player", transform.position);

        HandleWaterDetection();
        HandleRipples();
    }

    void HandleWaterDetection()
    {
        // Water Check (from your original PlayerMovement)
        inWater = playerIsInWater();
        // Toggle ripple object based on water status
        if (inWater) ripple.gameObject.SetActive(true);
        else ripple.gameObject.SetActive(false);
    }

    bool playerIsInWater(){
        float height = cc.height + cc.radius;
        return Physics.Raycast(transform.position + Vector3.up * height, Vector3.down, height * 2, waterLayer);
    }

    void HandleRipples()
    {
        // Re-implementing your OnTriggerStay logic here for movement-based ripples
        if (inWater && velocityXZ > 0.025f && Time.renderedFrameCount % 3 == 0)
        {
            int y = (int)transform.eulerAngles.y;
            CreateRipple(y - 100, y + 100, 3, 5f, 2.65f, 3f);
        }
    }

    // Your exact CreateRipple method
    void CreateRipple(int Start, int End, int Delta, float Speed, float Size, float Lifetime)
    {
        Vector3 forward = ripple.transform.eulerAngles;
        forward.y = Start;
        ripple.transform.eulerAngles = forward;

        for (int i = Start; i < End; i += Delta)
        {
            ripple.Emit(transform.position + ripple.transform.forward * 1.15f, ripple.transform.forward * Speed, Size, Lifetime, Color.white);
            ripple.transform.Rotate(Vector3.up * Delta, Space.World);
        }
    }

    // Trigger logic (Exactly like your original script)
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & waterLayer) != 0)
        {
            ripple.Emit(transform.position, Vector3.zero, 5, 0.1f, Color.white);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & waterLayer) != 0)
        {
            ripple.Emit(transform.position, Vector3.zero, 5, 0.1f, Color.white);
        }
    }
}