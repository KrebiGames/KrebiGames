using Friflo.Engine.ECS;
using UnityEngine;

namespace ECS.Input {
	public struct Move : IComponent { public Vector2 Value; }

	public struct Jump : ITag { }
}