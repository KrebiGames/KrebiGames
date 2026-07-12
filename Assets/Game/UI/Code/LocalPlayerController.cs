using Quantum;
using Quantum.Menu;
using TMPro;
using UnityEngine;

public class LocalPlayerController : MonoBehaviour {
	public QuantumMenuUIController MenuUIController;

	[SerializeField] private AssetRef<EntityPrototype> _characterPrototype;
	[SerializeField] private TMP_Dropdown _playerCountDropdown;

	public void OnDropdownChanged() => SetupLocalPlayers(GetLastSelectedLocalPlayersCount());

	private void SetupLocalPlayers(int localPlayersCount) {
		MenuUIController.ConnectArgs.RuntimePlayers = new RuntimePlayer[localPlayersCount];
		for (int i = 0; i < localPlayersCount; i++)
			MenuUIController.ConnectArgs.RuntimePlayers[i] = new RuntimePlayer {
				PlayerAvatar = _characterPrototype,
				PlayerNickname = $"Local player {i}"
			};
	}

	public int GetLastSelectedLocalPlayersCount() => _playerCountDropdown.value + 1;
}