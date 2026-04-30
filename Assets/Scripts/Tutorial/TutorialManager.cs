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
    public GameObject headText;
    public GameObject posterText;
    private bool hasFinishedTutorial = true;
    public GameObject warpPoint;
    public bool textOn = false;

    void Start()
    {
        // tasks.Add("Sprint", false);
        tasks.Add("Clicker", false);
        tasks.Add("Crouch", false);
        tasks.Add("Walk", false);
        tasks.Add("Pick-up", false);
        tasks.Add("Switch", false);
        tasks.Add("Throw", false);
        headText.SetActive(false);
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
        if (tasksCompleted == 6 && !textOn)
        {
            headText.SetActive(true);
            posterText.SetActive(true);
            hasFinishedTutorial = false;
            StartCoroutine(textOff());
            textOn = true;  
        }
    }
    
    IEnumerator textOff()
    {
        yield return new WaitForSeconds(3);
        headText.SetActive(false);
    }

    public void OnGameStart(InputAction.CallbackContext context)
    {
        if (context.started && !hasFinishedTutorial)
        {
            hasFinishedTutorial = true;
            PlayerRespawn script = player.GetComponent<PlayerRespawn>();
            script.RespawnPosition = warpPoint.transform.position;
            script.Respawn();
            InventoryManager man = GameObject.Find("UIManager").GetComponent<InventoryManager>();
            man.Add(ItemType.SonicGrenade, 3);
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
            InventoryManager man = GameObject.Find("UIManager").GetComponent<InventoryManager>();
            man.Add(ItemType.SonicGrenade, 3);
            player.GetComponent<CrazyTimer>().StartEffect();
        }
    }
}