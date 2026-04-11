using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CrazyTimer : MonoBehaviour
{
    public float MaxTime = 40;
    public float TimeBeforeStart = 10;

    private Volume volume;
    private Bloom bloom;
    private MotionBlur motionBlur;
    private Vignette vignette;
    private WhiteBalance whiteBalance;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private ScreenSpaceLensFlare lensFlare;
    private DepthOfField depthOfField;

    private float time = 0;
    private float maxLensDistortion = 0;
    private float lensDistortionSpeed = 1f;
    private float maxLensFlare = 0;
    private float lensFlareSpeed = 1f;
    private float maxBloom = 0.6f;
    private float bloomSpeed = 1;
    private float maxChromaticAbberation = 0.2f;
    private float chromaticAberrationSpeed = 1;
    private float maxWBTemp = -15;
    private float wbTempSpeed = 1;
    private float maxWBTint = 0;
    private float wbTintSpeed = 1;
    private float maxVignette = 0.4f;
    private float vignetteSpeed = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MaxTime -= TimeBeforeStart;
        volume = GameObject.Find("Global Volume").GetComponent<Volume>(); 
        volume.profile.TryGet(out bloom);
        volume.profile.TryGet(out motionBlur);
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out whiteBalance);
        volume.profile.TryGet(out chromaticAberration);
        volume.profile.TryGet(out lensDistortion);
        volume.profile.TryGet(out lensFlare);
        volume.profile.TryGet(out depthOfField);
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time >= TimeBeforeStart)
        {
            float timePercent;
            if (time - TimeBeforeStart > MaxTime) timePercent = 1;
            else timePercent = (time - TimeBeforeStart) / MaxTime;

            float percent = (-Mathf.Cos((time - TimeBeforeStart) * 0.5f * bloomSpeed) + 1f) / 2f * timePercent;

            maxBloom = Mathf.Min(10 * percent * bloomSpeed, 10);
            maxLensDistortion = Mathf.Max(-0.8f * percent * lensDistortionSpeed, -0.8f);
            maxChromaticAbberation = Mathf.Min(1 * percent * chromaticAberrationSpeed, 1);
            maxLensFlare = Mathf.Min(30 * percent * lensFlareSpeed, 30);
            maxWBTemp = Mathf.Min(4 * percent * wbTempSpeed, 4);
            maxWBTint = Mathf.Min(16 * percent * wbTintSpeed, 16);
            maxVignette = Mathf.Min(0.5f * percent * vignetteSpeed, 0.5f);
        }
        bloom.intensity.value = maxBloom;
        lensDistortion.intensity.value = maxLensDistortion;
        chromaticAberration.intensity.value = maxChromaticAbberation;
        lensFlare.intensity.value = maxLensFlare;
        whiteBalance.temperature.value = maxWBTemp;
        whiteBalance.tint.value = maxWBTint;
        vignette.intensity.value = maxVignette;
    }
}
