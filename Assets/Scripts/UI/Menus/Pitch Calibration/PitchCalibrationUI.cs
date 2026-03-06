using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PitchCalibrationUI : MonoBehaviour
{
	public GameObject MicPanel;
	public GameObject HighPitchPanel;
	public GameObject NormalPitchPanel;
	public GameObject FinalPanel;
	public MicInput MicInput;
	
	private enum State
	{
		MIC,
		HIGH,
		NORMAL,
		FINAL
	}
	private State state;

	private MeasurePitch highPitchScript;
	private MeasurePitch normalPitchScript;
	private float highPitch = 0;
	private float normalPitch = 0;
	private bool measureFinished = false;

    void Start()
    {
        state = State.MIC;
		MicPanel.SetActive(true);
		HighPitchPanel.SetActive(false);

		highPitchScript = HighPitchPanel.GetComponent<MeasurePitch>();	
		normalPitchScript = NormalPitchPanel.GetComponent<MeasurePitch>();	
    }

    public void OnMicButtonClicked()
	{
		MicPanel.SetActive(false);
		HighPitchPanel.SetActive(true);
		state = State.HIGH;
	}

	public void OnHighPitchButtonClicked()
	{
		if (measureFinished)
		{
			HighPitchPanel.SetActive(false);
			NormalPitchPanel.SetActive(true);
			state = State.NORMAL;
			measureFinished = false;
		}
	}

	public void OnNormalPitchButtonClicked()
	{
		if (measureFinished)
		{
			NormalPitchPanel.SetActive(false);
			FinalPanel.SetActive(true);
            StartCoroutine(SelectFinalButtonLater());
			state = State.FINAL;
			measureFinished = false;
		}
	}

	public void OnFinalButtonClicked()
	{
		Invoke(nameof(LoadGameScene), 3.5f);
	}

	private void LoadGameScene()
	{
		SceneManager.LoadScene("MVP");
	}

    void Update()
    {
        switch (state)
		{
			case State.MIC:
				break;
			case State.HIGH:
				if (!measureFinished) highPitch = highPitchScript.GetPitch();
				if (highPitch != -1) measureFinished = true;
				break;
			case State.NORMAL:
				if(!measureFinished) normalPitch = normalPitchScript.GetPitch();
				if (normalPitch != -1) {
					measureFinished = true;
					MicInput.setPitchCalibrationValues(highPitch, normalPitch);
				}
				break;
			case State.FINAL:
				break;
		}
    }


    private IEnumerator SelectFinalButtonLater()
        {
            // Wait for one frame so the UI can fully initialize
            yield return null; 
            FinalPanel.GetComponentInChildren<Button>().Select();
        }
}