using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Jumpscare")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Jumpscare", message: "Play Jumpscare", category: "Events", id: "7db935f3a8dbedc05bd119950003335b")]
public sealed partial class Jumpscare : EventChannel { }

