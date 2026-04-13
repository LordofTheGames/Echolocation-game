using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CrazyTimer : MonoBehaviour
{
    [Serializable]
    public struct EffectData
    {
        public float startingValue;
        public float maxValue;
        public float speed;
    }

    public AudioClip Crazy1;
    public AudioClip Crazy2;

    public float MaxTime = 50;
    public float TimeBeforeStart = 20;

    public EffectData lensDistortionData = new EffectData{startingValue = 0, maxValue = -1, speed = 1};
    public float lensDistortionMinOscillationValue = -0.5f;
    public EffectData lensFlareData = new EffectData{startingValue = 0, maxValue = 25, speed = 1};
    public EffectData bloomData = new EffectData{startingValue = 0.6f, maxValue = 10, speed = 1};
    public EffectData chromaticAberrationData = new EffectData{startingValue = 0.2f, maxValue = 1, speed = 1};
    public EffectData whiteBalanceTempData = new EffectData{startingValue = -15, maxValue = 4, speed = 1};
    public EffectData whiteBalanceTintData = new EffectData{startingValue = 0, maxValue = 16, speed = 1};
    public EffectData vignetteData = new EffectData{startingValue = 0.4f, maxValue = 0.4f, speed = 1};
    public EffectData motionBlurData = new EffectData{startingValue = 0.6f, maxValue = 1, speed = 1};
    public EffectData contrastData = new EffectData{startingValue = 0, maxValue = -16, speed = 1};
    public EffectData colourFilterRedData = new EffectData{startingValue = 1, maxValue = 0.5377358f, speed = 2};
    public EffectData colourFilterGreenData = new EffectData{startingValue = 1, maxValue = 0.2764774f, speed = 2};
    public EffectData colourFilterBlueData = new EffectData{startingValue = 1, maxValue = 0.2764774f, speed = 2};
    public EffectData dirtIntensityData = new EffectData{startingValue = 0, maxValue = 12, speed = 1};
    public EffectData soundVolumeData = new EffectData{startingValue = 0, maxValue = 1, speed = 0.2f};
    public float soundVolumeMinOscillationValue = 0.5f;

    private Volume volume;
    private Bloom bloom;
    private MotionBlur motionBlur;
    private Vignette vignette;
    private WhiteBalance whiteBalance;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private ScreenSpaceLensFlare lensFlare;
    private ColorAdjustments colourAdjustments;
    private Color colour;
    private bool playEffect = false;

    private float time = 0;
    private AudioSource crazy1Source; 
    private AudioSource crazy2Source; 
    private bool soundPlaying;
    private float volumeTimeBeforeStart = 0;
    private float lensDistortionTimeBeforeStart = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MaxTime -= TimeBeforeStart;
        volumeTimeBeforeStart = 0;
        lensDistortionTimeBeforeStart = 0;

        bloomData.maxValue -= bloomData.startingValue;
        lensDistortionData.maxValue -= lensDistortionData.startingValue;
        chromaticAberrationData.maxValue -= chromaticAberrationData.startingValue;
        lensFlareData.maxValue -= lensFlareData.startingValue;
        whiteBalanceTempData.maxValue -= whiteBalanceTempData.startingValue;
        whiteBalanceTintData.maxValue -= whiteBalanceTintData.startingValue;
        vignetteData.maxValue -= vignetteData.startingValue;
        contrastData.maxValue -= contrastData.startingValue;
        motionBlurData.maxValue -= motionBlurData.startingValue;
        colourFilterRedData.maxValue -= colourFilterRedData.startingValue;
        colourFilterBlueData.maxValue -= colourFilterBlueData.startingValue;
        colourFilterGreenData.maxValue -= colourFilterGreenData.startingValue;
        dirtIntensityData.maxValue -= dirtIntensityData.startingValue;

        volume = GameObject.Find("Global Volume").GetComponent<Volume>(); 
        volume.profile.TryGet(out bloom);
        volume.profile.TryGet(out motionBlur);
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out whiteBalance);
        volume.profile.TryGet(out chromaticAberration);
        volume.profile.TryGet(out lensDistortion);
        volume.profile.TryGet(out lensFlare);
        volume.profile.TryGet(out colourAdjustments);

        bloom.intensity.value = bloomData.startingValue;
        lensDistortion.intensity.value = lensDistortionData.startingValue;
        chromaticAberration.intensity.value = chromaticAberrationData.startingValue;
        lensFlare.intensity.value = lensFlareData.startingValue;
        whiteBalance.temperature.value = whiteBalanceTempData.startingValue;
        whiteBalance.tint.value = whiteBalanceTintData.startingValue;
        vignette.intensity.value = vignetteData.startingValue; 
        motionBlur.intensity.value = motionBlurData.startingValue; 
        colourAdjustments.contrast.value = contrastData.startingValue;
        colour.r = colourFilterRedData.startingValue;
        colour.g = colourFilterGreenData.startingValue;
        colour.b = colourFilterBlueData.startingValue;
        colour.a = 1; 
        colourAdjustments.colorFilter.value = colour;
        bloom.dirtIntensity.value = dirtIntensityData.startingValue;

        crazy1Source = gameObject.AddComponent<AudioSource>();
        crazy1Source.spatialBlend = 0f; // Heard equally in both ears since it's coming from the player
        crazy1Source.loop = true;
        crazy1Source.clip = Crazy1;
        crazy2Source = gameObject.AddComponent<AudioSource>();
        crazy2Source.spatialBlend = 0f; // Heard equally in both ears since it's coming from the player
        crazy2Source.loop = true;
        crazy2Source.clip = Crazy2;
    }

    void Update()
    {
        if (!playEffect) return;

        time += Time.deltaTime;
        if (time >= TimeBeforeStart)
        {
            if (!soundPlaying)
            {
                crazy1Source.volume = soundVolumeData.startingValue;
                crazy1Source.Play();
                crazy2Source.volume = soundVolumeData.startingValue;
                crazy2Source.Play();
                soundPlaying = true;
            }

            float percent;
            if (time - TimeBeforeStart > MaxTime) percent = 1;
            else percent = (time - TimeBeforeStart) / MaxTime;

            bloom.intensity.value = calcValue(bloomData, percent);
            bloom.dirtIntensity.value = calcValue(dirtIntensityData, percent);
            chromaticAberration.intensity.value = calcValue(chromaticAberrationData, percent);
            lensFlare.intensity.value = calcValue(lensFlareData, percent);
            whiteBalance.temperature.value = calcValue(whiteBalanceTempData, percent);
            whiteBalance.tint.value = calcValue(whiteBalanceTintData, percent);
            vignette.intensity.value = calcValue(vignetteData, percent);
            motionBlur.intensity.value = calcValue(motionBlurData, percent); 
            colour.r = calcValue(colourFilterRedData, percent * colourFilterRedData.speed);
            colour.g = calcValue(colourFilterGreenData, percent * colourFilterGreenData.speed);
            colour.b = calcValue(colourFilterBlueData, percent * colourFilterBlueData.speed);
            colourAdjustments.colorFilter.value = colour;

            if (percent * 1.75f < 1)
            {
                lensDistortion.intensity.value = calcValue(lensDistortionData, percent * 1.75f);
            }
            else
            {
                if (lensDistortionTimeBeforeStart == 0) lensDistortionTimeBeforeStart = time;
                float lensDistPercent = (Mathf.Cos((time - lensDistortionTimeBeforeStart) * lensDistortionData.speed) + 1f) / 2f; // start at 1
                float newMaxVal = lensDistortionData.maxValue - (lensDistortionMinOscillationValue - lensDistortionData.startingValue);
                lensDistortion.intensity.value = lensDistortionMinOscillationValue + Mathf.Max(newMaxVal * lensDistPercent, newMaxVal);
            }

            float newVolume;
            if (percent < 1)
            {
                newVolume = calcValue(soundVolumeData, percent);
            }
            else
            {
                if (volumeTimeBeforeStart == 0) volumeTimeBeforeStart = time;
                float volumePercent = (Mathf.Cos((time - volumeTimeBeforeStart) * soundVolumeData.speed) + 1f) / 2f; // start at 1
                float newMaxVal = soundVolumeData.maxValue - (soundVolumeMinOscillationValue - soundVolumeData.startingValue);
                newVolume = soundVolumeMinOscillationValue + Mathf.Min(newMaxVal * volumePercent, newMaxVal);
            }
            crazy1Source.volume = newVolume;
            crazy2Source.volume = newVolume;
            float pitchPercent = (Mathf.Cos(time * 0.22f) + 1f) / 2f; // start at 1
            float newPitch = 0.7f + Mathf.Min(0.5f * pitchPercent, 0.5f);
            crazy1Source.pitch = newPitch;
            crazy2Source.pitch = newPitch;
        }
    }

    private float calcValue(EffectData effectData, float percent)
    {
        if (effectData.maxValue >= effectData.startingValue)
            return effectData.startingValue + Mathf.Min(effectData.maxValue * percent, effectData.maxValue);
        else
            return effectData.startingValue + Mathf.Max(effectData.maxValue * percent, effectData.maxValue);
    }

	private IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            
            audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            
            yield return null; 
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    public void ResetEffect()
    {
        time = 0;
        volumeTimeBeforeStart = 0;
        lensDistortionTimeBeforeStart = 0;
        if (soundPlaying)
        {
            StartCoroutine(FadeOut(crazy1Source, 0.7f));
            StartCoroutine(FadeOut(crazy2Source, 0.7f));
            soundPlaying = false;
        }
        bloom.intensity.value = bloomData.startingValue;
        lensDistortion.intensity.value = lensDistortionData.startingValue;
        chromaticAberration.intensity.value = chromaticAberrationData.startingValue;
        lensFlare.intensity.value = lensFlareData.startingValue;
        whiteBalance.temperature.value = whiteBalanceTempData.startingValue;
        whiteBalance.tint.value = whiteBalanceTintData.startingValue;
        vignette.intensity.value = vignetteData.startingValue; 
        motionBlur.intensity.value = motionBlurData.startingValue; 
        colourAdjustments.contrast.value = contrastData.startingValue;
        colour.r = colourFilterRedData.startingValue;
        colour.g = colourFilterGreenData.startingValue;
        colour.b = colourFilterBlueData.startingValue;
        colourAdjustments.colorFilter.value = colour;
        bloom.dirtIntensity.value = dirtIntensityData.startingValue;
    }

    public void FadeOutSounds()
    {
        if (soundPlaying)
        {
            StartCoroutine(FadeOut(crazy1Source, 0.7f));
            StartCoroutine(FadeOut(crazy2Source, 0.7f));
            soundPlaying = false;
        }
    }

    public void StartEffect()
    {
        ResetEffect();
        playEffect = true;
    }

    public void StopEffect()
    {
        playEffect = false;
    }
}
