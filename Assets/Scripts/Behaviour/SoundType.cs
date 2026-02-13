using System;
using Unity.Behavior;

[Flags]
[BlackboardEnum]
public enum SoundType
{
	Rock = 1 << 0,
	Player = 1 << 1,
	Water = 1 << 2
}
