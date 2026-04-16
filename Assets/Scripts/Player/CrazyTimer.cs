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

    public bool isSprinting = false;
    public AudioClip Crazy1;
    public AudioClip Crazy2;

    public float EffectTime = 30;
    public float TimeBeforeStart = 20;
    public float SprintEffectTime = 5f; 
    public float SprintRecoverySpeed = 3f;

    public EffectData lensDistortionData = new EffectData { startingValue = 0, maxValue = -1, speed = 1 };
    public float lensDistortionMinOscillationValue = -0.5f;
    public float lensDistortionMaxWhenSprinting = -0.6f;
    public EffectData lensFlareData = new EffectData { startingValue = 0, maxValue = 25, speed = 1 };
    public EffectData bloomData = new EffectData { startingValue = 0.6f, maxValue = 10, speed = 1 };
    public EffectData chromaticAberrationData = new EffectData { startingValue = 0.2f, maxValue = 1, speed = 1 };
    public EffectData whiteBalanceTempData = new EffectData { startingValue = -15, maxValue = 4, speed = 1 };
    public EffectData whiteBalanceTintData = new EffectData { startingValue = 0, maxValue = 16, speed = 1 };
    public EffectData vignetteData = new EffectData { startingValue = 0.4f, maxValue = 0.4f, speed = 1 };
    public EffectData motionBlurData = new EffectData { startingValue = 0.6f, maxValue = 1, speed = 1 };
    public EffectData contrastData = new EffectData { startingValue = 0, maxValue = -16, speed = 1 };
    public EffectData colourFilterRedData = new EffectData { startingValue = 1, maxValue = 0.5377358f, speed = 2 };
    public EffectData colourFilterGreenData = new EffectData { startingValue = 1, maxValue = 0.2764774f, speed = 2 };
    public EffectData colourFilterBlueData = new EffectData { startingValue = 1, maxValue = 0.2764774f, speed = 2 };
    public EffectData dirtIntensityData = new EffectData { startingValue = 0.1f, maxValue = 12, speed = 1 };
    public EffectData soundVolumeData = new EffectData { startingValue = 0, maxValue = 1, speed = 0.2f };
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
    
    private float baseTime = 0;
    private float sprintTimeOffset = 0;
    
    private AudioSource crazy1Source;
    private AudioSource crazy2Source;
    private bool soundPlaying;
    private float volumeTimeBeforeStart = 0;
    private float lensDistortionTimeBeforeStart = 0;

    void Start()
    {
        volumeTimeBeforeStart = 0;
        lensDistortionTimeBeforeStart = 0;

        volume = GameObject.Find("Global Volume").GetComponent<Volume>();
        volume.profile.TryGet(out bloom);
        volume.profile.TryGet(out motionBlur);
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out whiteBalance);
        volume.profile.TryGet(out chromaticAberration);
        volume.profile.TryGet(out lensDistortion);
        volume.profile.TryGet(out lensFlare);
        volume.profile.TryGet(out colourAdjustments);

        bloom.intensity.Override(bloomData.startingValue);
        lensDistortion.intensity.Override(lensDistortionData.startingValue);
        chromaticAberration.intensity.Override(chromaticAberrationData.startingValue);
        lensFlare.intensity.Override(lensFlareData.startingValue);
        whiteBalance.temperature.Override(whiteBalanceTempData.startingValue);
        whiteBalance.tint.Override(whiteBalanceTintData.startingValue);
        vignette.intensity.Override(vignetteData.startingValue);
        motionBlur.intensity.Override(motionBlurData.startingValue);
        colourAdjustments.contrast.Override(contrastData.startingValue);
        bloom.dirtIntensity.Override(dirtIntensityData.startingValue);
        
        colour.r = colourFilterRedData.startingValue;
        colour.g = colourFilterGreenData.startingValue;
        colour.b = colourFilterBlueData.startingValue;
        colour.a = 1;
        colourAdjustments.colorFilter.Override(colour);

        crazy1Source = gameObject.AddComponent<AudioSource>();
        crazy1Source.spatialBlend = 0f; 
        crazy1Source.loop = true;
        crazy1Source.clip = Crazy1;
        
        crazy2Source = gameObject.AddComponent<AudioSource>();
        crazy2Source.spatialBlend = 0f; 
        crazy2Source.loop = true;
        crazy2Source.clip = Crazy2;
    }

    void Update()
    {
        if (!playEffect) return;

        baseTime += Time.deltaTime;
        if (isSprinting)
        {
            if (baseTime + sprintTimeOffset < TimeBeforeStart)
                sprintTimeOffset = TimeBeforeStart - baseTime;

            float dynamicMultiplier = EffectTime / SprintEffectTime;
            sprintTimeOffset += Time.deltaTime * (dynamicMultiplier - 1f);
        }
        else if (sprintTimeOffset > 0)
        {
            float rewindSpeed = EffectTime / SprintRecoverySpeed;
            sprintTimeOffset = Mathf.MoveTowards(sprintTimeOffset, 0f, Time.deltaTime * rewindSpeed);
        }
        float time = baseTime + sprintTimeOffset;


        if (time < TimeBeforeStart && soundPlaying)
        {
            soundPlaying = false;
            crazy1Source.Stop();
            crazy2Source.Stop();
        }

        if (time >= TimeBeforeStart && !soundPlaying)
        {
            crazy1Source.volume = soundVolumeData.startingValue;
            crazy1Source.Play();
            crazy2Source.volume = soundVolumeData.startingValue;
            crazy2Source.Play();
            soundPlaying = true;
        }
        float percent = Mathf.Clamp01((time - TimeBeforeStart) / EffectTime);

        bloom.intensity.Override(calcValue(bloomData, percent));
        bloom.dirtIntensity.Override(calcValue(dirtIntensityData, percent));
        chromaticAberration.intensity.Override(calcValue(chromaticAberrationData, percent));
        lensFlare.intensity.Override(calcValue(lensFlareData, percent));
        whiteBalance.temperature.Override(calcValue(whiteBalanceTempData, percent));
        whiteBalance.tint.Override(calcValue(whiteBalanceTintData, percent));
        vignette.intensity.Override(calcValue(vignetteData, percent));
        motionBlur.intensity.Override(calcValue(motionBlurData, percent));
        
        colour.r = calcValue(colourFilterRedData, percent * colourFilterRedData.speed);
        colour.g = calcValue(colourFilterGreenData, percent * colourFilterGreenData.speed);
        colour.b = calcValue(colourFilterBlueData, percent * colourFilterBlueData.speed);
        colourAdjustments.colorFilter.Override(colour);

        float targetDistortion, distortionPercent;
        if (isSprinting || sprintTimeOffset > 0) distortionPercent = percent;
        else distortionPercent = percent * 1.75f;
        if (distortionPercent < 1 || isSprinting || sprintTimeOffset > 0)  // oscillation "bounce" doesnt look good when player stops sprinting
        {
            if (isSprinting || sprintTimeOffset > 0)
                targetDistortion = Mathf.Lerp(lensDistortionData.startingValue, lensDistortionMaxWhenSprinting, Mathf.Clamp01(distortionPercent));
            else 
                targetDistortion = calcValue(lensDistortionData, distortionPercent);
            lensDistortionTimeBeforeStart = 0;
        }
        else
        {
            if (lensDistortionTimeBeforeStart == 0) lensDistortionTimeBeforeStart = baseTime;
            float lensDistPercent = (Mathf.Cos((baseTime - lensDistortionTimeBeforeStart) * lensDistortionData.speed) + 1f) / 2f; 
            targetDistortion = Mathf.Lerp(lensDistortionMinOscillationValue, lensDistortionData.maxValue, lensDistPercent);
        }
        float smoothedDistortion = Mathf.MoveTowards(lensDistortion.intensity.value, targetDistortion, Time.deltaTime * 3f);
        lensDistortion.intensity.Override(smoothedDistortion);

        float targetVolume;
        if (percent < 1)
        {
            targetVolume = calcValue(soundVolumeData, percent);
            volumeTimeBeforeStart = 0;
        }
        else
        {
            if (volumeTimeBeforeStart == 0) volumeTimeBeforeStart = baseTime;
            float volumePercent = (Mathf.Cos((baseTime - volumeTimeBeforeStart) * soundVolumeData.speed) + 1f) / 2f; 
            targetVolume = Mathf.Lerp(soundVolumeMinOscillationValue, soundVolumeData.maxValue, volumePercent);
        }
        float smoothedVolume = Mathf.MoveTowards(crazy1Source.volume, targetVolume, Time.deltaTime * 3f);
        
        if (soundPlaying)
        {
            crazy1Source.volume = smoothedVolume;
            crazy2Source.volume = smoothedVolume;
            float pitchPercent = (Mathf.Cos(baseTime * 0.22f) + 1f) / 2f; 
            float newPitch = 0.7f + Mathf.Min(0.5f * pitchPercent, 0.5f);
            crazy1Source.pitch = newPitch;
            crazy2Source.pitch = newPitch;
        }
    }

    private float calcValue(EffectData effectData, float percent)
    {
        return Mathf.Lerp(effectData.startingValue, effectData.maxValue, Mathf.Clamp01(percent));
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
        baseTime = 0;
        sprintTimeOffset = 0;
        isSprinting = false;
        
        volumeTimeBeforeStart = 0;
        lensDistortionTimeBeforeStart = 0;
        
        if (soundPlaying)
        {
            StartCoroutine(FadeOut(crazy1Source, 0.7f));
            StartCoroutine(FadeOut(crazy2Source, 0.7f));
            soundPlaying = false;
        }
        
        bloom.intensity.Override(bloomData.startingValue);
        lensDistortion.intensity.Override(lensDistortionData.startingValue);
        chromaticAberration.intensity.Override(chromaticAberrationData.startingValue);
        lensFlare.intensity.Override(lensFlareData.startingValue);
        whiteBalance.temperature.Override(whiteBalanceTempData.startingValue);
        whiteBalance.tint.Override(whiteBalanceTintData.startingValue);
        vignette.intensity.Override(vignetteData.startingValue);
        motionBlur.intensity.Override(motionBlurData.startingValue);
        colourAdjustments.contrast.Override(contrastData.startingValue);
        bloom.dirtIntensity.Override(dirtIntensityData.startingValue);
        
        colour.r = colourFilterRedData.startingValue;
        colour.g = colourFilterGreenData.startingValue;
        colour.b = colourFilterBlueData.startingValue;
        colourAdjustments.colorFilter.Override(colour);
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