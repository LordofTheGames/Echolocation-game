using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance;
    private Animator animator;
    private float loudness;
    private GameObject player;
    
    public float volume = 0.8f;
    public AudioSource audioSource;
    [SerializeField] private bool tutorial = true;
    [SerializeField] private Target walk, sprint;
    [SerializeField] private ThrowTarget target;
    [SerializeField] private GameObject look;
    [SerializeField] private Camera cam;
    [SerializeField] private AudioClip monsterSound;
    [SerializeField] private InventoryManager inventory;

    private void Awake(){
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");

        walk.OnCollision += OnWalk;
        sprint.OnCollision += OnSprint;
        target.OnCollision += OnTargetThrow;
        inventory.OnInventoryChanged += OnPick;

        animator.SetBool("Tutorial", tutorial);
    }

    private void FixedUpdate()
    {
        Loudness();

        if(loudness > 0.1 && animator.GetCurrentAnimatorStateInfo(0).IsName("mic")) {
            animator.SetTrigger("Change");
        }

        if(animator.GetBool("Next")) animator.SetBool("Next", false);
    }

    private void Loudness()
    {
        var script = GameObject.Find("MicInput").GetComponent<MicInput>();

        if(script == null) return;

        loudness = script.loudness;
    }

    public void OnSkip(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetBool("Tutorial", false);
            tutorial = false;
            Debug.Log("Skip");
        }
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        
        if(context.performed) animator.SetBool("Next", true);
    }

    public void OnEcho(InputAction.CallbackContext context)
    {
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("clicker")) 
            animator.SetTrigger("Change");
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        float angle = Vector3.Dot(cam.transform.forward, look.transform.up);

        if(animator.GetCurrentAnimatorStateInfo(0).IsName("look") && angle > 0.85) 
            animator.SetTrigger("Change");
    }

    public void OnWalk()
    {
        var script = player.GetComponent<PlayerMovement>();
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("walk") && !script.GetSprint()) 
            animator.SetTrigger("Change");
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        var script = player.GetComponent<PlayerMovement>();

        //switch when you uncrouch
        if(context.performed && animator.GetCurrentAnimatorStateInfo(0).IsName("crouch") && !script.GetCrouch()) 
            animator.SetTrigger("Change");
    }

    public void OnSprint()
    {
        var script = player.GetComponent<PlayerMovement>();
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("sprint") && script.GetSprint()) 
            animator.SetTrigger("Change");
    }

    public void OnPick()
    {
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("pick")) 
            animator.SetTrigger("Change");
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.started && animator.GetCurrentAnimatorStateInfo(0).IsName("throw"))
        {
            animator.SetTrigger("Change");
        }
    }

    public void OnTargetThrow()
    {
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("throw2")) {
            animator.SetTrigger("Change");
            audioSource.PlayOneShot(monsterSound, volume);
        }
    }

    // public void OnOpen(InputAction.CallbackContext context)
    // {
    //     if(animator.GetCurrentAnimatorStateInfo(0).IsName("openinventory")) 
    //         animator.SetTrigger("Change");
    // }

    public void OnChoose(InputAction.CallbackContext context)
    {
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("choosing")) 
            animator.SetTrigger("Change");
    }

    // public void OnSelect(InputAction.CallbackContext context)
    // {
    //     if(animator.GetCurrentAnimatorStateInfo(0).IsName("select")) 
    //         animator.SetTrigger("Change");
    // }

}
