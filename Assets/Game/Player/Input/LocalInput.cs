using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalInput : MonoBehaviour {
	private void Start() => QuantumCallback.Subscribe<CallbackPollInput>(this, PollInput);

	private void PollInput(CallbackPollInput callback) {
		QuantumDemoInputTopDown input = default;

		var localPlayers = callback.Game.GetLocalPlayers();
		if (localPlayers.Count == 0)
			return;

		var player = localPlayers[callback.PlayerSlot];

		LocalPlayerAccess localPlayerAccess = LocalPlayersController.Instance.GetLocalPlayerAccess(player);
		if (localPlayerAccess != null && localPlayerAccess.LocalPlayer != null) {
			PlayerInput playerInput = localPlayerAccess.PlayerInput;

			input.MoveDirection = playerInput.actions["Move"].ReadValue<Vector2>().ToFPVector2();

			Vector2 look = playerInput.actions["Look"].ReadValue<Vector2>();
			input.AimDirection = new Vector2(look.x, 0).ToFPVector2();

			input.Jump = playerInput.actions["Jump"].IsPressed();
		}

		callback.SetInput(input, DeterministicInputFlags.Repeatable);
	}
}
