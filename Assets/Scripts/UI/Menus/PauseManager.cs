using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Tooltip("Toggle pause with P key")]
    [SerializeField] private bool enableKeyboardToggle = true;
    [SerializeField] private GameObject pauseUI;

    [SerializeField] private PlayerInput playerInput;  
    [SerializeField] private string pauseMap = "Pause Menu";
    [SerializeField] private string prevMap; // Map before pausing

    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = FindAnyObjectByType<PlayerInput>();
        }
    } 


    public void OnPause(InputAction.CallbackContext context)
    {
        if (!enableKeyboardToggle) return;

        if (context.performed) 
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void SetPaused(bool pause)
    {
        if (pause == IsPaused)
            return;

        // THE SNITCH: This will tell you exactly WHICH GameObject is crashing
        if (playerInput == null) 
        {
            Debug.LogError($"[GHOST COMPONENT FOUND] playerInput is missing on GameObject: '{gameObject.name}'. Delete the PauseManager component from this object!", gameObject);
            return; 
        }

        IsPaused = pause;

        if (IsPaused) 
        {
            prevMap = playerInput.currentActionMap?.name;
            playerInput.SwitchCurrentActionMap(pauseMap);
        } 
        else 
        {
            if (!string.IsNullOrEmpty(prevMap)) 
            {
                playerInput.SwitchCurrentActionMap(prevMap);
            }
        }

        Time.timeScale = IsPaused ? 0f : 1f;

        AudioListener.pause = IsPaused;

        if (pauseUI != null)
            pauseUI.SetActive(IsPaused);
    }

    private void OnDisable()
    {
        if (IsPaused)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            IsPaused = false;
        }
    }
}
