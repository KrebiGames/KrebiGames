using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputPoll : MonoBehaviour {

	[SerializeField] private InputActionReference moveAction;
	[SerializeField] private InputActionReference lookAction;
	[SerializeField] private InputActionReference jumpAction;
	[SerializeField] private InputActionReference sprintAction;

	private readonly Dictionary<PlayerInput, CachedActions> actionCache = new();

	private class CachedActions {
		public InputAction Move;
		public InputAction Look;
		public InputAction Jump;
		public InputAction Sprint;
	}

	private void Start() {
		QuantumCallback.Subscribe<CallbackPollInput>(this, PollInput);
	}

	private void PollInput(CallbackPollInput callback) {
		PlayerInputData input = default;

		var localPlayers = callback.Game.GetLocalPlayers();
		if (localPlayers.Count == 0)
			return;

		var player = localPlayers[callback.PlayerSlot];

		LocalPlayerAccess localPlayerAccess =
			LocalPlayersController.Instance.GetLocalPlayerAccess(player);

		if (localPlayerAccess != null && localPlayerAccess.LocalPlayer != null) {

			PlayerInput playerInput = localPlayerAccess.PlayerInput;

			if (!actionCache.TryGetValue(playerInput, out CachedActions actions)) {
				actions = new CachedActions {
					Move = playerInput.actions.FindAction(moveAction.action.id),
					Look = playerInput.actions.FindAction(lookAction.action.id),
					Jump = playerInput.actions.FindAction(jumpAction.action.id),
					Sprint = playerInput.actions.FindAction(sprintAction.action.id)
				};

				actionCache.Add(playerInput, actions);
			}

			input.MoveDirection =
				actions.Move.ReadValue<Vector2>().ToFPVector2();

			Vector2 look = actions.Look.ReadValue<Vector2>();
			input.AimDirection =
				new Vector2(look.x, 0).ToFPVector2();

			input.Sprint = actions.Sprint.IsPressed();
			input.Jump = actions.Jump.IsPressed();
		}

		callback.SetInput(input, DeterministicInputFlags.Repeatable);
	}
}