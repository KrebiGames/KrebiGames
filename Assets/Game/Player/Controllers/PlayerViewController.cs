using Quantum;
using UnityEngine;

public unsafe class PlayerViewController : QuantumCallbacks {
	public string Nickname;
	[SerializeField] public QuantumEntityView EntityView;

	public PlayerRef PlayerRef;
	public PlayerTeam PlayerTeam;

	public void OnEntityInstantiated(QuantumGame game) {
		Frame frame = game.Frames.Predicted;
		PlayerStatus* playerStatus = frame.Unsafe.GetPointer<PlayerStatus>(EntityView.EntityRef);

		PlayerRef = playerStatus->PlayerRef;

		RuntimePlayer runtimePlayerData = frame.GetPlayerData(PlayerRef);
		Nickname = runtimePlayerData.PlayerNickname;

		PlayerTeam = playerStatus->PlayerTeam;

		PlayersController.Instance.RegisterPlayer(game, this);
	}

	public void OnEntityDestroyed(QuantumGame game) 
		=> PlayersController.Instance.DeregisterPlayer(this);
}
