using Unity.VisualScripting;
using UnityEngine;

public class HearingChecker : MonoBehaviour, INoiseSensitive, IEchoSeesPlayerSensitive
{
    public float maxSoundDistance = 30f;
    public float AgentEyeHeight = 3.9f;
    public LayerMask ObstructionMask;
    public bool ShowDebugVisuals;

    private SoundData newSource;
    private bool newSound = false;
    private float newMaxDist = 0;

    // This function is called automatically by the Scanner when rays hit the monster
    public void OnHeardScan(Vector3 source, float volume, bool isFootsteps, float priority)
    {
        if (source == null) return;
        if (volume <= 0) return;

        Vector3 eyePos = transform.position + Vector3.up * AgentEyeHeight;
        float distance = Vector3.Distance(eyePos, source);
        // TODO: this calculation is fairly arbritrary! improve it?
        newMaxDist = maxSoundDistance + (volume / 15f);
        if (distance <= newMaxDist)
        {
            bool viewObstructed = Physics.Raycast(eyePos, source - eyePos, distance, ObstructionMask);
            // TODO: this calculation is fairly arbritrary! improve it?
            if (viewObstructed) newMaxDist *= 0.75f;  

            if (distance <= newMaxDist){
                newSound = true;
                newSource.location = source;
                newSource.volume = volume;
                newSource.isFootsteps = isFootsteps;
            }
        }
    }

    public void OnMonsterEcholocation(int monsterSeenPlayerHits)
    {
        // Debug.Log("Monster seen player hits: " + monsterSeenPlayerHits);
    }

    // called every frame by monster behaviour tree to check for new sounds
    public SoundData? HearingCheck()
    {
        if (!newSound) return null;
        newSound = false;
        return newSource;
    }

    private void OnDrawGizmos()
    {
        if (ShowDebugVisuals)
        {
            if (newMaxDist == 0) newMaxDist = maxSoundDistance;
            Vector3 eyePos = transform.position + Vector3.up * AgentEyeHeight;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(eyePos, newMaxDist);
        }
    }
}
