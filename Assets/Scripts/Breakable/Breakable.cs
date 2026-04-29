using System.Collections;
using UnityEngine;
using System;
using Unity.Behavior;
// using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;
// using UnityEditor;


[SelectionBase]
public class Breakable : MonoBehaviour
{
    [SerializeField] private GameObject intactObject;
    [SerializeField] private GameObject brokenObject;

    [SerializeField] private PickupItem itemLockedUntilBreak;

    [SerializeField] private AudioClip breakSound;
    [SerializeField] private float breakSoundVolume = 1f;

    [Range(0, 1)]
    [SerializeField] private float minRelativeVolume = 0.5f;

    [Range(0, 1)]
    [SerializeField] private float minRelativePitch = 0.5f;

    [SerializeField] private float holdTimeToBreak = 1.2f;
    [SerializeField] private float maxDistanceToMic = 5f;
    [SerializeField] private float secondsUntilHideBrokenVisual = 5f;

    [SerializeField] private AudioSource audioSource;

    public bool IsAlarmBox = false;
    public AudioSource AlarmSound1;
    public AudioSource AlarmSound2;
    private ChaseStart cs;
    private FlashLights fl;

    private bool hasBroken;
    private float holdTimer;

    private GameObject player;
    private MicInput micInput;

    public static event System.Action OnRockBroken;
    public static event Action<Breakable> OnBroken;

    public bool HasBroken => hasBroken;

    private Image pitchBar;
    private GameObject breakablePitchUI;

    // Variables for tracking pitch history
    [Tooltip("How many frames of sound we need before trusting the data")]
    [SerializeField] private int minFramesForValidAudio = 5;

    [Tooltip("Time to hold audio data over")]
    [SerializeField] private float audioHistoryDuration = 0.2f;

    private struct AudioRecord
    {
        public float pitch;
        public float volume;
        public float time;
    }
    private Queue<AudioRecord> audioHistory = new Queue<AudioRecord>();

    // Make it easier to break the longer they interact
    private float maxTimeToBreak = 3.5f; 
    private float timePitchDecrement;
    private float currentRelativePitch;


    private void Start()
    {
        // Find the active root object ("UI")
        GameObject uiRoot = GameObject.Find("UI");

        if (uiRoot != null)
        {

            // Use Transform.Find with the exact path down to the inactive child
            Transform breakablePitchUITransform = uiRoot.transform.Find("BreakablePitchUI");
            Transform pitchBarTransform = uiRoot.transform.Find("BreakablePitchUI/Pitch Bar/Pitch Bar Image");

            if (breakablePitchUITransform != null)
            {
                breakablePitchUI = breakablePitchUITransform.gameObject;

            }

            if (pitchBarTransform != null)
            {
                // Grab the components and set up your variables
                pitchBar = pitchBarTransform.GetComponent<Image>();

                // Set initial fill to 0 and hide it
                pitchBar.fillAmount = 0f;
                pitchBar.gameObject.SetActive(false);
            }
        }

        currentRelativePitch = minRelativePitch;
        timePitchDecrement = minRelativePitch / maxTimeToBreak;
    }

    private void Awake()
    {
        micInput = GameObject.Find("MicInput").GetComponent<MicInput>();
        player = GameObject.FindGameObjectWithTag("Player");
        cs = GameObject.Find("Chase Trigger").GetComponent<ChaseStart>();
        fl = GameObject.Find("ChasePos").GetComponent<FlashLights>();

        if (intactObject != null) intactObject.SetActive(true);
        if (brokenObject != null) brokenObject.SetActive(false);

        // Key stays visible, but cannot be picked up before glass breaks
        if (itemLockedUntilBreak != null)
            itemLockedUntilBreak.SetCanPickup(false);
    }

