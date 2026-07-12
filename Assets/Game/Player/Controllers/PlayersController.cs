using Quantum;
using System.Collections.Generic;

public class PlayersController : QuantumCallbacks {
	public static PlayersController Instance { get; private set; }

	private Dictionary<EntityRef, PlayerViewController> playersByEntityRefs = new Dictionary<EntityRef, PlayerViewController>();

	private void Awake() => Instance = this;

	public void RegisterPlayer(QuantumGame game, PlayerViewController player) {
		playersByEntityRefs.Add(player.EntityView.EntityRef, player);

		if (game.PlayerIsLocal(player.PlayerRef))
			LocalPlayersController.Instance.InitializeLocalPlayer(player);

		foreach (var localPlayerAccess in LocalPlayersController.Instance.LocalPlayerAccessCollection)
			if (player.PlayerRef == localPlayerAccess.LocalPlayer?.PlayerRef)
				localPlayerAccess.AddToCameraTarget(player.transform, true);
	}

	public void DeregisterPlayer(PlayerViewController player) {
		playersByEntityRefs.Remove(player.EntityView.EntityRef);

		foreach (var localPlayerAccess in LocalPlayersController.Instance.LocalPlayerAccessCollection)
			if (player.PlayerRef == localPlayerAccess.LocalPlayer?.PlayerRef)
				localPlayerAccess.RemoveFromCameraTarget(player.transform);
	}
}
