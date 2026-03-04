using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Tooltip("Toggle pause with P key")]
    [SerializeField] private bool enableKeyboardToggle = true;
    [SerializeField] private GameObject pauseUI;

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!enableKeyboardToggle) return;

        TogglePause();
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
