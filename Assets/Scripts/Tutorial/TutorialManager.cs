using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class TutorialManager : MonoBehaviour
{   
     public static Action<string> OnTaskComplete; 
    private Dictionary<string, bool> tasks = new Dictionary<string, bool>();
    public float exitDelaySeconds = 3f;
    public GameObject player;
    public GameObject spawnPointAfterExit;
    private int tasksCompleted = 0;
    public TMP_Text headText;
    public TMP_Text posterText;
    private bool hasFinishedTutorial = true;
    public GameObject warpPoint;

    void Start()
    {
        tasks.Add("Sprint", false);
        tasks.Add("Clicker", false);
        tasks.Add("Crouch", false);
        tasks.Add("Walk", false);
        tasks.Add("Pick-up", false);
        tasks.Add("Switch", false);
        tasks.Add("Throw", false);
    }

    void OnEnable()
    {
        OnTaskComplete += HandleTaskComplete;
    }

    void OnDisable()
    {
        OnTaskComplete -= HandleTaskComplete;
    }

    private void HandleTaskComplete(string taskName)
    {
        if (tasks.ContainsKey(taskName) && !tasks[taskName])
        {
            tasksCompleted++;
            tasks[taskName] = true;
            Debug.Log($"Task {taskName} Completed!");
        }
    }

    void Update()
    {
        if (tasksCompleted == 7)
        {
            headText.enabled = true;
            posterText.enabled = true;
            hasFinishedTutorial = false;
            StartCoroutine(textOff());
        }
    }
    
    IEnumerator textOff()
    {
        yield return new WaitForSeconds(3);
        headText.enabled = false;
    }

    public void OnGameStart(InputAction.CallbackContext context)
    {
        if (context.started && !hasFinishedTutorial)
        {
            hasFinishedTutorial = true;
            PlayerRespawn script = player.GetComponent<PlayerRespawn>();
            script.RespawnPosition = warpPoint.transform.position;
            script.Respawn();
            player.GetComponent<CrazyTimer>().StartEffect();
        }
    }

    public void OnSkip(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            hasFinishedTutorial = true;
            PlayerRespawn script = player.GetComponent<PlayerRespawn>();
            script.RespawnPosition = warpPoint.transform.position;
            script.Respawn();
            player.GetComponent<CrazyTimer>().StartEffect();
        }
    }
}