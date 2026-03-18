using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeasureVolume : MonoBehaviour
{
    public GameObject KeepGoing;
    public GameObject MoreConsistent;
    public GameObject Good;
    public GameObject Button;
    public MicInput MicInput;

    public TMP_Text Number;

    public float consistencyRange = 0.05f;
    public float maxConsistentTimeSecs = 2;

    private int idx = 0;
    private int maxIdx;
    private float elapsedTimeSecs = 0;
    private float consistentTimeSecs = 0;
    private float[] currVolume;

    private GameObject currPrompt;

    void Awake()
    {
        maxIdx = (int)(60 * (maxConsistentTimeSecs + 1)); // maxConsistentTimeSecs + 1 seconds (assuming 60fps)
        currVolume = new float[maxIdx + 1];
        currPrompt = KeepGoing;
        KeepGoing.SetActive(false);
        MoreConsistent.SetActive(false);
        Good.SetActive(false);
        Button.SetActive(false);
    }

    public float GetVolume()
    {

        if (Input.GetKeyDown("space"))
        {
            changePrompt(Good);
            Number.text = "";
            Button.SetActive(true);
            StartCoroutine(SelectButtonLater());
            return 0f;
        }

        elapsedTimeSecs += Time.deltaTime;

        currVolume[idx] = MicInput.volume;
        Number.text = Mathf.RoundToInt(MicInput.volume * 100).ToString();
        if (currVolume[idx] != 0 && checkConsistency())
        {
            consistentTimeSecs += Time.deltaTime;
            idx += 1;
            if (idx == maxIdx) idx = 0;
        }
        else if (currVolume[idx] != 0)
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
            Number.text = "";
            Button.SetActive(true);
            StartCoroutine(SelectButtonLater());
            return currVolume.Sum() / currVolume.Length;
        }
        else return -1;
    }

    private bool checkConsistency()
    {
        for (int i = 0; i < idx; i++)
        {
            if (currVolume[idx] - currVolume[i] > consistencyRange || currVolume[idx] - currVolume[i] < -consistencyRange) 
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