    private void Update()
    {
        if (hasBroken || micInput == null || player == null) return;

        // If player walks too far away, reset everything
        if (Vector3.Distance(transform.position, player.transform.position) > maxDistanceToMic)
        {
            holdTimer = 0f;
            audioHistory.Clear();

            return;
        }

        // If UI disabled (too far or look away) reset current relative pitch
        if (breakablePitchUI != null && !breakablePitchUI.activeInHierarchy)
        {
            currentRelativePitch = minRelativePitch;
        }

        // Calculate duration average pitch, average volume, and if valid (enough data)
        float averagePitch = 0f;
        float averageVolume = 0f;
        bool hasValidData = false;

        float relPitchClamp = Mathf.Min(micInput.relativePitch, 1);
        float relVolClamp = Mathf.Min(micInput.relativeVolume, 1);
        bool isValidFrame = relPitchClamp >= 0f && 
                           relVolClamp >= 0f;

        // Only add new data to the queue if there is noise
        if (relVolClamp > 0.01f && isValidFrame)
        {
            // Add current frame's data to the queue
            audioHistory.Enqueue(new AudioRecord { 
                pitch = relPitchClamp, 
                volume = relVolClamp, 
                time = Time.time 
            });
        }

        // Remove entries older than our duration
        while (audioHistory.Count > 0 && Time.time - audioHistory.Peek().time > audioHistoryDuration)
        {
            audioHistory.Dequeue();
        }

        // Calculate averages if we have enough sustained frames
        if (audioHistory.Count >= minFramesForValidAudio)
        {
            hasValidData = true;
            
            float sumPitch = 0f;
            float sumVolume = 0f;
            
            // Add up all the values in our history
            foreach (var record in audioHistory)
            {
                sumPitch += record.pitch;
                sumVolume += record.volume;
            }
            
            // Divide by the count to get the average (mean)
            averagePitch = sumPitch / audioHistory.Count;
            averageVolume = sumVolume / audioHistory.Count;
        }
        
        // Using average pitch/volume instead of raw pitch
        if (hasValidData && averagePitch >= currentRelativePitch && averageVolume >= minRelativeVolume)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTimeToBreak)
            {
                Break();
            }
        }
        else
        {
            holdTimer = 0f;
        }

        if (pitchBar != null)
        {
            if (hasValidData && averageVolume > 0.01f) // Only do pitch if above a certain volume and enough data
            {
                // Turn the bar ON so we can see it
                if (!pitchBar.gameObject.activeSelf) 
                {
                    pitchBar.gameObject.SetActive(true);
                }

                // Use average pitch to calculate new width of bar
                float fillAmount = (averagePitch > currentRelativePitch) ? 1f : averagePitch / currentRelativePitch;
                pitchBar.fillAmount = fillAmount;
            }
            else
            {
                // Turn the bar OFF when quiet or insufficient data
                if (pitchBar.gameObject.activeSelf)
                {
                    pitchBar.fillAmount = 0f;
                    pitchBar.gameObject.SetActive(false);
                }
            }
        }

        // Make easier over time
        if (averageVolume > 0.01f)
        {
            currentRelativePitch -= timePitchDecrement * Time.deltaTime;
            if (currentRelativePitch < 0) currentRelativePitch = 0f;
        }
    }

    private void Break()
    {
        if (hasBroken) return;

        OnRockBroken?.Invoke(); // Tell the game the player broke a rock
        OnBroken?.Invoke(this);

        hasBroken = true;
        
        // Hide UI immediately upon breaking
        if (pitchBar != null)
        {
            pitchBar.fillAmount = 0f;
            pitchBar.gameObject.SetActive(false);
        }

        if (intactObject != null) intactObject.SetActive(false);
        if (brokenObject != null) brokenObject.SetActive(true);

        if (itemLockedUntilBreak != null)
            itemLockedUntilBreak.SetCanPickup(true);

        if (audioSource != null && breakSound != null)
        {
            audioSource.PlayOneShot(breakSound, breakSoundVolume);
        }
        if (IsAlarmBox)
        {
            cs.GlassBroken = true;
            GameObject.Find("Monster").GetComponent<BehaviorGraphAgent>().BlackboardReference.SetVariableValue("chaseInit", true);
            fl.flashLights();
            StartCoroutine(PlayAlarms());
        } 

        StartCoroutine(HideBrokenVisualLater());
    }

    private IEnumerator HideBrokenVisualLater()
    {
        if (secondsUntilHideBrokenVisual <= 0) yield break;
        yield return new WaitForSeconds(secondsUntilHideBrokenVisual);
        if (brokenObject != null)
        {
            brokenObject.SetActive(false);
        }
    }

    private IEnumerator PlayAlarms()
    {
        yield return new WaitForSeconds(3);
        AlarmSound1.Play();
        AlarmSound2.Play();
    }
}
