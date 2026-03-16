using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;



public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Tooltip("Toggle pause with P key")]
    [SerializeField] private bool enableKeyboardToggle = true;
    [SerializeField] private GameObject pauseUI;

    [SerializeField] private GameObject firstSelectedButton; // Drag Quit game object here in inspector


    [SerializeField] private PlayerInput playerInput;  
    [SerializeField] private string pauseMap = "Pause Menu";
    private string prevMap; // Map before pausing

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

        IsPaused = pause;

        Time.timeScale = IsPaused ? 0f : 1f;

        AudioListener.pause = IsPaused;

        if (pauseUI != null)
            pauseUI.SetActive(IsPaused);

        if (IsPaused) 
        {
            prevMap = playerInput.currentActionMap?.name;
            playerInput.SwitchCurrentActionMap(pauseMap);
            EnableCursor();

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        } 
        else 
        {
            if (!string.IsNullOrEmpty(prevMap)) 
            {
                playerInput.SwitchCurrentActionMap(prevMap);
            }
            DisableCursor();
        }   
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


    private void EnableCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void DisableCursor()
    {
        Cursor.visible = false;
    }

    // Changed delay method since pause "freezes" time so needs another way for delay
    public void OnQuitClicked()
    {
        StartCoroutine(LoadMainMenuWithDelay());
    }

    private System.Collections.IEnumerator LoadMainMenuWithDelay()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        // Reset variables before loading next scene
        Time.timeScale = 1f;
        AudioListener.pause = false;
        IsPaused = false;

        SceneManager.LoadScene("MainMenu");
    }
}
