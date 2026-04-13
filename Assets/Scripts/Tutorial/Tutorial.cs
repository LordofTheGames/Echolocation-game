using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TutorialStates
{
    START,
    GOAL,
    MIC,
    CLICKER,
    CLICKER_COOLDOWN,
    LOOK,
    WALK,
    CROUCH,
    SPRINT,
    PICK,
    CHOOSING,
    THROW,
    THROW_2,
    EMITTER_THROW,
    SONIC_GRENADE_THROW,
    BREAK,
    HIDE,
    EXIT_HIDE,
    MONSTER,
    DIFFERENT_SURFACES,
    WATERFALL,
    CONGRATS,
    END
}

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance;
    private float micRelVolume;
    private GameObject player;
    public GameObject warpPoint;
    
    public float volume = 0.8f;
    public AudioSource audioSource;
    // [SerializeField] private bool tutorial = true;
    [SerializeField] private Target walk, sprint;
    [SerializeField] private ThrowTarget target;
    [SerializeField] private GameObject look;
    [SerializeField] private Camera cam;
    [SerializeField] private AudioClip monsterSound;
    [SerializeField] private InventoryManager inventory;

    private Dictionary<TutorialStates, GameObject> states;

    private void Awake(){
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        walk.OnCollision += OnWalk;
        sprint.OnCollision += OnSprint;
        target.OnCollision += OnTargetThrow;
        inventory.OnInventoryChanged += OnPick;
        Breakable.OnRockBroken += OnBreakTutorial;
        HideInBox.OnPlayerHide += OnHideTutorial;
        HideInBox.OnPlayerExit += OnExitTutorial;


        // animator.SetBool("Tutorial", tutorial);
        micRelVolume = GameObject.Find("MicInput").GetComponent<MicInput>().relativeVolume;

        foreach (TutorialStates state in Enum.GetValues(typeof(TutorialStates)))
        {
            states.Add(state, transform.Find("Canvas/" + state.ToString()).gameObject);
        }
    }

    private void FixedUpdate()
    {
        // if(micRelVolume > 0.1 && animator.GetCurrentAnimatorStateInfo(0).IsName("mic")) 
            // animator.SetTrigger("Change");
    }

    public void OnSkip(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // animator.SetBool("Tutorial", false);
            // tutorial = false;
            PlayerRespawn script = player.GetComponent<PlayerRespawn>();
            script.RespawnPosition = warpPoint.transform.position;
            script.Respawn();
            player.GetComponent<CrazyTimer>().StartEffect();
        }
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        // if(animator.GetCurrentAnimatorStateInfo(0).IsName("waterfall"))
            StartCoroutine(WaitBeforeStarting(5));
        // if(context.started) animator.SetTrigger("Next");
    }
    private IEnumerator WaitBeforeStarting(float seconds)
    {
            yield return new WaitForSeconds(seconds);
            PlayerRespawn script = player.GetComponent<PlayerRespawn>();
            script.RespawnPosition = warpPoint.transform.position;
            script.Respawn();
            player.GetComponent<CrazyTimer>().StartEffect();
    }

    public void OnEcho(InputAction.CallbackContext context)
    {
        // if(animator.GetCurrentAnimatorStateInfo(0).IsName("clicker")) 
        //     animator.SetTrigger("Change");
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        float angle = Vector3.Dot(cam.transform.forward, look.transform.up);

        // if(animator.GetCurrentAnimatorStateInfo(0).IsName("look") && angle > 0.85) 
        //     animator.SetTrigger("Change");
    }

    public void OnWalk()
    {
        var script = player.GetComponent<PlayerMovement>();
        // if (animator.GetCurrentAnimatorStateInfo(0).IsName("walk") && !script.GetSprint()) 
        //     animator.SetTrigger("Change");
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        var script = player.GetComponent<PlayerMovement>();

        //switch when you uncrouch
        // if(context.performed && animator.GetCurrentAnimatorStateInfo(0).IsName("crouch") && !script.GetCrouch()) 
        //     animator.SetTrigger("Change");
    }

    public void OnSprint()
    {
        var script = player.GetComponent<PlayerMovement>();
        // if(animator.GetCurrentAnimatorStateInfo(0).IsName("sprint") && script.GetSprint()) 
        //     animator.SetTrigger("Change");
    }

    public void OnPick()
    {
        // if(animator.GetCurrentAnimatorStateInfo(0).IsName("pick")) 
        //     animator.SetTrigger("Change");
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        // if (context.started && animator.GetCurrentAnimatorStateInfo(0).IsName("throw"))
        //     animator.SetTrigger("Change");
    }

    public void OnTargetThrow(string hitTag)
    {
        // if(animator.GetCurrentAnimatorStateInfo(0).IsName("throw2") && hitTag == "Throwable Rock") 
        // {
        //     animator.SetTrigger("Change");
        // }
        // else if (animator.GetCurrentAnimatorStateInfo(0).IsName("emitter throw") && hitTag == "Throwable Emitter")
        // {
        //     animator.SetTrigger("Change");
        // }
        // else if (animator.GetCurrentAnimatorStateInfo(0).IsName("sonic grenade throw") && hitTag == "Throwable Grenade")
        // {
        //     animator.SetTrigger("Change");
        // }
    }

    // public void OnOpen(InputAction.CallbackContext context)
    // {
    //     if(animator.GetCurrentAnimatorStateInfo(0).IsName("openinventory")) 
    //         animator.SetTrigger("Change");
    // }

    public void OnChoose(InputAction.CallbackContext context)
    {
        // if(animator.GetCurrentAnimatorStateInfo(0).IsName("choosing")) 
        //     animator.SetTrigger("Change");
    }

    // public void OnSelect(InputAction.CallbackContext context)
    // {
    //     if(animator.GetCurrentAnimatorStateInfo(0).IsName("select")) 
    //         animator.SetTrigger("Change");
    // }


    public void OnBreakTutorial()
    {
        // if (animator.GetCurrentAnimatorStateInfo(0).IsName("break"))
        //     animator.SetTrigger("Change");
    }

    public void OnHideTutorial()
    {
        // if (animator.GetCurrentAnimatorStateInfo(0).IsName("hide"))
        //     animator.SetTrigger("Change");
    }

    public void OnExitTutorial()
    {
        // if (animator.GetCurrentAnimatorStateInfo(0).IsName("exit hide"))
        //     animator.SetTrigger("Change");
    }

    public void PlayMonsterSound()
    {
        audioSource.PlayOneShot(monsterSound, volume);
    }

}
