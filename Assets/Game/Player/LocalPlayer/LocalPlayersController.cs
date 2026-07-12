using System.Collections.Generic;
using UnityEngine;
using Quantum;

public class LocalPlayersController : MonoBehaviour {
	public static LocalPlayersController Instance { get; private set; }

	[SerializeField] private LocalPlayersConfig[] _localPlayersConfigPrefabs;

	private Dictionary<int, LocalPlayerAccess> _localPlayerAccessByPlayerIndices = new Dictionary<int, LocalPlayerAccess>();

	public Dictionary<int, LocalPlayerAccess>.ValueCollection LocalPlayerAccessCollection {
		get {
			if(_localPlayerAccessByPlayerIndices.Count == 0) {
				Initialize();
			}

			return _localPlayerAccessByPlayerIndices.Values;
		}
	}

	private void Awake() => Instance = this;

	public LocalPlayerAccess InitializeLocalPlayer(PlayerViewController playerViewController) {
		LocalPlayerAccess localPlayerAccess = GetLocalPlayerAccess(playerViewController.PlayerRef);
		localPlayerAccess.InitializeLocalPlayer(playerViewController);

		return localPlayerAccess;
	}

	public LocalPlayerAccess GetLocalPlayerAccess(int playerIndex) {
		if(_localPlayerAccessByPlayerIndices.Count == 0) {
			Initialize();
		}

		_localPlayerAccessByPlayerIndices.TryGetValue(playerIndex, out LocalPlayerAccess localPlayerAccess);
		return localPlayerAccess;
	}

	private void Initialize() {
		if(_localPlayerAccessByPlayerIndices.Count != 0)
			return;

		var localPlayerIndices = QuantumRunner.Default.Game.GetLocalPlayers();
		if(localPlayerIndices.Count == 0)
			return;

		LocalPlayersConfig localPlayersConfig = Instantiate(_localPlayersConfigPrefabs[localPlayerIndices.Count - 1], transform);
		for(int i = 0; i < localPlayerIndices.Count; i++) {
			LocalPlayerAccess localPlayerAccess = localPlayersConfig.GetLocalPlayerAccess(i);
			localPlayerAccess.IsMainLocalPlayer = i == 0;

			_localPlayerAccessByPlayerIndices.Add(localPlayerIndices[i], localPlayerAccess);
		}
	}
}
