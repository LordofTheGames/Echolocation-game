using System;
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

    public float MaxTime = 50;
    public float TimeBeforeStart = 20;

    public EffectData lensDistortionData = new EffectData{startingValue = 0, maxValue = -0.8f, speed = 1};
    public EffectData lensFlareData = new EffectData{startingValue = 0, maxValue = 30, speed = 1};
    public EffectData bloomData = new EffectData{startingValue = 0.6f, maxValue = 10, speed = 1};
    public EffectData chromaticAberrationData = new EffectData{startingValue = 0.2f, maxValue = 1, speed = 1};
    public EffectData whiteBalanceTempData = new EffectData{startingValue = -15, maxValue = 4, speed = 1};
    public EffectData whiteBalanceTintData = new EffectData{startingValue = 0, maxValue = 16, speed = 1};
    public EffectData vignetteData = new EffectData{startingValue = 0.4f, maxValue = 0.4f, speed = 1};
    public EffectData motionBlurData = new EffectData{startingValue = 0.6f, maxValue = 1, speed = 1};
    public EffectData contrastData = new EffectData{startingValue = 0, maxValue = -24, speed = 1};
    public EffectData colourFilterRedData = new EffectData{startingValue = 1, maxValue = 0.5377358f, speed = 1};
    public EffectData colourFilterGreenData = new EffectData{startingValue = 1, maxValue = 0.2764774f, speed = 1};
    public EffectData colourFilterBlueData = new EffectData{startingValue = 1, maxValue = 0.2764774f, speed = 1};
    public EffectData dirtIntensityData = new EffectData{startingValue = 0, maxValue = 20, speed = 1};

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

    private float time = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MaxTime -= TimeBeforeStart;

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

    }

    void Update()
    {
        time += Time.deltaTime;
        if (time >= TimeBeforeStart)
        {
            float timePercent;
            if (time - TimeBeforeStart > MaxTime) timePercent = 1;
            else timePercent = (time - TimeBeforeStart) / MaxTime;

            // float percent = (-Mathf.Cos((time - TimeBeforeStart) * 0.5f) + 1f) / 2f * timePercent;
            float percent = timePercent;

            bloom.intensity.value = calcValue(bloomData, percent);
            bloom.dirtIntensity.value = calcValue(dirtIntensityData, percent);
            lensDistortion.intensity.value = calcValue(lensDistortionData, percent);
            chromaticAberration.intensity.value = calcValue(chromaticAberrationData, percent);
            lensFlare.intensity.value = calcValue(lensFlareData, percent);
            whiteBalance.temperature.value = calcValue(whiteBalanceTempData, percent);
            whiteBalance.tint.value = calcValue(whiteBalanceTintData, percent);
            vignette.intensity.value = calcValue(vignetteData, percent);
            motionBlur.intensity.value = calcValue(motionBlurData, percent); 
            colour.r = calcValue(colourFilterRedData, percent);
            colour.g = calcValue(colourFilterGreenData, percent);
            colour.b = calcValue(colourFilterBlueData, percent);
            colourAdjustments.colorFilter.value = colour;
        }
    }

    private float calcValue(EffectData effectData, float percent)
    {
        if (effectData.maxValue >= effectData.startingValue)
            return effectData.startingValue + Mathf.Min(effectData.maxValue * percent, effectData.maxValue);
        else
            return effectData.startingValue + Mathf.Max(effectData.maxValue * percent, effectData.maxValue);
    }

    public void ResetEffect()
    {
        time = 0;
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
    }
}
