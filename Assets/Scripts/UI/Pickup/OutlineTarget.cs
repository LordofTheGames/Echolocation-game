using System.Collections.Generic;
using UnityEngine;

public class OutlineTarget : MonoBehaviour
{
    [SerializeField] private string outlinedLayerName = "Outlined Object";
    [SerializeField] private bool affectChildren = true;

    private int outlinedLayer = -1;

    private struct Entry
    {
        public Transform t;
        public int originalLayer;
    }

    private readonly List<Entry> cache = new();

    private void OnEnable()
    {
        outlinedLayer = LayerMask.NameToLayer(outlinedLayerName);
        if (outlinedLayer == -1) return;

        RebuildCache();
    }

    private void RebuildCache()
    {
        cache.Clear();

        if (!affectChildren)
        {
            cache.Add(new Entry { t = transform, originalLayer = gameObject.layer });
            return;
        }

        var all = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            cache.Add(new Entry { t = all[i], originalLayer = all[i].gameObject.layer });
        }
    }

    public void SetOutlined(bool on)
    {
        if (outlinedLayer == -1) return;

        if (cache.Count == 0) RebuildCache();

        for (int i = 0; i < cache.Count; i++)
        {
            var t = cache[i].t;
            if (!t) continue;

            t.gameObject.layer = on ? outlinedLayer : cache[i].originalLayer;
        }
    }
}

