using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CirclingBatManager : MonoBehaviour
{
    public GameObject text;
    public GameObject col;
    public Image pitchBar;
    public GameObject batPrefab;
    public Transform centerPoint;
    public Transform endPoint;
    public CirclingBatMovement centerBat;

    public int count = 20;
    public float spawnRadius = 0.5f;
    public float minSpeed = 1.8f;
    public float maxSpeed = 3.2f;

    public float circleRadius = 1.2f;
    public float hoverHeightSpread = 1.6f;
    public float pullBackWeight = 4f;
    public float hardClampRadius = 2f;

    public LayerMask obstacleMask;
    public float agentRadius = 0.35f;
    public float skin = 0.05f;
    public bool depenetrateAfterMove = true;

    public float acceleration = 8f;

    public Transform playerCam;
    public float scareTriggerDistance = 10f;
    public string scareActionName = "ScareBats";
    public float escapeSpeedMultiplier = 1.5f;
    public float despawnDistance = 1.2f;

    [Range(0f, 1f)]
    [SerializeField] float minRelativeVolume = 0.35f;
    [Range(0f, 1f)]
    [SerializeField] float minRelativePitch = 0.35f;
    [SerializeField] float holdTimeToScare = 1.0f;

    [HideInInspector] public readonly List<CirclingBatMovement> agents = new();

    InputAction scareBatsAction;
    bool isScared;
    bool playerInRange;
    public bool IsScared => isScared;

    private MicInput micInput;
    float micHoldTimer;

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
    private float timePitchDecrement;
    private float currentRelativePitch;


    void Awake()
    {
        var micGo = GameObject.Find("MicInput");
        if (micGo != null)
            micInput = micGo.GetComponent<MicInput>();
    }

    void Start()
    {
        pitchBar.fillAmount = 0f;
        currentRelativePitch = minRelativePitch;
        timePitchDecrement = minRelativePitch / holdTimeToScare;

        if (!string.IsNullOrEmpty(scareActionName))
        {
            scareBatsAction = InputSystem.actions.FindAction(scareActionName);
            if (scareBatsAction != null)
            {
                scareBatsAction.performed += OnScareBats;
                scareBatsAction.Enable();
            }
        }

        SpawnBats();
    }

    void SpawnBats()
    {
        agents.Clear();

        if (centerBat != null)
        {
            centerBat.Init(this, Random.value * 9999f);
            if (!agents.Contains(centerBat))
                agents.Add(centerBat);
        }

        int spawnCount = Mathf.Max(0, count - agents.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * spawnRadius;
            offset.y *= 0.4f;
            Vector3 pos = centerPoint.position + offset;

            var go = Instantiate(batPrefab, pos, Quaternion.identity, transform);
            var agent = go.GetComponent<CirclingBatMovement>();
            if (!agent) agent = go.AddComponent<CirclingBatMovement>();

            agent.Init(this, Random.value * 9999f);
            agents.Add(agent);
        }
    }

    void Update()
    {
        if (playerCam == null || centerPoint == null) return;

        float distance = Vector3.Distance(playerCam.position, centerPoint.position);
        playerInRange = distance <= scareTriggerDistance && !isScared;
        if (playerInRange)
        {
            col.SetActive(true);
            text.SetActive(true);
        }
        else
        {
            col.SetActive(false);
            text.SetActive(false);
        }

        if (isScared || micInput == null) return;

        // if (!playerInRange)
        // {
        //     micHoldTimer = 0f;
        //     return;
        // }

        // if (micInput.relativePitch >= minRelativePitch && micInput.relativeVolume >= minRelativeVolume)
        // {
        //     micHoldTimer += Time.deltaTime;
        //     if (micHoldTimer >= holdTimeToScare)
        //         isScared = true;
        // }
        // else
        // {
        //     micHoldTimer = 0f;
        // }





        // If player walks too far away, reset everything
        if (!playerInRange)
        {
            micHoldTimer = 0f;
            audioHistory.Clear();

            return;
        }

        // If UI disabled (too far or look away) reset current relative pitch
        if (!text.activeInHierarchy)
        {
            currentRelativePitch = minRelativePitch;
        }

        // Calculate duration average pitch, average volume, and if valid (enough data)
        float averagePitch = 0f;
        float averageVolume = 0f;
        bool hasValidData = false;

        float relPitchClamp = Mathf.Clamp01(micInput.relativePitch);
        float relVolClamp = Mathf.Clamp01(micInput.relativeVolume);
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
            micHoldTimer += Time.deltaTime;

            if (micHoldTimer >= holdTimeToScare)
            {
                isScared = true;
            }
        }
        else
        {
            micHoldTimer = 0f;
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

    void OnScareBats(InputAction.CallbackContext context)
    {
        if (!context.performed || isScared || playerCam == null || centerPoint == null) return;

        if (!playerInRange) return;

        isScared = true;
    }

    public void NotifyAgentDespawn(CirclingBatMovement agent)
    {
        if (agent != null)
        {
            agents.Remove(agent);
            if (centerBat == agent) centerBat = null;
        }

        if (agents.Count == 0)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (scareBatsAction != null)
        {
            scareBatsAction.performed -= OnScareBats;
        }
    }
}
