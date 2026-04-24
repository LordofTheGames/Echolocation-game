using UnityEngine;

public class MiniMap : MonoBehaviour
{
    [Tooltip("The texture the Mask Camera renders to")]
    public RenderTexture currentBrushRT;
    
    [Tooltip("The new texture that saves the progress")]
    public RenderTexture permanentFogRT;
    
    [Tooltip("The Material set to Mobile/Particles/Additive")]
    public Material stampMaterial;

    void Start()
    {
        // Clear the permanent canvas to pure black exactly once at the start
        RenderTexture.active = permanentFogRT;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = null;
    }

    void LateUpdate()
    {
        if (currentBrushRT == null || permanentFogRT == null || stampMaterial == null) return;

        RenderTexture.active = permanentFogRT;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, permanentFogRT.width, permanentFogRT.height, 0);

        // Because the material is additive, black adds 0 (does nothing), and white adds 1 (stays white permanently).
        Graphics.DrawTexture(new Rect(0, 0, permanentFogRT.width, permanentFogRT.height), currentBrushRT, stampMaterial);

        GL.PopMatrix();
        RenderTexture.active = null;
    }
}