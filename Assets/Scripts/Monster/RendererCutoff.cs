using UnityEngine;

public class RendererCutoff : MonoBehaviour
{
    public Transform player;
    // public float cutoffDistance = 50f;

    public float fadeStartDistance = 40f; // Start dissolving
    public float fadeEndDistance = 50f;   // Fully dissolved (invisible)

    private Material mat;
    private MeshRenderer meshRenderer;
    private float currentCutoff;
    private static readonly int CutoffID = Shader.PropertyToID("_Cutoff");

    void Start()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        mat = meshRenderer.material;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        float targetCutoff;

        // Calculate the Cutoff value (0 = fully visible, 1 = fully invisible)
        if (distance > fadeEndDistance)
        {
            targetCutoff = 1.1f; // Go slightly above 1 to ensure full invisibility
        }
        else if (distance < fadeStartDistance)
        {
            targetCutoff = 0f; // Fully visible
        }
        else
        {
            // Map distance to 0-1 range
            float range = fadeEndDistance - fadeStartDistance;
            float distanceIntoRange = distance - fadeStartDistance;
            targetCutoff = distanceIntoRange / range;
        }

        // Apply Cutoff to the material
        // We only update if the value has changed significantly to save processing
        if (Mathf.Abs(currentCutoff - targetCutoff) > 0.01f)
        {
            currentCutoff = targetCutoff;
            mat.SetFloat(CutoffID, currentCutoff);
            
            // Optimization: Completely disable the renderer if fully dissolved
            if (currentCutoff >= 1.0f && meshRenderer.enabled)
                meshRenderer.enabled = false;
            else if (currentCutoff < 1.0f && !meshRenderer.enabled)
                meshRenderer.enabled = true;
        }

        // float offsetSqr = (player.position - transform.position).sqrMagnitude;
        // if (offsetSqr > cutoffDistance * cutoffDistance)
        // {
        //     if (meshRenderer.enabled) meshRenderer.enabled = false;
        // }
        // else
        // {
        //     if (!meshRenderer.enabled) meshRenderer.enabled = true;
        // }
    }
}
