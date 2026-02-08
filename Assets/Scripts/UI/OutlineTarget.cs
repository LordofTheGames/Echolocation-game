using UnityEngine;

public class OutlineTarget : MonoBehaviour
{
    [SerializeField] private string outlinedLayerName = "Outlined Object";

    private int outlinedLayer;
    private int originalLayer;

    private void Awake()
    {
        outlinedLayer = LayerMask.NameToLayer(outlinedLayerName);
        if (outlinedLayer == -1)
        originalLayer = gameObject.layer;

        if (outlinedLayer != -1 && gameObject.layer == outlinedLayer)
            gameObject.layer = originalLayer;
    }

    // called every frame that player is looking at target
    public void SetOutlined(bool on)
    {
        if (outlinedLayer == -1) return;
        gameObject.layer = on ? outlinedLayer : originalLayer;
    }
}