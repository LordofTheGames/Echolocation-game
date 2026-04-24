using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/New difficulty vars")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "New difficulty vars", message: "new difficulty vars", category: "Events", id: "88346b508a68844393226711e5986bd8")]
public sealed partial class NewDifficultyVars : EventChannel { }

