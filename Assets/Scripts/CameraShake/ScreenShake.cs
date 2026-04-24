using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private Breakable targetBreakable;
    private void OnEnable()
    {
        Breakable.OnBroken += TriggerShake;
    }

    private void OnDisable()
    {
        Breakable.OnBroken -= TriggerShake;
    }

    private void TriggerShake(Breakable broken)
    {
        if (broken != targetBreakable) return;

        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        float currentTime = 0;
        Vector3 originalPos = transform.localPosition;

        while (currentTime < duration)
        {
            float strength = curve.Evaluate(currentTime / duration);

            transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * strength;

            currentTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}