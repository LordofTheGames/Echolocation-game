using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Start Main Behaviour")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Start Main Behaviour", message: "Start Main Behaviour", category: "Events", id: "9dd7d6f250f3ab7115f64ce0117873d6")]
public sealed partial class StartMainBehaviour : EventChannel { }

