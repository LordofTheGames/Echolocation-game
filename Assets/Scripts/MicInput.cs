using UnityEngine;

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
    private float minPitchHz = 60f;
    private float maxPitchHz = 2000f;
    private FFTWindow fftWindow = FFTWindow.BlackmanHarris;
    private float[] spectrum;
    private int sampleRate;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;

        if (Microphone.devices.Length == 0){
            Debug.LogError("no microphone device");
            enabled = false;
            return;
        }

        micDevice = Microphone.devices[0];
        audioSource.clip = Microphone.Start(micDevice, true,1, sampleRate);

        audioSource.Play();

        samples = new float[windowSize];
        spectrum = new float[spectrumSize];
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

        audioSource.GetSpectrumData(spectrum, 0, fftWindow);

        int minBin = Mathf.FloorToInt(minPitchHz * spectrumSize / (float)sampleRate);
        int maxBin = Mathf.CeilToInt(maxPitchHz * spectrumSize / (float)sampleRate);

        int half = spectrumSize / 2;
        minBin = Mathf.Clamp(minBin, 1, half- 1);
        maxBin = Mathf.Clamp(maxBin,minBin+ 1, half -1);

        int best = minBin;
        float bestVal = 0f;
        for (int i = minBin; i <= maxBin; i++){
            float v = spectrum[i];
            if (v > bestVal){
                bestVal = v;
                best = i;
            }
        }

        int from = Mathf.Max(best- 2, minBin);
        int to = Mathf.Min(best+ 2, maxBin);

        float weightedSum = 0f;
        float energySum = 0f;

        for (int i = from; i <= to; i++){
            float freq = i * (float)sampleRate / spectrumSize;
            float w = spectrum[i];
            weightedSum += freq * w;
            energySum += w;
        }

        if (energySum > 0f)
            pitchHz = weightedSum / energySum;
        else
            pitchHz = 0f;
    }

    void OnDisable()
    {
        if (!string.IsNullOrEmpty(micDevice))
            Microphone.End(micDevice);
    }
}