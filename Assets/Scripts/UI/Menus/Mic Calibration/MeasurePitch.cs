using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MeasurePitch : MonoBehaviour
{
    public GameObject KeepGoing;
    public GameObject MoreConsistent;
    public GameObject Good;
    public GameObject Button;
    public MicInput MicInput;

    public float consistencyRange = 5;
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
        elapsedTimeSecs += Time.deltaTime;

        currPitch[idx] = MicInput.pitchHz;
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
        }

        if (elapsedTimeSecs > 1 && elapsedTimeSecs < 3)
            changePrompt(KeepGoing);

        if (consistentTimeSecs >= maxConsistentTimeSecs)
        {
            changePrompt(Good);
            Button.SetActive(true);
            StartCoroutine(SelectButtonLater());
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

    private IEnumerator SelectButtonLater()
        {
            // Wait for one frame so the UI can fully initialize
            yield return null; 
            Button.GetComponent<Button>().Select();
        }
}
