using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Increase Chase Speed")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Increase Chase Speed", message: "Increase Chase Speed", category: "Events", id: "647735df3f50501e59b4b03f55b5fb23")]
public sealed partial class IncreaseChaseSpeed : EventChannel { }

