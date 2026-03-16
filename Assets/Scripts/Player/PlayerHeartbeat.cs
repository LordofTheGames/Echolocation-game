using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerHeartbeat : MonoBehaviour
{
    [Header("Monster Source")]
    public Transform[] monsterTransforms;

    [Header("Heartbeat Audio")]
    public AudioClip heartbeatClip;
    public AudioSource audioSource;

    [Header("Distance Thresholds (m)")]
    public float maxDistance = 20f;
    public float mediumDistance = 10f;
    public float closeDistance = 5f;

    [Header("Slow (20m ~ 10m)")]
    [Range(0f, 1f)] public float slowVolume = 0.35f;
    [Range(0.5f, 2f)] public float slowPitch = 0.75f;

    [Header("Medium (10m ~ 5m)")]
    [Range(0f, 1f)] public float mediumVolume = 0.6f;
    [Range(0.5f, 2f)] public float mediumPitch = 1f;

    [Header("Fast (within 5m)")]
    [Range(0f, 1f)] public float fastVolume = 0.9f;
    [Range(0.5f, 2f)] public float fastPitch = 1.35f;

    [Header("Smoothing")]
    [Range(1f, 30f)] public float smoothSpeed = 12f;
    [Range(0.02f, 0.5f)] public float updateInterval = 0.1f;

    float _targetVolume;
    float _targetPitch;
    float _nextUpdateTime;
    bool _wasInRange;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.clip = heartbeatClip;
        }
        _targetVolume = 0f;
        _targetPitch = slowPitch;
    }

    void Update()
    {
        if (heartbeatClip == null || audioSource == null) return;

        if (Time.time >= _nextUpdateTime)
        {
            _nextUpdateTime = Time.time + updateInterval;
            float closestDist = GetClosestMonsterDistance();
            UpdateTargetParams(closestDist);
        }

        audioSource.volume = Mathf.MoveTowards(audioSource.volume, _targetVolume, smoothSpeed * Time.deltaTime);
        audioSource.pitch = Mathf.MoveTowards(audioSource.pitch, _targetPitch, smoothSpeed * Time.deltaTime);

        if (_targetVolume > 0.001f)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying && audioSource.volume < 0.01f)
                audioSource.Stop();
        }
    }

    float GetClosestMonsterDistance()
    {
        if (monsterTransforms == null || monsterTransforms.Length == 0) return float.MaxValue;

        Vector3 playerPos = transform.position;
        float minSq = float.MaxValue;
        foreach (var t in monsterTransforms)
        {
            if (t == null) continue;
            float sq = (t.position - playerPos).sqrMagnitude;
            if (sq < minSq) minSq = sq;
        }
        return minSq == float.MaxValue ? float.MaxValue : Mathf.Sqrt(minSq);
    }

    void UpdateTargetParams(float distance)
    {
        if (distance > maxDistance)
        {
            _targetVolume = 0f;
            _targetPitch = slowPitch;
            _wasInRange = false;
            return;
        }

        _wasInRange = true;
        if (distance <= closeDistance)
        {
            _targetVolume = fastVolume;
            _targetPitch = fastPitch;
        }
        else if (distance <= mediumDistance)
        {
            _targetVolume = mediumVolume;
            _targetPitch = mediumPitch;
        }
        else
        {
            _targetVolume = slowVolume;
            _targetPitch = slowPitch;
        }
    }

    public void SetMonsterTransforms(Transform[] monsters)
    {
        monsterTransforms = monsters;
    }
}
