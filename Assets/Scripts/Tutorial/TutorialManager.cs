using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{   
     public static Action<string> OnTaskComplete; 
    private Dictionary<string, bool> tasks = new Dictionary<string, bool>();
    public float exitDelaySeconds = 3f;
    public GameObject player;
    public GameObject spawnPointAfterExit;

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
            tasks[taskName] = true;
            Debug.Log($"Task {taskName} Completed!");
        }
    }
}