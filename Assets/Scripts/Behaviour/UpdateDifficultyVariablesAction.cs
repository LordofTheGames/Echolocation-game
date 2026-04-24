using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update difficulty variables", story: "Update difficulty variables", category: "Action", id: "da0c9d3b13bac56d7c50ff056939738b")]
public partial class UpdateDifficultyVariablesAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    [SerializeReference] public BlackboardVariable<float> MoveSpeed_Patrol;
    [SerializeReference] public BlackboardVariable<float> currentSoundVolume;
    [SerializeReference] public BlackboardVariable<float> TurnSpeed_Patrol;
    [SerializeReference] public BlackboardVariable<float> StoppingDistance_Patrol;
    [SerializeReference] public BlackboardVariable<float> MinWaitAfterMove_Patrol;
    [SerializeReference] public BlackboardVariable<float> MaxWaitAfterMove_Patrol;
    [SerializeReference] public BlackboardVariable<float> MinRoomTime_Patrol;
    [SerializeReference] public BlackboardVariable<float> MaxRoomTime_Patrol;
    [SerializeReference] public BlackboardVariable<float> uniformity_Patrol;
    [SerializeReference] public BlackboardVariable<int> numRays_Patrol;
    [SerializeReference] public BlackboardVariable<float> angle_Patrol;
    [SerializeReference] public BlackboardVariable<float> visualVolume_Patrol;

    [SerializeReference] public BlackboardVariable<float> MoveSpeed_Investigate;
    [SerializeReference] public BlackboardVariable<float> TurnSpeed_Investigate;
    [SerializeReference] public BlackboardVariable<float> StoppingDistance_Investigate;
    [SerializeReference] public BlackboardVariable<float> Repetitions_Investigate;
    [SerializeReference] public BlackboardVariable<float> RadiusFromStartPoint_Investigate;
    [SerializeReference] public BlackboardVariable<float> MinWaitBetweenRepetitions_Investigate;
    [SerializeReference] public BlackboardVariable<float> MaxWaitBetweenRepetitions_Investigate;
    [SerializeReference] public BlackboardVariable<float> uniformity_Investigate;
    [SerializeReference] public BlackboardVariable<int> numRays_Investigate;
    [SerializeReference] public BlackboardVariable<float> angle_Investigate;
    [SerializeReference] public BlackboardVariable<float> visualVolume_Investigate;

    [SerializeReference] public BlackboardVariable<float> MaxQuietSound_Sound;
    [SerializeReference] public BlackboardVariable<float> MaxMediumSound_Sound;
    [SerializeReference] public BlackboardVariable<float> uniformity_Sound;
    [SerializeReference] public BlackboardVariable<int> numRays_Sound;
    [SerializeReference] public BlackboardVariable<float> angle_Sound;
    [SerializeReference] public BlackboardVariable<float> visualVolume_Sound;
    [SerializeReference] public BlackboardVariable<float> MinWaitTime_Quiet;
    [SerializeReference] public BlackboardVariable<float> MaxWaitTime_Quiet;
    [SerializeReference] public BlackboardVariable<float> MoveSpeed_Quiet;
    [SerializeReference] public BlackboardVariable<float> StoppingDistance_Quiet;
    [SerializeReference] public BlackboardVariable<float> RadiusFromSound_Quiet;
    [SerializeReference] public BlackboardVariable<float> MinWaitTime_Medium;
    [SerializeReference] public BlackboardVariable<float> MaxWaitTime_Medium;
    [SerializeReference] public BlackboardVariable<float> MoveSpeed_Medium;
    [SerializeReference] public BlackboardVariable<float> StoppingDistance_Medium;
    [SerializeReference] public BlackboardVariable<float> RadiusFromSound_Medium;
    [SerializeReference] public BlackboardVariable<float> MinWaitTime_Loud;
    [SerializeReference] public BlackboardVariable<float> MaxWaitTime_Loud;
    [SerializeReference] public BlackboardVariable<float> MoveSpeed_Loud;
    [SerializeReference] public BlackboardVariable<float> StoppingDistance_Loud;
    [SerializeReference] public BlackboardVariable<float> RadiusFromSound_Loud;

    [SerializeReference] public BlackboardVariable<float> MinSpeed_Chase;
    [SerializeReference] public BlackboardVariable<float> MaxSpeed_Chase;
    [SerializeReference] public BlackboardVariable<float> FinalChaseSpeed_Chase;
    [SerializeReference] public BlackboardVariable<float> SpeedIncrement_Chase;
    [SerializeReference] public BlackboardVariable<float> TurnSpeed_Chase;
    [SerializeReference] public BlackboardVariable<float> StoppingDistance_Chase;
    [SerializeReference] public BlackboardVariable<float> DistanceMonsterLosesPlayer_Chase;
    [SerializeReference] public BlackboardVariable<float> uniformity_Chase;
    [SerializeReference] public BlackboardVariable<int> numRays_Chase;
    [SerializeReference] public BlackboardVariable<float> angle_Chase;
    [SerializeReference] public BlackboardVariable<float> visualVolume_Chase;

    [SerializeReference] public BlackboardVariable<float> MoveSpeed_RunAway;
    [SerializeReference] public BlackboardVariable<float> TurnSpeed_RunAway;
    [SerializeReference] public BlackboardVariable<float> StoppingDistance_RunAway;
    [SerializeReference] public BlackboardVariable<float> uniformity_RunAway;
    [SerializeReference] public BlackboardVariable<int> numRays_RunAway;
    [SerializeReference] public BlackboardVariable<float> angle_RunAway;
    [SerializeReference] public BlackboardVariable<float> visualVolume_RunAway;

    [SerializeReference] public BlackboardVariable<float> GrenadeActivationDistance;
    [SerializeReference] public BlackboardVariable<float> DistanceRunFromGrenade;

    [SerializeReference] public BlackboardVariable<float> DistanceToTriggerInvestigation_Hiding;
    [SerializeReference] public BlackboardVariable<float> MaxTimeBeforeInvestigation_Hiding;
    [SerializeReference] public BlackboardVariable<float> MinInvestigationTime_Hiding;
    [SerializeReference] public BlackboardVariable<float> MaxInvestigationTime_Hiding;
    [SerializeReference] public BlackboardVariable<float> MinWaitBeforeGotoSound_Hiding;
    [SerializeReference] public BlackboardVariable<float> MaxWaitBeforeGotoSound_Hiding;
    [SerializeReference] public BlackboardVariable<float> PatrolRadius_Hiding;
    [SerializeReference] public BlackboardVariable<float> MoveSpeedGotoSound_Hiding;
    [SerializeReference] public BlackboardVariable<float> TurnSpeedGotoSound_Hiding;
    [SerializeReference] public BlackboardVariable<float> StoppingDistanceGotoSound_Hiding;
    [SerializeReference] public BlackboardVariable<float> MoveSpeedPatrol_Hiding;
    [SerializeReference] public BlackboardVariable<float> TurnSpeedPatrol_Hiding;
    [SerializeReference] public BlackboardVariable<float> StoppingDistancePatrol_Hiding;
    [SerializeReference] public BlackboardVariable<float> uniformity_Hiding;
    [SerializeReference] public BlackboardVariable<int> numRays_Hiding;
    [SerializeReference] public BlackboardVariable<float> angle_Hiding;
    [SerializeReference] public BlackboardVariable<float> visualVolume_Hiding;

    [SerializeReference] public BlackboardVariable<float> MaxDistanceFromPlayer;
    [SerializeReference] public BlackboardVariable<float> DistanceToMoveToIfFarFromPlayer;
    [SerializeReference] public BlackboardVariable<float> MoveSpeedFarFromPlayer;
    [SerializeReference] public BlackboardVariable<float> StoppingDistanceFarFromPlayer;
    [SerializeReference] public BlackboardVariable<float> TimeUntilMovesClose;
    [SerializeReference] public BlackboardVariable<float> DistanceToMoveAwayFromPlayer;
    [SerializeReference] public BlackboardVariable<float> MoveSpeedAwayFromPlayer;
    [SerializeReference] public BlackboardVariable<float> StoppingDistanceAwayFromPlayer;
    [SerializeReference] public BlackboardVariable<float> TimeUntilMovesAway;

    [SerializeReference] public BlackboardVariable<float> PlayerSeenScreamVolume;
    [SerializeReference] public BlackboardVariable<float> GrenadeScreamVolume;
    [SerializeReference] public BlackboardVariable<float> WaterfallScreamVolume;
    [SerializeReference] public BlackboardVariable<float> HeardSoundGruntVolume;
    [SerializeReference] public BlackboardVariable<float> ClickingVolume;
    [SerializeReference] public BlackboardVariable<float> GrowlVolume;

    protected override Status OnStart()
    {
        MonsterDifficultyManager manager = Self.Value.GetComponent<MonsterDifficultyManager>();

        MoveSpeed_Patrol.Value = manager.MoveSpeed_Patrol;
        TurnSpeed_Patrol.Value = manager.TurnSpeed_Patrol;
        StoppingDistance_Patrol.Value = manager.StoppingDistance_Patrol;
        // MoveRadius_Patrol.Value = manager.MoveRadius_Patrol;
        MinWaitAfterMove_Patrol.Value = manager.MinWaitAfterMove_Patrol;
        MaxWaitAfterMove_Patrol.Value = manager.MaxWaitAfterMove_Patrol;
        MinRoomTime_Patrol.Value = manager.MinRoomTime_Patrol;
        MaxRoomTime_Patrol.Value = manager.MaxRoomTime_Patrol;
        uniformity_Patrol.Value = manager.EcholocationParams_Patrol.uniformity;
        numRays_Patrol.Value = manager.EcholocationParams_Patrol.numRays;
        angle_Patrol.Value = manager.EcholocationParams_Patrol.angle;
        visualVolume_Patrol.Value = manager.EcholocationParams_Patrol.visualVolume;

        MoveSpeed_Investigate.Value = manager.MoveSpeed_Investigate;
        TurnSpeed_Investigate.Value = manager.TurnSpeed_Investigate;
        StoppingDistance_Investigate.Value = manager.StoppingDistance_Investigate;
        Repetitions_Investigate.Value = manager.Repetitions_Investigate;
        RadiusFromStartPoint_Investigate.Value = manager.RadiusFromStartPoint_Investigate;
        MinWaitBetweenRepetitions_Investigate.Value = manager.MinWaitBetweenRepetitions_Investigate;
        MaxWaitBetweenRepetitions_Investigate.Value = manager.MaxWaitBetweenRepetitions_Investigate;
        uniformity_Investigate.Value = manager.EcholocationParams_Investigate.uniformity;
        numRays_Investigate.Value = manager.EcholocationParams_Investigate.numRays;
        angle_Investigate.Value = manager.EcholocationParams_Investigate.angle;
        visualVolume_Investigate.Value = manager.EcholocationParams_Investigate.visualVolume;

        MaxQuietSound_Sound.Value = manager.MaxQuietSound_Sound;
        MaxMediumSound_Sound.Value = manager.MaxMediumSound_Sound;
        uniformity_Sound.Value = manager.EcholocationParams_Sound.uniformity;
        numRays_Sound.Value = manager.EcholocationParams_Sound.numRays;
        angle_Sound.Value = manager.EcholocationParams_Sound.angle;
        visualVolume_Sound.Value = manager.EcholocationParams_Sound.visualVolume;
        MinWaitTime_Quiet.Value = manager.QuietSounds.MinWaitTime;
        MaxWaitTime_Quiet.Value = manager.QuietSounds.MaxWaitTime;
        MoveSpeed_Quiet.Value = manager.QuietSounds.MoveSpeed;
        StoppingDistance_Quiet.Value = manager.QuietSounds.StoppingDistance;
        RadiusFromSound_Quiet.Value = manager.QuietSounds.RadiusFromSound;
        MinWaitTime_Medium.Value = manager.MediumSounds.MinWaitTime;
        MaxWaitTime_Medium.Value = manager.MediumSounds.MaxWaitTime;
        MoveSpeed_Medium.Value = manager.MediumSounds.MoveSpeed;
        StoppingDistance_Medium.Value = manager.MediumSounds.StoppingDistance;
        RadiusFromSound_Medium.Value = manager.MediumSounds.RadiusFromSound;
        MinWaitTime_Loud.Value = manager.LoudSounds.MinWaitTime;
        MaxWaitTime_Loud.Value = manager.LoudSounds.MaxWaitTime;
        MoveSpeed_Loud.Value = manager.LoudSounds.MoveSpeed;
        StoppingDistance_Loud.Value = manager.LoudSounds.StoppingDistance;
        RadiusFromSound_Loud.Value = manager.LoudSounds.RadiusFromSound;

        MinSpeed_Chase.Value = manager.MinSpeed_Chase;
        MaxSpeed_Chase.Value = manager.MaxSpeed_Chase;
        FinalChaseSpeed_Chase.Value = manager.FinalChaseSpeed_Chase;
        SpeedIncrement_Chase.Value = manager.SpeedIncrement_Chase;
        TurnSpeed_Chase.Value = manager.TurnSpeed_Chase;
        StoppingDistance_Chase.Value = manager.StoppingDistance_Chase;
        DistanceMonsterLosesPlayer_Chase.Value = manager.DistanceMonsterLosesPlayer_Chase;
        uniformity_Chase.Value = manager.EcholocationParams_Chase.uniformity;
        numRays_Chase.Value = manager.EcholocationParams_Chase.numRays;
        angle_Chase.Value = manager.EcholocationParams_Chase.angle;
        visualVolume_Chase.Value = manager.EcholocationParams_Chase.visualVolume;

        MoveSpeed_RunAway.Value = manager.MoveSpeed_RunAway;
        TurnSpeed_RunAway.Value = manager.TurnSpeed_RunAway;
        StoppingDistance_RunAway.Value = manager.StoppingDistance_RunAway;
        uniformity_RunAway.Value = manager.EcholocationParams_RunAway.uniformity;
        numRays_RunAway.Value = manager.EcholocationParams_RunAway.numRays;
        angle_RunAway.Value = manager.EcholocationParams_RunAway.angle;
        visualVolume_RunAway.Value = manager.EcholocationParams_RunAway.visualVolume;

        GrenadeActivationDistance.Value = manager.GrenadeActivationDistance;
        DistanceRunFromGrenade.Value = manager.DistanceRunFromGrenade;

        DistanceToTriggerInvestigation_Hiding.Value = manager.DistanceToTriggerInvestigation_Hiding;
        MaxTimeBeforeInvestigation_Hiding.Value = manager.MaxTimeBeforeInvestigation_Hiding;
        MinInvestigationTime_Hiding.Value = manager.MinInvestigationTime_Hiding;
        MaxInvestigationTime_Hiding.Value = manager.MaxInvestigationTime_Hiding;
        MinWaitBeforeGotoSound_Hiding.Value = manager.MinWaitBeforeGotoSound_Hiding;
        MaxWaitBeforeGotoSound_Hiding.Value = manager.MaxWaitBeforeGotoSound_Hiding;
        PatrolRadius_Hiding.Value = manager.PatrolRadius_Hiding;
        MoveSpeedGotoSound_Hiding.Value = manager.MoveSpeedGotoSound_Hiding;
        TurnSpeedGotoSound_Hiding.Value = manager.TurnSpeedGotoSound_Hiding;
        StoppingDistanceGotoSound_Hiding.Value = manager.StoppingDistanceGotoSound_Hiding;
        MoveSpeedPatrol_Hiding.Value = manager.MoveSpeedPatrol_Hiding;
        TurnSpeedPatrol_Hiding.Value = manager.TurnSpeedPatrol_Hiding;
        StoppingDistancePatrol_Hiding.Value = manager.StoppingDistancePatrol_Hiding;
        uniformity_Hiding.Value = manager.EcholocationParams_Hiding.uniformity;
        numRays_Hiding.Value = manager.EcholocationParams_Hiding.numRays;
        angle_Hiding.Value = manager.EcholocationParams_Hiding.angle;
        visualVolume_Hiding.Value = manager.EcholocationParams_Hiding.visualVolume;

        MaxDistanceFromPlayer.Value = manager.MaxDistanceFromPlayer;
        DistanceToMoveToIfFarFromPlayer.Value = manager.DistanceToMoveToIfFarFromPlayer;
        MoveSpeedFarFromPlayer.Value = manager.MoveSpeedFarFromPlayer;
        StoppingDistanceFarFromPlayer.Value = manager.StoppingDistanceFarFromPlayer;
        TimeUntilMovesClose.Value = manager.TimeUntilMovesClose;
        DistanceToMoveAwayFromPlayer.Value = manager.DistanceToMoveAwayFromPlayer;
        MoveSpeedAwayFromPlayer.Value = manager.MoveSpeedAwayFromPlayer;
        StoppingDistanceAwayFromPlayer.Value = manager.StoppingDistanceAwayFromPlayer;
        TimeUntilMovesAway.Value = manager.TimeUntilMovesAway;

        PlayerSeenScreamVolume.Value = manager.PlayerSeenScreamVolume;
        GrenadeScreamVolume.Value = manager.GrenadeScreamVolume;
        WaterfallScreamVolume.Value = manager.WaterfallScreamVolume;
        HeardSoundGruntVolume.Value = manager.HeardSoundGruntVolume;
        ClickingVolume.Value = manager.ClickingVolume;
        GrowlVolume.Value = manager.GrowlVolume;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

