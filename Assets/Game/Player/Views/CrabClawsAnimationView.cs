using UnityEngine;
using Quantum;
using Photon.Deterministic;

public class CrabClawsAnimationView : QuantumEntityViewComponent {
	[Header("View")]
	[SerializeField] bool usePredictedFrame = true;
	[SerializeField] Transform leftClaw;
	[SerializeField] Transform rightClaw;
	[SerializeField] Transform leftClawTarget;
	[SerializeField] Transform rightClawTarget;

	[Header("Body Tuck")]
	[SerializeField] float bodyTuckPosition = 0.05f;
	[SerializeField] float bodyTuckRotation = 5f;

	[Header("Idle Movement")]
	[SerializeField] float moveThreshold = 1f;
	[SerializeField] float idleSpeed = 0.5f;
	[SerializeField] float idleSmooth = 5f;
	[SerializeField] Vector3 idlePositionAmount = new Vector3(0.005f, 0.003f, 0.001f);
	[SerializeField] Vector3 idleRotationAmount = new Vector3(2f, 2f, 3f);
	[SerializeField] float oppositePhase = 0.5f;

	Vector3 leftBasePosition;
	Vector3 rightBasePosition;
	Quaternion leftBaseRotation;
	Quaternion rightBaseRotation;
	float idleTime;
	float idleBlend;

	void Awake() {
		leftBasePosition = leftClawTarget.localPosition;
		rightBasePosition = rightClawTarget.localPosition;
		leftBaseRotation = leftClawTarget.localRotation;
		rightBaseRotation = rightClawTarget.localRotation;
	}

	//public override void OnUpdateView() {
	//	var frame = usePredictedFrame ? PredictedFrame : VerifiedFrame;
	//	if (frame == null)
	//		return;

	//	var kcc = frame.Get<KCC>(EntityRef);
	//	FPVector3 fpVelocity = kcc.Data.KinematicVelocity + kcc.Data.DynamicVelocity;
	//	Vector3 velocity = new Vector3(fpVelocity.X.AsFloat, fpVelocity.Y.AsFloat, fpVelocity.Z.AsFloat);
	//	float speed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
	//	bool isIdle = speed <= moveThreshold && kcc.Data.IsGrounded;

	//	float blendT = 1f - Mathf.Exp(-idleSmooth * Time.deltaTime);
	//	idleBlend = Mathf.Lerp(idleBlend, isIdle ? 1f : 0f, blendT);

	//	if (isIdle)
	//		idleTime += Time.deltaTime * idleSpeed;

	//	if (isIdle) {
	//		float leftPhase = idleTime * Mathf.PI * 2f;
	//		float rightPhase = leftPhase + Mathf.PI * 2f * oppositePhase;
	//		UpdateIdleClaw(leftClawTarget, leftBasePosition, leftBaseRotation, leftPhase, 1f);
	//		UpdateIdleClaw(rightClawTarget, rightBasePosition, rightBaseRotation, rightPhase, -1f);
	//	} else {
	//		UpdatePhysicalClaw(leftClawTarget, leftClaw);
	//		UpdatePhysicalClaw(rightClawTarget, rightClaw);
	//	}
	//}

	void UpdateIdleClaw(Transform target, Vector3 basePosition, Quaternion baseRotation, float phase, float side) {
		Vector3 idlePosition = new Vector3(Mathf.Sin(phase) * idlePositionAmount.x, Mathf.Sin(phase * 0.73f) * idlePositionAmount.y, Mathf.Cos(phase * 0.87f) * idlePositionAmount.z) * idleBlend;
		Vector3 idleRotation = new Vector3(Mathf.Sin(phase * 0.81f) * idleRotationAmount.x, Mathf.Cos(phase * 0.67f) * idleRotationAmount.y, Mathf.Sin(phase * 0.93f) * idleRotationAmount.z) * idleBlend;
		Vector3 tuckPosition = baseRotation * Vector3.right * bodyTuckPosition * side;
		Quaternion tuckRotation = Quaternion.Euler(0f, 0f, -bodyTuckRotation * side);

		target.localPosition = basePosition + idlePosition + tuckPosition;
		target.localRotation = baseRotation * Quaternion.Euler(idleRotation) * tuckRotation;
	}

	void LateUpdate() {
		UpdatePhysicalClaw(leftClawTarget, leftClaw);
		UpdatePhysicalClaw(rightClawTarget, rightClaw);
	}

	void UpdatePhysicalClaw(Transform target, Transform claw) {
		if (target == null || claw == null)
			return;

		target.SetPositionAndRotation(claw.position, claw.rotation);
	}
}