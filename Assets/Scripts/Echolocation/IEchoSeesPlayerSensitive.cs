using UnityEngine;

// Public interface for informing Monster of the number of rays that hit the player when it echolocates
public interface IEchoSeesPlayerSensitive
{
    void OnMonsterEcholocation(int monsterSeenPlayerHits);
} 