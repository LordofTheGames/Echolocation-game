using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeasurePitch : MonoBehaviour
{
    public GameObject KeepGoing;
    public GameObject MoreConsistent;
    public GameObject Good;
    public GameObject Button;
    public GameObject SkipButton;
    public MicInput MicInput;

    public TMP_Text Number;

    public float consistencyRange = 8;
    public float maxConsistentTimeSecs = 2;

    private int idx = 0;
    private int maxIdx;
    private float elapsedTimeSecs = 0;
    private float consistentTimeSecs = 0;
    private float[] currPitch;

    private GameObject currPrompt;

    void Awake()
    {
        maxIdx = (int)(60 * (maxConsistentTimeSecs + 1)); // maxConsistentTimeSecs + 1 seconds (assuming 60fps)
        currPitch = new float[maxIdx + 1];
        currPrompt = KeepGoing;
        KeepGoing.SetActive(false);
        MoreConsistent.SetActive(false);
        Good.SetActive(false);
        Button.SetActive(false);
    }

    public float GetPitch()
    {

        if (Input.GetKeyDown("space"))
        {
            changePrompt(Good);
            Number.text = "";
            Button.SetActive(true);
            StartCoroutine(SelectButtonLater(Button));
            return 0f;
        }

        elapsedTimeSecs += Time.deltaTime;

        currPitch[idx] = MicInput.pitchHz;
        Number.text = Mathf.RoundToInt(MicInput.pitchHz).ToString();
        if (currPitch[idx] != 0 && checkConsistency())
        {
            consistentTimeSecs += Time.deltaTime;
            idx += 1;
            if (idx == maxIdx) idx = 0;
        }
        else if (currPitch[idx] != 0)
        {
            consistentTimeSecs = 0;
            idx = 0;
            if (elapsedTimeSecs >= 3)
            {
                changePrompt(MoreConsistent);
            }
            if (SkipButton != null && elapsedTimeSecs >= 5)
            {
                SkipButton.SetActive(true);
                StartCoroutine(SelectButtonLater(SkipButton));
            }
        }

        if (elapsedTimeSecs > 1 && elapsedTimeSecs < 3)
            changePrompt(KeepGoing);

        if (consistentTimeSecs >= maxConsistentTimeSecs)
        {
            if (SkipButton != null) SkipButton.SetActive(false);
            changePrompt(Good);
            Number.text = "";
            Button.SetActive(true);
            StartCoroutine(SelectButtonLater(Button));
            return currPitch.Sum() / currPitch.Length;
        }
        else return -1;
    }

    private bool checkConsistency()
    {
        for (int i = 0; i < idx; i++)
        {
            if (currPitch[idx] - currPitch[i] > consistencyRange || currPitch[idx] - currPitch[i] < -consistencyRange) 
                return false;
        }
        return true;
    }

    private void changePrompt(GameObject prompt)
    {
            currPrompt.SetActive(false);
            currPrompt = prompt;
            currPrompt.SetActive(true);
    }

    private IEnumerator SelectButtonLater(GameObject button)
        {
            // Wait for one frame so the UI can fully initialize
            yield return null; 
            button.GetComponent<Button>().Select();
        }
}
