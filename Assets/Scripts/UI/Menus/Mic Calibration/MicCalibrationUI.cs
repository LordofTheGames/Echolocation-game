using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MicCalibrationUI : MonoBehaviour
{
	public GameObject MicPanel;
	public GameObject LoudVolPanel;
	public GameObject NormalVolPanel;
	public GameObject FinalVolPanel;
	public GameObject HighPitchPanel;
	public GameObject NormalPitchPanel;
	public GameObject FinalPitchPanel;
	public MicInput MicInput;
	
	private enum State
	{
		MIC,
		LOUD_VOL,
		NORMAL_VOL,
		FINAL_VOL,
		HIGH_PITCH,
		NORMAL_PITCH,
		FINAL_PITCH
	}
	private State state;

	private MeasurePitch highPitchScript;
	private MeasurePitch normalPitchScript;
	private MeasureVolume loudVolScript;
	private MeasureVolume normalVolScript;
	private float highPitch = 0;
	private float normalPitch = 0;
	private float loudVol = 0;
	private float normalVol = 0;
	private bool measureFinished = false;

    void Start()
    {
        state = State.MIC;
		MicPanel.SetActive(true);

		highPitchScript = HighPitchPanel.GetComponent<MeasurePitch>();	
		normalPitchScript = NormalPitchPanel.GetComponent<MeasurePitch>();	
		loudVolScript = LoudVolPanel.GetComponent<MeasureVolume>();	
		normalVolScript = NormalVolPanel.GetComponent<MeasureVolume>();	
    }

    public void OnMicButtonClicked()
	{
		MicPanel.SetActive(false);
		LoudVolPanel.SetActive(true);
		state = State.LOUD_VOL;
	}
	public void OnLoudVolButtonClicked()
	{
		if (measureFinished)
		{
			LoudVolPanel.SetActive(false);
			NormalVolPanel.SetActive(true);
			state = State.NORMAL_VOL;
			measureFinished = false;
		}
	}
	public void OnNormalVolButtonClicked()
	{
		if (measureFinished)
		{
			NormalVolPanel.SetActive(false);
			FinalVolPanel.SetActive(true);
            StartCoroutine(SelectButtonLater(FinalVolPanel));
			state = State.FINAL_VOL;
			measureFinished = false;
		}
	}
	public void OnFinalVolButtonClicked()
	{
		FinalVolPanel.SetActive(false);
		HighPitchPanel.SetActive(true);
		state = State.HIGH_PITCH;
	}

	public void OnHighPitchButtonClicked()
	{
		if (measureFinished)
		{
			HighPitchPanel.SetActive(false);
			NormalPitchPanel.SetActive(true);
			state = State.NORMAL_PITCH;
			measureFinished = false;
		}
	}

	public void OnNormalPitchButtonClicked()
	{
		if (measureFinished)
		{
			NormalPitchPanel.SetActive(false);
			FinalPitchPanel.SetActive(true);
            StartCoroutine(SelectButtonLater(FinalPitchPanel));
			state = State.FINAL_PITCH;
			measureFinished = false;
		}
	}

	public void onSkipButtonClicked()
	{
		NormalPitchPanel.SetActive(false);
		FinalPitchPanel.SetActive(true);
		StartCoroutine(SelectButtonLater(FinalPitchPanel));
		state = State.FINAL_PITCH;
		measureFinished = false;
		normalPitch = 50;
		MicInput.setPitchCalibrationValues(highPitch - normalPitch, normalPitch);
	}

	public void OnFinalPitchButtonClicked()
	{
		Invoke(nameof(LoadGameScene), 3.5f);
	}

	private void LoadGameScene()
	{
		SceneManager.LoadScene("BETA");
	}

    void Update()
    {
        switch (state)
		{
			case State.MIC:
				break;
			case State.LOUD_VOL:
				if (!measureFinished) loudVol = loudVolScript.GetVolume();
				if (loudVol != -1) measureFinished = true;
				break;
			case State.NORMAL_VOL:
				if(!measureFinished) normalVol = normalVolScript.GetVolume();
				if (normalVol != -1) {
					measureFinished = true;
					MicInput.setVolumeCalibrationValues(loudVol - normalVol, normalVol);
				}
				break;
			case State.FINAL_VOL:
				break;
			case State.HIGH_PITCH:
				if (!measureFinished) highPitch = highPitchScript.GetPitch();
				if (highPitch != -1) measureFinished = true;
				break;
			case State.NORMAL_PITCH:
				if(!measureFinished) normalPitch = normalPitchScript.GetPitch();
				if (normalPitch != -1) {
					measureFinished = true;
					MicInput.setPitchCalibrationValues(highPitch - normalPitch, normalPitch);
				}
				break;
			case State.FINAL_PITCH:
				break;
		}
    }


    private IEnumerator SelectButtonLater(GameObject panel)
        {
            // Wait for one frame so the UI can fully initialize
            yield return null; 
            panel.GetComponentInChildren<Button>().Select();
        }
}