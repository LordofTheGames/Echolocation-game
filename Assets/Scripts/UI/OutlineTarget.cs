using UnityEngine;

public class OutlineTarget : MonoBehaviour
{
    [SerializeField] private string outlinedLayerName = "Outlined Object";
    
    // NEW: A simple switch to allow or prevent the outline visual!
    [SerializeField] public bool canBeOutlined = true; 
    
    private int outlinedLayer;
    private int originalLayer;

    private void Awake()
    {
        outlinedLayer = LayerMask.NameToLayer(outlinedLayerName);
        
        // FIXED: Now always saves the original layer safely
        originalLayer = gameObject.layer;

        if (outlinedLayer != -1 && gameObject.layer == outlinedLayer)
            gameObject.layer = originalLayer;
    }

    // called every frame that player is looking at target
    public void SetOutlined(bool on)
    {
        // NEW: If we turned off outlines for this specific object, do nothing.
        if (!canBeOutlined) return; 

        if (outlinedLayer == -1) return;
        gameObject.layer = on ? outlinedLayer : originalLayer;
    }
}