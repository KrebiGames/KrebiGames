using Friflo.Engine.ECS.Systems;
using UnityEngine;

namespace ECS.Input {
	public class MoveSystem : QuerySystem<Move, Action> {
		protected override void OnUpdate() {
			foreach (var (moves, actions, entities) in Query.Chunks)
				for (int e = 0; e < entities.Length; e++)
					moves[e].Value = actions[e].Value.ReadValue<Vector2>();
		}
	}
}