using UnityEngine;

public class MonsterDifficultyManager : MonoBehaviour
{
    public NewDifficultyVars NewVarsEvent;
    [Header("Click this to update variables in behaviour tree manually")]
    public bool UpdateVariables;

    [System.Serializable]
    public struct MonsterSoundData 
    {
        [Tooltip("Min time monster waits between hearing and starting to investigate")]
        public float MinWaitTime;
        [Tooltip("Max time monster waits between hearing and starting to investigate")]
        public float MaxWaitTime;
        public float MoveSpeed;
        public float StoppingDistance;
        [Tooltip("Once monster has heard noise, it will travel near (not exactly where noise was). This is how near it travels")]
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

    // ----------------- Patrol Variables -----------------
    [Header("Patrol Variables")]
    public float MoveSpeed_Patrol = 6;
    public float TurnSpeed_Patrol = 5;
    public float StoppingDistance_Patrol = 2;
    [Tooltip("When picking a new location to patrol to, how far from current position to look")]
    public float MoveRadius_Patrol = 20;
    public float MinWaitAfterMove_Patrol = 0;
    public float MaxWaitAfterMove_Patrol = 1;
    public EcholocationParams EcholocationParams_Patrol = new EcholocationParams {uniformity = 0, numRays = 20000, angle = 100, visualVolume = 100};

    // -------------- Investigate Variables ---------------
    [Header("Investigate Variables")]
    public float MoveSpeed_Investigate = 8;
    public float TurnSpeed_Investigate = 5;
    public float StoppingDistance_Investigate = 2;
    [Tooltip("For investigation, monster picks a location near point of interest and goes there. This is number of times that is repeated")]
    public float Repetitions_Investigate = 4;
    [Tooltip("When picking a new location near point of interest, how far from point of interest to look")]
    public float RadiusFromStartPoint_Investigate = 10;
    public float MinWaitBetweenRepetitions_Investigate = 1;
    public float MaxWaitBetweenRepetitions_Investigate = 1.5f;
    public EcholocationParams EcholocationParams_Investigate = new EcholocationParams {uniformity = 0, numRays = 20000, angle = 100, visualVolume = 100};

    // ----------------- Sound Variables ------------------
    [Header("Sound Variables")]
    [Tooltip("Threshold for what is considered a quiet sound (above is medium)")]
    public float MaxQuietSound_Sound = 35;
    [Tooltip("Threshold for what is considered a monster sound (above is loud)")]
    public float MaxMediumSound_Sound = 75;
    public EcholocationParams EcholocationParams_Sound = new EcholocationParams {uniformity = 0, numRays = 20000, angle = 100, visualVolume = 100};
    public MonsterSoundData QuietSounds = new MonsterSoundData {MinWaitTime = 1, MaxWaitTime = 3, MoveSpeed = 4, StoppingDistance = 2, RadiusFromSound = 8};
    public MonsterSoundData MediumSounds = new MonsterSoundData {MinWaitTime = 0.2f, MaxWaitTime = 1, MoveSpeed = 7, StoppingDistance = 2, RadiusFromSound = 5};
    public MonsterSoundData LoudSounds = new MonsterSoundData {MinWaitTime = 0, MaxWaitTime = 0, MoveSpeed = 9, StoppingDistance = 2, RadiusFromSound = 2};

    // ----------------- Chase Variables ------------------
    [Header("Chase Variables")]
    public float MinSpeed_Chase = 10;
    public float MaxSpeed_Chase = 30;
    [Tooltip("Amount speed increases per 1/10th of a second (when chasing)")] 
    public float SpeedIncrement_Chase = 0.18f;
    public float TurnSpeed_Chase = 10;
    public float StoppingDistance_Chase = 2;
    [Tooltip("Once monster has seen player, monster is locked on unless player gets this distance away")] 
    public float DistanceMonsterLosesPlayer_Chase = 15;
    public EcholocationParams EcholocationParams_Chase = new EcholocationParams {uniformity = 0, numRays = 20000, angle = 100, visualVolume = 100};

    // ---------------- Run Away Variables ----------------
    [Header("Run Away Variables")]
    public float MoveSpeed_RunAway = 13;
    public float TurnSpeed_RunAway = 5;
    public float StoppingDistance_RunAway = 2;
    // public EcholocationParams EcholocationParamsGrenade;
    // public EcholocationParams EcholocationParamsWaterfall;
    public EcholocationParams EcholocationParams_RunAway = new EcholocationParams {uniformity = 0, numRays = 20000, angle = 100, visualVolume = 100};

