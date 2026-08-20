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
	[SerializeField] private InputActionReference leftClawReachAction;
	[SerializeField] private InputActionReference rightClawReachAction;
	[SerializeField] private InputActionReference leftClawKickAction;
	[SerializeField] private InputActionReference rightClawKickAction;
	[SerializeField] private InputActionReference leftClawCutAction;
	[SerializeField] private InputActionReference rightClawCutAction;

	private readonly Dictionary<PlayerInput, CachedActions> actionCache = new();

	private class CachedActions {
		public InputAction Move;
		public InputAction Look;
		public InputAction Jump;
		public InputAction Sprint;
		public InputAction LeftClawReach;
		public InputAction RightClawReach;
		public InputAction LeftClawKick;
		public InputAction RightClawKick;
		public InputAction LeftClawCut;
		public InputAction RightClawCut;
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
		LocalPlayerAccess localPlayerAccess = LocalPlayersController.Instance.GetLocalPlayerAccess(player);

		if (localPlayerAccess != null && localPlayerAccess.LocalPlayer != null) {
			PlayerInput playerInput = localPlayerAccess.PlayerInput;

			if (!actionCache.TryGetValue(playerInput, out CachedActions actions)) {
				actions = new CachedActions {
					Move = playerInput.actions.FindAction(moveAction.action.id),
					Look = playerInput.actions.FindAction(lookAction.action.id),
					Jump = playerInput.actions.FindAction(jumpAction.action.id),
					Sprint = playerInput.actions.FindAction(sprintAction.action.id),
					LeftClawReach = playerInput.actions.FindAction(leftClawReachAction.action.id),
					RightClawReach = playerInput.actions.FindAction(rightClawReachAction.action.id),
					LeftClawKick = playerInput.actions.FindAction(leftClawKickAction.action.id),
					RightClawKick = playerInput.actions.FindAction(rightClawKickAction.action.id),
					LeftClawCut = playerInput.actions.FindAction(leftClawCutAction.action.id),
					RightClawCut = playerInput.actions.FindAction(rightClawCutAction.action.id)
				};

				actionCache.Add(playerInput, actions);
			}

			input.MoveDirection = actions.Move.ReadValue<Vector2>().ToFPVector2();

			Vector2 look = actions.Look.ReadValue<Vector2>();
			input.AimDirection = new Vector2(look.x, 0).ToFPVector2();

			input.Jump = actions.Jump.IsPressed();
			input.Sprint = actions.Sprint.IsPressed();
			input.LeftClawReach = FP.FromFloat_UNSAFE(actions.LeftClawReach.ReadValue<float>());
			input.RightClawReach = FP.FromFloat_UNSAFE(actions.RightClawReach.ReadValue<float>());
			input.LeftClawKick = actions.LeftClawKick.IsPressed();
			input.RightClawKick = actions.RightClawKick.IsPressed();
			input.LeftClawCut = actions.LeftClawCut.IsPressed();
			input.RightClawCut = actions.RightClawCut.IsPressed();
		}

		callback.SetInput(input, DeterministicInputFlags.Repeatable);
	}
}