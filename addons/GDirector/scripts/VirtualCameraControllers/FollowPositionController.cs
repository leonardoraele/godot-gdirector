using Godot;

namespace Raele.GDirector.VirtualCameraControllers;

public partial class FollowPositionController : VirtualCameraController
{
	[Export] public Node3D? FollowTarget;
	[Export] public Vector3 LocalOffset;
	[Export] public Vector3 GlobalOffset;

	[ExportGroup("Smoothing")]
	[Export(PropertyHint.Range, "0.01,1,0.01")] public float LerpWeight = 1f;
	[Export] public float DeadZoneRadius = 0;
	[Export] public float MaxDistance = float.PositiveInfinity;

    public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.FollowTarget == null) {
			GD.PushWarning(nameof(FollowPositionController) + " node requires a valid FollowTarget, but it's empty.");
			return;
		}

		// Apply the offset to the follow target
		Vector3 offesetFollowTarget = this.FollowTarget.GlobalPosition
			+ this.LocalOffset * this.FollowTarget.Basis
			+ this.GlobalOffset;

		// Calculate the position to move the camera to
		Vector3 targetPosition = this.Camera.GlobalPosition.DirectionTo(offesetFollowTarget)
			* (this.Camera.GlobalPosition.DistanceTo(offesetFollowTarget) - this.DeadZoneRadius);

		// Lerp toward the target position
		float distanceToTarget = this.Camera.GlobalPosition.DistanceTo(targetPosition);

		// Check if it's able to lerp toward the target and still be within valid distance of the target, then lerp;
		// otherwise move directly to the target to keep the camera within the valid distance.
		if (distanceToTarget * (1 - this.LerpWeight) > this.MaxDistance) {
			this.Camera.GlobalPosition = targetPosition.MoveToward(this.Camera.GlobalPosition, this.MaxDistance);
		} else {
			this.Camera.GlobalPosition = this.Camera.GlobalPosition.Lerp(targetPosition, this.LerpWeight);
		}
	}
}
