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
    public float maxConsistentTimeSecs = 3;

    private int idx = 0;
    private int maxIdx = 60; // 1 second (assuming 60fps)
    private float elapsedTimeSecs = 0;
    private float consistentTimeSecs = 0;
    private float[] currPitch;

    private GameObject currPrompt;

    void Awake()
    {
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
        if (idx != 0 && currPitch[idx] != 0 && currPitch[idx] - currPitch[idx - 1] <= consistencyRange && currPitch[idx] - currPitch[idx - 1] >= -consistencyRange)
        {
            consistentTimeSecs += Time.deltaTime;
        }
        else if (idx != 0 && currPitch[idx] != 0)
        {
            consistentTimeSecs = 0;
            if (elapsedTimeSecs >= 3)
            {
                changePrompt(MoreConsistent);
            }
        }

        idx += 1;
        if (idx == maxIdx) idx = 0;

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
