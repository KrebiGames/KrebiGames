using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerAccess : MonoBehaviour {
	[SerializeField] public PlayerInput PlayerInput;
	[SerializeField] public CinemachineCamera CMCamera;

	public bool IsMainLocalPlayer { get; set; }
	public PlayerViewController LocalPlayer { get; private set; }

	public void InitializeLocalPlayer(PlayerViewController localPlayer)
		=> LocalPlayer = localPlayer;

	public void AddToCameraTarget(Transform target, bool isLocal) 
		=> CMCamera.Target.TrackingTarget = target;

	public void RemoveFromCameraTarget(Transform target)
		=> CMCamera.Target.TrackingTarget = null;
}
