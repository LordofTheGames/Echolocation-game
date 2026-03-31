using UnityEngine;

public class MonsterDifficultyManager : MonoBehaviour
{

    [System.Serializable]
    public struct MonsterSoundData 
    {
        public float MinWaitTime;
        public float MaxWaitTime;
        public float MoveSpeed;
        public float StoppingDistance;
        public float RadiusFromSound;
    }
    [System.Serializable]
    public struct EcholocationParams
    {
        [Tooltip("Echo Projection Uniformity (0 = clumped, 1 = uniform)")] public float uniformity;
        [Tooltip("Number of Rays")] public int numRays;
        [Tooltip("Angle of projection (0 = line, 60 = cone, 360 = sphere)")] public float angle;
        [Tooltip("Volume of sound (used for fading effect)")] public float visualVolume;
    }

    public float MoveSpeed_Patrol;
    public float TurnSpeed_Patrol;
    public float StoppingDistance_Patrol;
    public float MoveRadius_Patrol;
    public float MinWaitAfterMove_Patrol;
    public float MaxWaitAfterMove_Patrol;
    public EcholocationParams EcholocationParams_Patrol;

    public float MoveSpeed_Investigate;
    public float TurnSpeed_Investigate;
    public float StoppingDistance_Investigate;
    public float Repetitions_Investigate;
    public float RadiusFromStartPoint_Investigate;
    public float MinWaitBetweenRepetitions_Investigate;
    public float MaxWaitBetweenRepetitions_Investigate;
    public EcholocationParams EcholocationParams_Investigate;

    public float MaxQuietSound_Sound;
    public float MaxMediumSound_Sound;
    public EcholocationParams EcholocationParams_Sound;
    public MonsterSoundData QuietSounds = new MonsterSoundData {MinWaitTime = 0, MaxWaitTime = 0, MoveSpeed = 0, StoppingDistance = 0, RadiusFromSound = 0};
    public MonsterSoundData MediumSounds = new MonsterSoundData {MinWaitTime = 0, MaxWaitTime = 0, MoveSpeed = 0, StoppingDistance = 0, RadiusFromSound = 0};
    public MonsterSoundData LoudSounds = new MonsterSoundData {MinWaitTime = 0, MaxWaitTime = 0, MoveSpeed = 0, StoppingDistance = 0, RadiusFromSound = 0};

    public float MinSpeed_Chase;
    public float MaxSpeed_Chase;
    [Tooltip("Amount speed increases per 1/10th of a second")] public float SpeedIncrement_Chase;
    public float TurnSpeed_Chase;
    public float StoppingDistance_Chase;
    public float DistanceMonsterLosesPlayer_Chase;
    public EcholocationParams EcholocationParams_Chase;

    public float MoveSpeed_RunAway;
    public float TurnSpeed_RunAway;
    public float StoppingDistance_RunAway;
    // public EcholocationParams EcholocationParamsGrenade;
    // public EcholocationParams EcholocationParamsWaterfall;
    public EcholocationParams EcholocationParams_RunAway;

    public float GrenadeActivationDistance;
    public float DistanceRunFromGrenade;

    public float DistanceToTriggerInvestigation_Hiding;
    public float MaxTimeBeforeInvestigation_Hiding;
    public float MinInvestigationTime_Hiding;
    public float MaxInvestigationTime_Hiding;
    public float MinWaitBeforeGotoSound_Hiding;
    public float MaxWaitBeforeGotoSound_Hiding;
    public float PatrolRadius_Hiding;
    public float MoveSpeedGotoSound_Hiding;
    public float TurnSpeedGotoSound_Hiding;
    public float StoppingDistanceGotoSound_Hiding;
    public float MoveSpeedPatrol_Hiding;
    public float TurnSpeedPatrol_Hiding;
    public float StoppingDistancePatrol_Hiding;
    // public EcholocationParams EcholocationParamsInvestigating_Hiding;
    // public EcholocationParams EcholocationParamsGotoSound_Hiding;
    // public EcholocationParams EcholocationParamsSeenPlayer_Hiding;
    public EcholocationParams EcholocationParams_Hiding;

    public float MaxDistanceFromPlayer;
    public float DistanceToMoveToIfFarFromPlayer;
    public float MoveSpeedFarFromPlayer;
    public float StoppingDistanceFarFromPlayer;

    public float PlayerSeenScreamVolume;
    public float GrenadeScreamVolume;
    public float WaterfallScreamVolume;
    public float HeardSoundGruntVolume;
    public float ClickingVolume;
    public float GrowlVolume;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