    // ---------------- Grenade Variables -----------------
    [Header("Grenade Variables")]
    [Tooltip("How close the grenade has to be when it explodes for monster to react")] 
    public float GrenadeActivationDistance = 15;
    [Tooltip("How far away the monster runs from player when grenade is thrown")] 
    public float DistanceRunFromGrenade = 50;

    // ----------------- Hiding Variables -----------------
    [Header("Hiding Variables")]
    [Tooltip("If the monster is this close when the player hides, the monster will come to investigate straight away")] 
    public float DistanceToTriggerInvestigation_Hiding = 15;
    [Tooltip("If the monster was not close enough when player hid, time before monster comes to investigate")] 
    public float MaxTimeBeforeInvestigation_Hiding = 15;
    [Tooltip("Min duration of investigation around hiding hole")] 
    public float MinInvestigationTime_Hiding = 10;
    [Tooltip("Max duration of investigation around hiding hole")] 
    public float MaxInvestigationTime_Hiding = 18;
    [Tooltip("Min time monster waits between hearing a sound and going to sound (when player is hiding)")]
    public float MinWaitBeforeGotoSound_Hiding = 0.5f;
    [Tooltip("Max time monster waits between hearing a sound and going to sound (when player is hiding)")]
    public float MaxWaitBeforeGotoSound_Hiding = 2;
    public float MoveSpeedGotoSound_Hiding = 6;
    public float TurnSpeedGotoSound_Hiding = 5;
    public float StoppingDistanceGotoSound_Hiding = 0.1f;
    [Tooltip("When picking a new location to move to (while investigating hiding), how far from current position to look")]
    public float PatrolRadius_Hiding = 7;
    public float MoveSpeedPatrol_Hiding = 4;
    public float TurnSpeedPatrol_Hiding = 5;
    public float StoppingDistancePatrol_Hiding = 2;
    // public EcholocationParams EcholocationParamsInvestigating_Hiding;
    // public EcholocationParams EcholocationParamsGotoSound_Hiding;
    // public EcholocationParams EcholocationParamsSeenPlayer_Hiding;
    public EcholocationParams EcholocationParams_Hiding = new EcholocationParams {uniformity = 0, numRays = 20000, angle = 100, visualVolume = 100};

    // ------------ Far From Player Variables -------------
    [Header("Far From Player Variables")]
    [Tooltip("Max distance monster has to be from player for monster to be considered \"Far\" from player")]
    public float MaxDistanceFromPlayer = 45;
    [Tooltip("If monster is \"Far\" from player, how close towards player it will move")]
    public float DistanceToMoveToIfFarFromPlayer = 35;
    public float MoveSpeedFarFromPlayer = 12;
    public float StoppingDistanceFarFromPlayer = 2;

    // ---------- Sound Effect Volume Variables -----------
    [Header("Sound Effect Volume Variables")]
    public float PlayerSeenScreamVolume = 1;
    public float GrenadeScreamVolume = 1;
    public float WaterfallScreamVolume = 1;
    public float HeardSoundGruntVolume = 1;
    public float ClickingVolume = 1;
    public float GrowlVolume = 1;

    void Start()
    {
        UpdateVariables = false;
        NewVarsEvent.SendEventMessage();
    }

