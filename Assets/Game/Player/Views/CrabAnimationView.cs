using UnityEngine;
using Quantum;
using Photon.Deterministic;
using FIMSpace.FProceduralAnimation;

public class CrabAnimationView : QuantumEntityViewComponent {
	[Header("View")]
	[SerializeField] private Transform root;
	[SerializeField] private bool usePredictedFrame = true;

	[Header("Body Height")]
	[SerializeField] private float minBodyHeight = -0.03f;
	[SerializeField] private float maxBodyHeight = 0.1f;
	[SerializeField] private float bodyHeightSmooth = 50f;

	[Header("Movement")]
	[SerializeField] private float moveThreshold = 0.05f;
	[SerializeField] private float maxSpeed = 5f;
	[SerializeField] private float velocitySmooth = 20f;

	[Header("Step Size")]
	[SerializeField] private float minGlueRange = 0.15f;
	[SerializeField] private float maxGlueRange = 0.5f;

	LegsAnimator legsAnimator;
	Vector3 smoothedVelocity;
	float currentBodyHeight;

	private void Awake() => legsAnimator = root.GetComponentInChildren<LegsAnimator>();

	public override void OnUpdateView() {
		var frame = usePredictedFrame ? PredictedFrame : VerifiedFrame;
		if (frame == null)
			return;

		var kcc = frame.Get<KCC>(EntityRef);

		UpdateVelocity(kcc);

		float speed = smoothedVelocity.magnitude;
		float speed01 = Mathf.Clamp01(speed / maxSpeed);
		bool isMoving = speed > moveThreshold;

		UpdateBodyHeight(speed01);
		UpdateMovement(kcc, isMoving);
		// Orientation
		root.localRotation = Quaternion.Euler(0f, frame.Get<CrabOrientation>(EntityRef).LocalYaw.AsFloat, 0f);
		// Setup size
		legsAnimator.GlueRangeThreshold = Mathf.Lerp(minGlueRange, maxGlueRange, speed01);
	}

	private void UpdateVelocity(KCC kcc) {
		FPVector3 fpVelocity = kcc.Data.KinematicVelocity + kcc.Data.DynamicVelocity;
		Vector3 velocity = new Vector3(fpVelocity.X.AsFloat, 0f, fpVelocity.Z.AsFloat);
		float velocityT = 1f - Mathf.Exp(-velocitySmooth * Time.deltaTime);
		smoothedVelocity = Vector3.Lerp(smoothedVelocity, velocity, velocityT);
	}

	private void UpdateMovement(KCC kcc, bool isMoving) {
		legsAnimator.User_SetDesiredMovementDirection(isMoving ? smoothedVelocity : Vector3.zero);
		legsAnimator.User_SetIsMoving(isMoving);
		legsAnimator.User_SetIsGrounded(kcc.Data.IsGrounded);
	}

	private void UpdateBodyHeight(float speed01) {
		float targetBodyHeight = Mathf.Lerp(minBodyHeight, maxBodyHeight, speed01);
		float heightT = 1f - Mathf.Exp(-bodyHeightSmooth * Time.deltaTime);
		currentBodyHeight = Mathf.Lerp(currentBodyHeight, targetBodyHeight, heightT);

		Vector3 pelvisOffset = legsAnimator.ExtraPelvisOffset;
		pelvisOffset.y = currentBodyHeight;
		legsAnimator.ExtraPelvisOffset = pelvisOffset;
	}
}