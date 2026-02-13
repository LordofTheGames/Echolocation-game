using UnityEngine;

public class OutlineTarget : MonoBehaviour
{
    [SerializeField] private string outlinedLayerName = "Outlined Object";
    
    public bool canBeOutlined = true; 
    
    private int outlinedLayer;
    private int originalLayer;

    private void Awake()
    {
        outlinedLayer = LayerMask.NameToLayer(outlinedLayerName);
        originalLayer = gameObject.layer;

        if (outlinedLayer != -1 && gameObject.layer == outlinedLayer)
            gameObject.layer = originalLayer;
    }

    // called every frame that player is looking at target
    public void SetOutlined(bool on)
    {
        if (!canBeOutlined) return; 

        if (outlinedLayer == -1) return;
        gameObject.layer = on ? outlinedLayer : originalLayer;
    }
}