    public void OnPlayerDeath()
    {
        return;
        // ----------------- Patrol Variables -----------------
        MoveSpeed_Patrol -= 1;
        TurnSpeed_Patrol -= 1;
        StoppingDistance_Patrol += 1;
        // MoveRadius_Patrol -= 1;
        MinWaitAfterMove_Patrol += 1;
        MaxWaitAfterMove_Patrol += 1;
        // EcholocationParams_Patrol.uniformity -= 1;
        // EcholocationParams_Patrol.numRays -= 1;
        // EcholocationParams_Patrol.angle -= 1;
        // EcholocationParams_Patrol.visualVolume -= 1;

        // -------------- Investigate Variables ---------------
        MoveSpeed_Investigate -= 1;
        TurnSpeed_Investigate -= 1;
        StoppingDistance_Investigate += 1;
        Repetitions_Investigate -= 1;
        // RadiusFromStartPoint_Investigate += 1;
        MinWaitBetweenRepetitions_Investigate += 1;
        MaxWaitBetweenRepetitions_Investigate += 1;
        // EcholocationParams_Investigate.uniformity -= 1;
        // EcholocationParams_Investigate.numRays -= 1;
        // EcholocationParams_Investigate.angle -= 1;
        // EcholocationParams_Investigate.visualVolume -= 1;

        // ----------------- Sound Variables ------------------
        MaxQuietSound_Sound += 1;
        MaxMediumSound_Sound += 1;
        // EcholocationParams_Sound.uniformity -= 1;
        // EcholocationParams_Sound.numRays -= 1;
        // EcholocationParams_Sound.angle -= 1;
        // EcholocationParams_Sound.visualVolume -= 1;
        QuietSounds.MinWaitTime += 1;
        QuietSounds.MaxWaitTime += 1;
        QuietSounds.MoveSpeed -= 1;
        QuietSounds.StoppingDistance += 1;
        QuietSounds.RadiusFromSound += 1;
        MediumSounds.MinWaitTime += 1;
        MediumSounds.MaxWaitTime += 1;
        MediumSounds.MoveSpeed -= 1;
        MediumSounds.StoppingDistance += 1;
        MediumSounds.RadiusFromSound += 1;
        LoudSounds.MinWaitTime += 1;
        LoudSounds.MaxWaitTime += 1;
        LoudSounds.MoveSpeed -= 1;
        LoudSounds.StoppingDistance += 1;
        LoudSounds.RadiusFromSound += 1;

        // ----------------- Chase Variables ------------------
        MinSpeed_Chase -= 1;
        MaxSpeed_Chase -= 1;
        SpeedIncrement_Chase -= 1;
        TurnSpeed_Chase -= 1;
        StoppingDistance_Chase += 1;
        DistanceMonsterLosesPlayer_Chase -= 1;
        // EcholocationParams_Chase.uniformity -= 1;
        // EcholocationParams_Chase.numRays -= 1;
        // EcholocationParams_Chase.angle -= 1;
        // EcholocationParams_Chase.visualVolume -= 1;

        // ---------------- Run Away Variables ----------------
        MoveSpeed_RunAway -= 1;
        TurnSpeed_RunAway -= 1;
        StoppingDistance_RunAway += 1;
        // EcholocationParams_Chase.uniformity -= 1;
        // EcholocationParams_Chase.numRays -= 1;
        // EcholocationParams_Chase.angle -= 1;
        // EcholocationParams_Chase.visualVolume -= 1;

        // ---------------- Grenade Variables -----------------
        GrenadeActivationDistance += 1;
        DistanceRunFromGrenade += 1;

        // ----------------- Hiding Variables -----------------
        DistanceToTriggerInvestigation_Hiding -= 1;
        MaxTimeBeforeInvestigation_Hiding += 1;
        MinInvestigationTime_Hiding -= 1;
        MaxInvestigationTime_Hiding -= 1;
        MinWaitBeforeGotoSound_Hiding += 1;
        MaxWaitBeforeGotoSound_Hiding += 1;
        PatrolRadius_Hiding += 1;
        MoveSpeedGotoSound_Hiding -= 1;
        TurnSpeedGotoSound_Hiding -= 1;
        StoppingDistanceGotoSound_Hiding += 1;
        MoveSpeedPatrol_Hiding -= 1;
        TurnSpeedPatrol_Hiding -= 1;
        StoppingDistancePatrol_Hiding += 1;
        // EcholocationParams_Hiding.uniformity -= 1;
        // EcholocationParams_Hiding.numRays -= 1;
        // EcholocationParams_Hiding.angle -= 1;
        // EcholocationParams_Hiding.visualVolume -= 1;

        // ------------ Far From Player Variables -------------
        MaxDistanceFromPlayer += 1;
        DistanceToMoveToIfFarFromPlayer += 1;
        MoveSpeedFarFromPlayer -= 1;
        StoppingDistanceFarFromPlayer += 1;

        // ---------- Sound Effect Volume Variables -----------
        // PlayerSeenScreamVolume -= 1;
        // GrenadeScreamVolume -= 1;
        // WaterfallScreamVolume -= 1;
        // HeardSoundGruntVolume -= 1;
        // ClickingVolume -= 1;
        // GrowlVolume -= 1;

        NewVarsEvent.SendEventMessage();
    }

    void Update()
    {
        
        if (UpdateVariables)
        {
            UpdateVariables = false;
            NewVarsEvent.SendEventMessage();
        }
    }
}
