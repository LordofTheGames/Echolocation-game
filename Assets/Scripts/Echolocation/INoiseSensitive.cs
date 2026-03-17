using UnityEngine;

// Public interface for informing Monster of echo/sound rays that hit it and the source they come from
public interface INoiseSensitive
{
    void OnHeardScan(Transform source, float volume, bool isFootsteps = false, float priority = 1);
}
