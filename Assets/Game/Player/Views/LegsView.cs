using UnityEngine;
using Quantum;
using Photon.Deterministic;
using FIMSpace.FProceduralAnimation;

public class LegsView : QuantumEntityViewComponent {
	[SerializeField] private LegsAnimator legsAnimator;

	[Header("View")]
	[SerializeField] private bool usePredictedFrame = true;
	[SerializeField] private float movingThreshold = 0.05f;
	[SerializeField] private float velocitySmooth = 20f;

	private Vector3 smoothedVelocity;

	public override void OnUpdateView() {
		var frame = usePredictedFrame ? PredictedFrame : VerifiedFrame;
		if (frame == null)
			return;

		var kcc = frame.Get<KCC>(EntityRef);

		FPVector3 fpVelocity =
			kcc.Data.KinematicVelocity +
			kcc.Data.DynamicVelocity;

		Vector3 velocity = new Vector3(
			fpVelocity.X.AsFloat,
			0f,
			fpVelocity.Z.AsFloat
		);

		float t = 1f - Mathf.Exp(-velocitySmooth * Time.deltaTime);
		smoothedVelocity = Vector3.Lerp(smoothedVelocity, velocity, t);

		float speed = smoothedVelocity.magnitude;
		bool isMoving = speed > movingThreshold;

		legsAnimator.User_SetDesiredMovementDirection(
			isMoving ? smoothedVelocity : Vector3.zero
		);

		legsAnimator.User_SetIsMoving(isMoving);
		legsAnimator.User_SetIsGrounded(kcc.Data.IsGrounded);
	}
}