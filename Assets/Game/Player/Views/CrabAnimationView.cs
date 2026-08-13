using UnityEngine;
using Quantum;
using Photon.Deterministic;
using FIMSpace.FProceduralAnimation;

public class CrabAnimationView : QuantumEntityViewComponent {

	[Header("View")]
	[SerializeField] private Transform root;
	[SerializeField] private Transform shell;
	[SerializeField] private bool usePredictedFrame = true;

	[Header("Step Size")]
	[SerializeField] private float minGlueRange = 0.15f;
	[SerializeField] private float maxGlueRange = 0.5f;

	[Header("Movement")]
	[SerializeField] private float moveThreshold = 0.05f;
	[SerializeField] private float maxSpeed = 5f;
	[SerializeField] private float velocitySmooth = 20f;

	[Header("Body Height")]
	[SerializeField] private float minBodyHeight = -0.03f;
	[SerializeField] private float maxBodyHeight = 0.1f;
	[SerializeField] private float bodyHeightSmooth = 50f;

	[Header("Body Pitch")]
	[SerializeField] private float minBodyPitch = -30f;
	[SerializeField] private float maxBodyPitch = -60f;
	[SerializeField] private float bodyPitchSmooth = 20f;

	[Header("Landing Wobble")]
	[SerializeField] private float landingHeightWobble = 0.06f;
	[SerializeField] private float landingPitchWobble = 4f;
	[SerializeField] private float landingWobbleFrequency = 3.5f;
	[SerializeField] private float landingWobbleDamping = 6f;
	[SerializeField] private float landingMinImpactSpeed = 1f;
	[SerializeField] private float landingMaxImpactSpeed = 5f;

	float currentBodyPitch;
	float currentBodyHeight;
	float verticalVelocity;
	float maxFallSpeed;
	float landingWobbleTime;
	float landingWobbleStrength;
	float landingHeightOffset;
	float landingPitchOffset;
	bool wasGrounded;
	bool groundedInitialized;

	LegsAnimator legsAnimator;
	Vector3 smoothedVelocity;

	private void Awake() => legsAnimator = root.GetComponentInChildren<LegsAnimator>();

	public override void OnUpdateView() {
		var frame = usePredictedFrame ? PredictedFrame : VerifiedFrame;
		if (frame == null)
			return;

		var kcc = frame.Get<KCC>(EntityRef);
		bool isGrounded = kcc.Data.IsGrounded;

		UpdateVelocity(kcc);

		if (!groundedInitialized) {
			wasGrounded = isGrounded;
			groundedInitialized = true;
		}

		if (!isGrounded)
			maxFallSpeed = Mathf.Max(maxFallSpeed, -verticalVelocity);

		if (isGrounded && !wasGrounded) {
			StartLandingWobble(maxFallSpeed);
			maxFallSpeed = 0f;
		}

		UpdateLandingWobble();

		float speed = smoothedVelocity.magnitude;
		float speed01 = Mathf.Clamp01(speed / maxSpeed);
		bool isMoving = speed > moveThreshold;

		legsAnimator.GlueRangeThreshold = Mathf.Lerp(minGlueRange, maxGlueRange, speed01);

		UpdateMovement(isMoving, isGrounded);
		UpdateBodyHeight(speed01);
		UpdateBodyRotation(speed01, frame.Get<CrabOrientation>(EntityRef).LocalYaw.AsFloat, isGrounded);

		wasGrounded = isGrounded;
	}

	private void UpdateVelocity(KCC kcc) {
		FPVector3 fpVelocity = kcc.Data.KinematicVelocity + kcc.Data.DynamicVelocity;
		Vector3 fullVelocity = new Vector3(fpVelocity.X.AsFloat, fpVelocity.Y.AsFloat, fpVelocity.Z.AsFloat);
		Vector3 velocity = new Vector3(fullVelocity.x, 0f, fullVelocity.z);

		float velocityT = 1f - Mathf.Exp(-velocitySmooth * Time.deltaTime);
		smoothedVelocity = Vector3.Lerp(smoothedVelocity, velocity, velocityT);

		LAM_VerticalUmbrellaPose.SetVelocity(legsAnimator, fullVelocity);
	}

	private void StartLandingWobble(float impactSpeed) {
		landingWobbleStrength = Mathf.InverseLerp(landingMinImpactSpeed, landingMaxImpactSpeed, impactSpeed);
		landingWobbleTime = 0f;
	}

	private void UpdateLandingWobble() {
		if (landingWobbleStrength <= 0.001f) {
			landingHeightOffset = 0f;
			landingPitchOffset = 0f;
			return;
		}

		landingWobbleTime += Time.deltaTime;

		float decay = Mathf.Exp(-landingWobbleDamping * landingWobbleTime);
		float wave = Mathf.Cos(landingWobbleTime * landingWobbleFrequency * Mathf.PI * 2f);

		landingHeightOffset = -wave * landingHeightWobble * landingWobbleStrength * decay;
		landingPitchOffset = wave * landingPitchWobble * landingWobbleStrength * decay;

		if (decay < 0.01f)
			landingWobbleStrength = 0f;
	}

	private void UpdateMovement(bool isMoving, bool isGrounded) {
		legsAnimator.User_SetDesiredMovementDirection(isMoving ? smoothedVelocity : Vector3.zero);
		legsAnimator.User_SetIsMoving(isMoving);
		legsAnimator.User_SetIsGrounded(isGrounded);
	}

	private void UpdateBodyHeight(float speed01) {
		float targetBodyHeight = Mathf.Lerp(minBodyHeight, maxBodyHeight, speed01);
		float heightT = 1f - Mathf.Exp(-bodyHeightSmooth * Time.deltaTime);
		currentBodyHeight = Mathf.Lerp(currentBodyHeight, targetBodyHeight, heightT);

		Vector3 pelvisOffset = legsAnimator.ExtraPelvisOffset;
		pelvisOffset.y = currentBodyHeight + landingHeightOffset;
		legsAnimator.ExtraPelvisOffset = pelvisOffset;
	}

	private void UpdateBodyRotation(float speed01, float yaw, bool isGrounded) {
		float targetPitch = isGrounded ? Mathf.Lerp(minBodyPitch, maxBodyPitch, speed01) : minBodyPitch;
		float rotationT = 1f - Mathf.Exp(-bodyPitchSmooth * Time.deltaTime);
		currentBodyPitch = Mathf.Lerp(currentBodyPitch, targetPitch, rotationT);

		root.localRotation = Quaternion.Euler(0f, yaw, 0f);
		shell.localRotation = Quaternion.Euler(currentBodyPitch + landingPitchOffset, 0f, 0f);
	}
}