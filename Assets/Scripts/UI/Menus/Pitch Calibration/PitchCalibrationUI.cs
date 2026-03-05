using UnityEngine;
using UnityEngine.SceneManagement;

public class PitchCalibrationUI : MonoBehaviour
{
	public GameObject MicPanel;
	public GameObject HighPitchPanel;
	public GameObject NormalPitchPanel;
	public GameObject FinalPanel;
	
	private enum State
	{
		MIC,
		HIGH,
		NORMAL,
		FINAL
	}
	private State state;

    public void Start()
    {
        state = State.MIC;
		MicPanel.SetActive(true);
		HighPitchPanel.SetActive(false);
    }

    public void OnMicButtonClicked()
	{
		MicPanel.SetActive(false);
		HighPitchPanel.SetActive(true);
		state = State.HIGH;
	}

	public void OnHighPitchButtonClicked()
	{
	}

	public void OnFinalButtonClicked()
	{
		Invoke(nameof(LoadGameScene), 3f);
	}

	private void LoadGameScene()
	{
		SceneManager.LoadScene("MVP");
	}

    public void Update()
    {
        switch (state)
		{
			case State.MIC:
				break;
			case State.HIGH:
				// do pitch stuff
				break;
			case State.NORMAL:
				// do pitch stuff
				break;
			case State.FINAL:
				break;
		}
    }
}