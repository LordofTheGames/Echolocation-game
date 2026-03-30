using UnityEngine;

public class MonsterDifficultyManager : MonoBehaviour
{

    [System.Serializable]
    public struct MonsterSoundData 
    {
        public float MinWaitTime;
        public float MaxWaitTime;
        public float MoveSpeed;
        public float TurnSpeed;
        public float StoppingDistance;
        public float RadiusFromSound;
    }

    public float MoveSpeed_Patrol;
    public float TurnSpeed_Patrol;
    public float StoppingDistance_Patrol;
    public float MoveRadius_Patrol;
    public float MinWaitAfterMove_Patrol;
    public float MaxWaitAfterMove_Patrol;

    public float MoveSpeed_Investigate;
    public float TurnSpeed_Investigate;
    public float StoppingDistance_Investigate;
    public float Repetitions_Investigate;
    public float RadiusFromStartPoint_Investigate;
    public float MinWaitBetweenRepetitions_Investigate;
    public float MaxWaitBetweenRepetitions_Investigate;

    public float MaxQuietSound_Sound;
    public float MaxMediumSound_Sound;
    public MonsterSoundData QuietSounds = new MonsterSoundData {MinWaitTime = 0, MaxWaitTime = 0, MoveSpeed = 0, TurnSpeed = 0, StoppingDistance = 0, RadiusFromSound = 0};
    public MonsterSoundData MediumSounds = new MonsterSoundData {MinWaitTime = 0, MaxWaitTime = 0, MoveSpeed = 0, TurnSpeed = 0, StoppingDistance = 0, RadiusFromSound = 0};
    public MonsterSoundData LoudSounds = new MonsterSoundData {MinWaitTime = 0, MaxWaitTime = 0, MoveSpeed = 0, TurnSpeed = 0, StoppingDistance = 0, RadiusFromSound = 0};

    public float MinSpeed_Chase;
    public float MaxSpeed_Chase;
    [Tooltip("Amount speed increases per second")] public float SpeedIncrement_Chase;
    public float TurnSpeed_Chase;
    public float StoppingDistance_Chase;

    public float MoveSpeed_RunAway;
    public float TurnSpeed_RunAway;
    public float StoppingDistance_RunAway;

    public float DistanceRunFromGrenade;

    public float DistanceToTriggerInvestigation_Hiding;
    public float MaxTimeBeforeInvestigation_Hiding;
    public float MinInvestigationTime_Hiding;
    public float MaxInvestigationTime_Hiding;
    public float MinWaitBeforeGotoSound_Hiding;
    public float MaxWaitBeforeGotoSound_Hiding;

    public float MaxDistanceFromPlayer;
    public float DistanceToMoveToIfFarFromPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
