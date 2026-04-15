using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Reflection;

public enum TutorialStates
{
    // THE ORDER OF THESE IS IMPORTANT:
    // It is the order that the prompts will be displayed in!
    // (when nextState is called, it will set the next state to be the next one on the list)
    // each enum value should have a corresponding GameObject (containing the text etc.) with EXACTLY the same name
    START,
    GOAL,
    MIC,
    WARNING,
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
    CONGRATS
}

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance;
    
    public AudioSource audioSource;
    public AudioClip monsterSound;
    public float monsterSoundVolume = 0.8f;

    public GameObject lookTarget, walkTarget, sprintTarget, throwTarget;
    public GameObject look;
    public Camera cam;
    public InventoryManager inventory;
    public GameObject spawnPointAfterExit;

    public Image micFillBar;

    private GameObject player;
    private MicInput micInput;
    private Dictionary<TutorialStates, GameObject> states;
    private Dictionary<TutorialStates, List<GameObject>> stateObjects;
    private TutorialStates currentState;
    private bool hasFinishedTutorial = false;

    private float micHoldTimer = 0f;
    private float requiredMicTime = 1.5f;

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
        micInput = GameObject.Find("MicInput").GetComponent<MicInput>();

        // register functions to be called when certain events are triggered (event listeners)
        // for example, OnWalk is called when walkTarget's Target script triggers its OnCollision event
        walkTarget.GetComponent<Target>().OnCollision += OnWalk;
        sprintTarget.GetComponent<Target>().OnCollision += OnSprint;
        throwTarget.GetComponentInChildren<ThrowTarget>().OnCollision += OnTargetThrow;
        inventory.OnInventoryChanged += OnPick;
        Breakable.OnRockBroken += OnBreakTutorial;
        HideInBox.OnPlayerHide += OnHideTutorial;
        HideInBox.OnPlayerExit += OnExitTutorial;

        states = new Dictionary<TutorialStates, GameObject>();
        stateObjects = new Dictionary<TutorialStates, List<GameObject>>();
        foreach (TutorialStates state in Enum.GetValues(typeof(TutorialStates)))
        {
            GameObject stateGO = transform.Find("Canvas/" + state.ToString()).gameObject;
            if (stateGO == null)
            {
                Debug.LogError("ERROR: Cannot find \"" + state.ToString() + "\" GameObject (for tutorial)");
                continue;
            }
            states.Add(state, stateGO); 
            states[state].SetActive(false);
            stateObjects[state] = new List<GameObject>();
        }

        // ----------- ADD STATE OBJECTS HERE --------------
        // add any objects that you want shown/hidden along with the state's prompt (e.g. targets)
        // they will then be shown/hidden automatically (when nextState is called)
        // you can add multiple objects to one state, and they will all turn on/off together
        stateObjects[TutorialStates.LOOK].Add(lookTarget);
        stateObjects[TutorialStates.WALK].Add(walkTarget);
        stateObjects[TutorialStates.SPRINT].Add(sprintTarget);
        stateObjects[TutorialStates.THROW].Add(throwTarget);
        stateObjects[TutorialStates.THROW_2].Add(throwTarget);
        stateObjects[TutorialStates.EMITTER_THROW].Add(throwTarget);
        stateObjects[TutorialStates.SONIC_GRENADE_THROW].Add(throwTarget);

        foreach (KeyValuePair<TutorialStates, List<GameObject>> kvp in stateObjects)
            foreach (GameObject obj in kvp.Value) obj.SetActive(false);

        currentState = TutorialStates.START;
        states[currentState].SetActive(true);
        foreach (GameObject obj in stateObjects[currentState]) obj.SetActive(true);
    }

    private void Update()
{
    if (currentState == TutorialStates.MIC)
    {
        // Player is making noise
        if (micInput.relativeVolume > 0.1f)
        {
            micHoldTimer += Time.deltaTime;
            if (micFillBar != null)
            {
                micFillBar.fillAmount = micHoldTimer / requiredMicTime;
                if (micFillBar.fillAmount > 0.5)
                    {
                        micFillBar.color = Color.Lerp(Color.white, Color.white, micFillBar.fillAmount);
                    }
            }

            if (micHoldTimer >= requiredMicTime)
            {
                micHoldTimer = 0f; 
                if (micFillBar != null) micFillBar.fillAmount = 0f; 
                PlayMonsterSound(); 
                nextState();
            }
        }
    }
}

    public void OnSkip(InputAction.CallbackContext context)
    {
        if (context.started && !hasFinishedTutorial)
        {
            hasFinishedTutorial = true;
            StartCoroutine(ExitTutorialAfterWait(0));
        }
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        if(context.started) nextState();
    }

    public void OnEcho(InputAction.CallbackContext context)
    {
        if (currentState == TutorialStates.CLICKER)
            nextState();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        float angle = Vector3.Dot(cam.transform.forward, look.transform.up);

        if(currentState == TutorialStates.LOOK && angle > 0.85) 
            nextState();
    }

    public void OnWalk()
    {
        var script = player.GetComponent<PlayerMovement>();
        if (currentState == TutorialStates.WALK && !script.GetSprint()) 
            nextState();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        var script = player.GetComponent<PlayerMovement>();

        //switch when you uncrouch
        if(context.performed && currentState == TutorialStates.CROUCH && !script.GetCrouch()) 
            nextState();
    }

    public void OnSprint()
    {
        var script = player.GetComponent<PlayerMovement>();
        if(currentState == TutorialStates.SPRINT && script.GetSprint()) 
            nextState();
    }

    public void OnPick()
    {
        if(currentState == TutorialStates.PICK) 
            nextState();
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.started && currentState == TutorialStates.THROW)
            nextState();
    }

    public void OnTargetThrow(string hitTag)
    {
        if(currentState == TutorialStates.THROW_2 && hitTag == "Throwable Rock") 
        {
            nextState();
        }
        else if (currentState == TutorialStates.EMITTER_THROW && hitTag == "Throwable Emitter")
        {
            nextState();
        }
        else if (currentState == TutorialStates.SONIC_GRENADE_THROW && hitTag == "Throwable Grenade")
        {
            nextState();
        }
    }

    public void OnChoose(InputAction.CallbackContext context)
    {
        if(currentState == TutorialStates.CHOOSING) 
            nextState();
    }

    public void OnBreakTutorial()
    {
        if (currentState == TutorialStates.BREAK)
            nextState();
    }

    public void OnHideTutorial()
    {
        if (currentState == TutorialStates.HIDE)
            nextState();
    }

    public void OnExitTutorial()
    {
        if (currentState == TutorialStates.EXIT_HIDE)
            nextState();
    }

    public void PlayMonsterSound()
    {
        audioSource.PlayOneShot(monsterSound, monsterSoundVolume);
    }

    // advances currentState to the next TutorialStates enum member
    private void nextState()
    {
        if (hasFinishedTutorial) return;

        TutorialStates finalState = (TutorialStates)Enum.GetValues(typeof(TutorialStates)).Length - 1;
        if (currentState == finalState)
        {
        } 
        else
        {
            states[currentState].SetActive(false);
            foreach (GameObject obj in stateObjects[currentState]) obj.SetActive(false);
            
            currentState += 1; // The state moves forward here
            
            states[currentState].SetActive(true);
            foreach (GameObject obj in stateObjects[currentState]) obj.SetActive(true);

            // --- ADD THIS NEW INITIALIZATION BLOCK ---
            if (currentState == TutorialStates.MIC)
            {
                micHoldTimer = 0f; // Reset the internal timer
                if (micFillBar != null)
                {
                    micFillBar.fillAmount = 0f; // Visually empty the circle
                    micFillBar.color = Color.white; // Ensure it starts white, not red
                }
            }
            // -----------------------------------------

            // (Your existing CLICKER_COOLDOWN time scale logic would go here too!)

            if (currentState == finalState) StartCoroutine(ExitTutorialAfterWait(5));
        }
    }
    private IEnumerator ExitTutorialAfterWait(float seconds)
    {
            hasFinishedTutorial = true;
            yield return new WaitForSeconds(seconds);

            states[currentState].SetActive(false);
            foreach (GameObject obj in stateObjects[currentState]) obj.SetActive(false);
            transform.Find("Canvas/Panel").gameObject.SetActive(false);

            PlayerRespawn script = player.GetComponent<PlayerRespawn>();
            script.RespawnPosition = spawnPointAfterExit.transform.position;
            script.Respawn();
            player.GetComponent<CrazyTimer>().StartEffect();
    }

}
