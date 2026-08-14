using UnityEngine;
using Quantum;
using Photon.Deterministic;

public class CrabClawsAnimationView : QuantumEntityViewComponent {
	[Header("View")]
	[SerializeField] private bool usePredictedFrame = true;
	[SerializeField] private Transform leftClawTarget;
	[SerializeField] private Transform rightClawTarget;

	[Header("Body Tuck")]
	[SerializeField] private float bodyTuckPosition = 0.05f;
	[SerializeField] private float bodyTuckRotation = 5f;

	[Header("Idle Movement")]
	[SerializeField] private float moveThreshold = 1f;
	[SerializeField] private float idleSpeed = 0.5f;
	[SerializeField] private float idleSmooth = 5f;
	[SerializeField] private Vector3 idlePositionAmount = new Vector3(0.005f, 0.003f, 0.001f);
	[SerializeField] private Vector3 idleRotationAmount = new Vector3(2f, 2f, 3f);
	[SerializeField] private float oppositePhase = 0.5f;

	[Header("Jump")]
	[SerializeField] private float maxJumpPositionOffset = 0.3f;
	[SerializeField] private float maxJumpRotation = 60f;
	[SerializeField] private float jumpSpreadPosition = 0.03f;
	[SerializeField, Range(0f, 0.5f)] private float jumpVariation = 0.15f;
	[SerializeField] private float jumpSmooth = 8f;
	[SerializeField] private float landingSmooth = 25f;

	float maxJumpSpeed = 5f;
	Vector3 leftBasePosition;
	Vector3 rightBasePosition;
	Quaternion leftBaseRotation;
	Quaternion rightBaseRotation;
	float idleTime;
	float idleBlend;
	float leftJump;
	float rightJump;
	float airborneBlend;

	private void Awake() {
		leftBasePosition = leftClawTarget.localPosition;
		rightBasePosition = rightClawTarget.localPosition;
		leftBaseRotation = leftClawTarget.localRotation;
		rightBaseRotation = rightClawTarget.localRotation;
	}

	public override void OnActivate(Frame frame) {
		if (!frame.TryGet<KCC>(EntityRef, out var kcc))
			return;

		KCCSettings settings = frame.FindAsset(kcc.Settings);
		foreach (var processorRef in settings.Processors) {
			KCCProcessor processor = frame.FindAsset(processorRef);
			if (processor is EnvironmentProcessor env) {
				maxJumpSpeed = env.JumpMultiplier.AsFloat;
				break;
			}
		}
	}

	public override void OnUpdateView() {
		var frame = usePredictedFrame ? PredictedFrame : VerifiedFrame;
		if (frame == null)
			return;

		var kcc = frame.Get<KCC>(EntityRef);
		FPVector3 fpVelocity = kcc.Data.KinematicVelocity + kcc.Data.DynamicVelocity;
		Vector3 velocity = new Vector3(fpVelocity.X.AsFloat, fpVelocity.Y.AsFloat, fpVelocity.Z.AsFloat);
		bool isGrounded = kcc.Data.IsGrounded;

		float speed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
		bool isIdle = speed <= moveThreshold && isGrounded;
		float blendT = 1f - Mathf.Exp(-idleSmooth * Time.deltaTime);
		idleBlend = Mathf.Lerp(idleBlend, isIdle ? 1f : 0f, blendT);
		if (isIdle)
			idleTime += Time.deltaTime * idleSpeed;

		float targetJump = Mathf.Clamp(velocity.y / maxJumpSpeed, -1f, 1f);
		float smooth = isGrounded ? landingSmooth : jumpSmooth;
		float leftT = 1f - Mathf.Exp(-(smooth * (1f + jumpVariation)) * Time.deltaTime);
		float rightT = 1f - Mathf.Exp(-(smooth * (1f - jumpVariation)) * Time.deltaTime);
		leftJump = Mathf.Lerp(leftJump, targetJump, leftT);
		rightJump = Mathf.Lerp(rightJump, targetJump, rightT);

		float airT = 1f - Mathf.Exp(-smooth * Time.deltaTime);
		airborneBlend = Mathf.Lerp(airborneBlend, isGrounded ? 0f : 1f, airT);

		float leftPhase = idleTime * Mathf.PI * 2f;
		float rightPhase = leftPhase + Mathf.PI * 2f * oppositePhase;
		UpdateClaw(leftClawTarget, leftBasePosition, leftBaseRotation, leftPhase, 1f, leftJump, airborneBlend);
		UpdateClaw(rightClawTarget, rightBasePosition, rightBaseRotation, rightPhase, -1f, rightJump, airborneBlend);
	}

	private void UpdateClaw(Transform target, Vector3 basePosition, Quaternion baseRotation, float phase, float side, float jump, float airborne) {
		float jumpScale = 1f + jumpVariation * side;

		Vector3 idlePosition = new Vector3(
			Mathf.Sin(phase) * idlePositionAmount.x,
			Mathf.Sin(phase * 0.73f) * idlePositionAmount.y,
			Mathf.Cos(phase * 0.87f) * idlePositionAmount.z
		) * idleBlend;

		Vector3 idleRotation = new Vector3(
			Mathf.Sin(phase * 0.81f) * idleRotationAmount.x,
			Mathf.Cos(phase * 0.67f) * idleRotationAmount.y,
			Mathf.Sin(phase * 0.93f) * idleRotationAmount.z
		) * idleBlend;

		Vector3 jumpPosition = target.parent.InverseTransformDirection(-Vector3.up) * jump * maxJumpPositionOffset * jumpScale;
		Quaternion jumpRotation = Quaternion.Euler(jump * maxJumpRotation * jumpScale, 0f, 0f);
		Vector3 tuckPosition = baseRotation * Vector3.right * bodyTuckPosition * side;
		Quaternion tuckRotation = Quaternion.Euler(0f, 0f, -bodyTuckRotation * side);
		Vector3 spreadPosition = baseRotation * Vector3.right * -jumpSpreadPosition * side * airborne;

		target.localPosition = basePosition + idlePosition + jumpPosition + tuckPosition + spreadPosition;
		target.localRotation = baseRotation * Quaternion.Euler(idleRotation) * jumpRotation * tuckRotation;
	}
}