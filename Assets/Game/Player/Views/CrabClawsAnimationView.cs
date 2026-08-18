using UnityEngine;
using Quantum;

public class CrabClawsAnimationView : QuantumEntityViewComponent {
	[SerializeField] bool usePredictedFrame = true;
	[SerializeField] Transform leftClaw;
	[SerializeField] Transform rightClaw;
	[SerializeField] Transform leftClawTarget;
	[SerializeField] Transform rightClawTarget;

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