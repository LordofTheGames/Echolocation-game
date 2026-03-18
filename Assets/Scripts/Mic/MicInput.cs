using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MicInput : MonoBehaviour
{

    // [Range(0f,3f)]
    // public float sensitivity = 3f;
    public float volume;
    public float relativeVolume;
    private int windowSize = 4096;

    private AudioSource audioSource;
    private string micDevice;
    private float[] samples;

    public float pitchHz;
    public float relativePitch;
    public SwiftF0Runner swiftF0;
    
    private int sampleRate;

    [SerializeField] private AudioMixer micMixer;
    [SerializeField] private AudioMixerGroup micSilentGroup;
    [SerializeField] private string micVolumeParam = "MicSilentVolume";

    private static float highPitch;
    private static float normalPitch;
    private static float highVolume;
    private static float normalVolume;

    [RuntimeInitializeOnLoadMethod]
    static void InitialisePitches()
    {
        highPitch = 0;
        normalPitch = 0;
        highVolume = 0;
        normalVolume = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string currSceneName = SceneManager.GetActiveScene().name;
        // if no mic calibration, set default values
        if (currSceneName != "MicCalibration" && currSceneName != "MainMenu")
        {
            if (normalPitch == 0)
                normalPitch = 50;
            if (highPitch == 0)
                highPitch = 140 - normalPitch;
            if (normalVolume == 0)
                normalVolume = 0.2f;
            if (highVolume == 0)
                highVolume = 0.97f - normalVolume;
        }
        sampleRate = AudioSettings.outputSampleRate;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;

        if (micSilentGroup != null)
            audioSource.outputAudioMixerGroup = micSilentGroup;

        if (Microphone.devices.Length == 0){
            Debug.LogError("no microphone device");
            enabled = false;
            return;
        }

        micDevice = Microphone.devices[0];
        audioSource.clip = Microphone.Start(micDevice, true,1, sampleRate);

        audioSource.Play();

        samples = new float[windowSize];

        if (micMixer != null && !string.IsNullOrEmpty(micVolumeParam))
            micMixer.SetFloat(micVolumeParam, -80f);
    }

    // Update is called once per frame
    void Update() 
    {
        int positon = Microphone.GetPosition(micDevice);
        int start = positon - windowSize;
        if (start <0) return;

        audioSource.clip.GetData(samples, start);

        float sum = 0f;
        for (int i = 0; i < samples.Length; i++){
            float s = samples[i];
            sum += s * s;
        }
        float rms = Mathf.Sqrt(sum / samples.Length);

        // float target = Mathf.Clamp01(rms * sensitivity);
        // if (target  < 0.05f){
        //     target = 0f;
        // }

        // loudness = target;
        volume = rms;
        if (volume  < 0.01f){
            volume = 0f;
        }
        relativeVolume = getRelativeVolume(volume);

        if (volume == 0f){
            pitchHz = 0f;
            return;
        }
        
        if (swiftF0 != null){
            pitchHz = swiftF0.Run(samples);
            relativePitch = getRelativePitch(pitchHz);
        } else{
            pitchHz = 0f;
            relativePitch = 0;
        }
    }

    void OnDisable()
    {
        if (!string.IsNullOrEmpty(micDevice))
            Microphone.End(micDevice);
    }

    public void setPitchCalibrationValues(float highPitch, float normalPitch)
    {
        MicInput.normalPitch = normalPitch;
        MicInput.highPitch = highPitch;
    }
    private float getRelativePitch(float pitch)
    {
        if (highPitch != 0)
            return (pitch - normalPitch) / highPitch;
        else return 0;
    }
    public void setVolumeCalibrationValues(float highVolume, float normalVolume)
    {
        MicInput.normalVolume = normalVolume;
        MicInput.highVolume = highVolume;
    }
    private float getRelativeVolume(float volume)
    {
        if (highVolume != 0)
            return (volume - normalVolume) / highVolume;
        else return 0;
    }
}