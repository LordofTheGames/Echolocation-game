using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{   
     public static Action<string> OnTaskComplete; 
    private Dictionary<string, bool> tasks = new Dictionary<string, bool>();

    void Start()
    {
        tasks.Add("Sprint", false);
        tasks.Add("Clicker", false);
        tasks.Add("Crouch", false);
        tasks.Add("Walk", false);
        tasks.Add("Pick-up", false);
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