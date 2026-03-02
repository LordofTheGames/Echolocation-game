using UnityEngine;
using UnityEngine.InputSystem;

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance;
    private Animator animator;
    private float loudness;
    private GameObject player;
    [SerializeField] private Target walk, sprint;
    [SerializeField] private GameObject look;
    [SerializeField] private Camera cam;

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
    }

    private void Update()
    {
        Loudness();
        if(animator.GetBool("Change")) animator.SetBool("Change", false);

        if(Input.anyKeyDown) animator.SetBool("Change", true);
        else if(loudness > 0 && animator.GetCurrentAnimatorStateInfo(0).IsName("mic")) animator.SetTrigger("Mic");

        if(animator.GetCurrentAnimatorStateInfo(0).IsName("end")) Destroy(gameObject);
    }

    private void Loudness()
    {
        var script = GameObject.Find("MicInput").GetComponent<MicInput>();

        if(script == null) return;

        loudness = script.loudness;
    }

    public void OnEcho(InputAction.CallbackContext context)
    {
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("clicker")) 
            animator.SetTrigger("Clicker");
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        float angle = Vector3.Dot(cam.transform.forward, look.transform.up);

        if(animator.GetCurrentAnimatorStateInfo(0).IsName("look") && angle > 0.8) 
            animator.SetTrigger("Look");
    }

    public void OnWalk()
    {
        var script = player.GetComponent<PlayerMovement>();
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("walk") && !script.GetSprint()) 
            animator.SetTrigger("Walk");
    }

    public void OnSprint()
    {
        var script = player.GetComponent<PlayerMovement>();
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("sprint") && script.GetSprint()) 
            animator.SetTrigger("Sprint");
    }

    public void OnPick(InputAction.CallbackContext context)
    {
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("pick")) 
            animator.SetTrigger("Pick");
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.started && animator.GetCurrentAnimatorStateInfo(0).IsName("throw"))
        {
            animator.SetTrigger("Throw");
        }
        else if (context.canceled && animator.GetCurrentAnimatorStateInfo(0).IsName("throw2"))
        {
            animator.SetTrigger("Throw2");
        }
            
    }

    public void OnOpen(InputAction.CallbackContext context)
    {
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("openinventory")) 
            animator.SetTrigger("Open");
    }

    public void OnChoose(InputAction.CallbackContext context)
    {
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("choosing")) 
            animator.SetTrigger("Choose");
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("select")) 
            animator.SetTrigger("Select");
    }

}
