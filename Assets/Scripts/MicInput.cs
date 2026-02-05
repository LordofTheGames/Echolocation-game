using UnityEngine;
using UnityEngine.Audio;

public class MicInput : MonoBehaviour
{

    private float sensitivity = 3f;
    public float loudness;
    private int windowSize = 4096;

    private AudioSource audioSource;
    private string micDevice;
    private float[] samples;

    public float pitchHz;

    private int spectrumSize = 2048;
    private int minPitchHz = 60;
    private int maxPitchHz = 500;
    private FFTWindow fftWindow = FFTWindow.BlackmanHarris;
    private int sampleRate;

    private AudioPitchEstimator pitchEstimator;
    [SerializeField] private AudioMixer micMixer;
    [SerializeField] private AudioMixerGroup micSilentGroup;
    [SerializeField] private string micVolumeParam = "MicSilentVolume";


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        pitchEstimator = GetComponent<AudioPitchEstimator>();
        if (pitchEstimator == null)
            pitchEstimator = gameObject.AddComponent<AudioPitchEstimator>();
        pitchEstimator.frequencyMin = minPitchHz;
        pitchEstimator.frequencyMax = maxPitchHz;

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

        float target = Mathf.Clamp01(rms * sensitivity);
        if (target  < 0.05f){
            target = 0f;
        }

        loudness = target;

        if (loudness == 0f){
            pitchHz = 0f;
            return;
        }

    // SRH
        float detected = pitchEstimator.Estimate(audioSource);
        pitchHz = detected;
    }

    void OnDisable()
    {
        if (!string.IsNullOrEmpty(micDevice))
            Microphone.End(micDevice);
    }
}