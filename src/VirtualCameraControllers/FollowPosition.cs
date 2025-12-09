using Godot;

namespace Raele.GDirector.VirtualCameraControllers;

/// <summary>
/// This controller makes the camera move alongside a target node. It can mimic it's the follow target's position
/// directly, or move at an offsetted position.
///
/// You can set a deadzone radius where the camera will remain still while the target moves within it. If the target is
/// moves outside the deadzone radius, the camera will move toward it or alway from it, as appropriate, to keep the
/// distance within the valid range.
///
/// This camera controller is useful for following objects that move at high speeds, but it's not ideal for
/// player-controlled characters in most games.
/// </summary>
public partial class FollowPosition : VirtualCameraController
{
	/// <summary>
	/// The node to follow.
	/// </summary>
	[Export] public Node3D? FollowTarget;
	/// <summary>
	/// The offset to apply to the follow target in the target's local space.
	/// </summary>
	[Export] public Vector3 FollowTargetOffset;
	/// <summary>
	/// The maximum distance the camera can be from the follow target. If the camera is farther than this distance from
	/// the follow target, it will move directly to the follow target to keep the distance within the valid range.
	///
	/// This value must be greater than the <see cref="DeadZoneFartherLimit"/>.
	/// </summary>
	[Export] public float MaxDistance = float.PositiveInfinity;
	/// <summary>
	/// The radius of the dead zone around the follow target. The camera will remain still for as long as the follow
	/// target is within at least this distance and the distance set by <see cref="DeadZoneCloserLimit"/>.
	///
	/// This value must be greater than or equal to the <see cref="DeadZoneCloserLimit"/> and lower than or equal to the
	/// <see cref="MaxDistance"/>.
	/// </summary>
	[Export] public float DeadZoneFartherLimit = 0;
	/// <summary>
	/// The radius of the dead zone around the follow target. The camera will remain still for as long as the follow
	/// target is within at least this distance and the distance set by <see cref="DeadZoneFartherLimit"/>.
	///
	/// This value must be greater than or equal to the <see cref="MinDistance"/> and lower than or equal to the
	/// <see cref="DeadZoneFartherLimit"/>.
	/// </summary>
	[Export] public float DeadZoneCloserLimit = 0;
	/// <summary>
	/// The minimum distance the camera can be from the follow target. If the camera is closer than this distance from
	/// the follow target, it will move directly alway from it to keep the distance within the valid range.
	///
	/// This value must be lower than or equal to the <see cref="DeadZoneCloserLimit"/>.
	/// </summary>
	[Export] public float MinDistance;
	/// <summary>
	/// This is the weight of the linear interpolation used to smoothly move the camera toward the follow target while
	/// it's outside the deadzone radius. (see <see cref="DeadZoneCloserLimit"/>)
	///
	/// Values closer to 1 mean less smoothing, and values closer to 0 mean more smoothing. If this is set to 1, the
	/// camera will always be within the deadzone radius or in it's limites. If it's set to 0, the camera will
	/// ignore the deadzone radius and will only move when it's farther than the <see cref="MaxDistance"/>.
	/// </summary>
	[Export(PropertyHint.Range, "0,1,0.01")] public float LerpWeight = 1f;

	public Vector3 FollowTargetPosition {
		get {
			Transform3D? transform = this.FollowTarget?.GlobalTransform;
			return transform != null
				? transform.Value.Origin + transform.Value.Basis * this.FollowTargetOffset
				: this.FollowTargetOffset;
		}
	}

    public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.FollowTarget == null) {
			GD.PushWarning(nameof(FollowPosition) + " node requires a valid FollowTarget, but it's empty.");
			return;
		}

		// Apply the offset to the follow target
		Vector3 followTargetPosition = this.FollowTargetPosition; // Read FollowTargetPosition only once bc it has some computational cost

		// Calculate the distance between the camera and the follow target
		float currentDistance = this.Camera.GlobalPosition.DistanceTo(followTargetPosition);

		// Calculate the direction from the camera to the follow target
		Vector3 cameraDirection = currentDistance > Mathf.Epsilon
			? (this.Camera.GlobalPosition - followTargetPosition).Normalized()
			: this.Camera.GlobalTransform.Basis.Z;

		// Move the camera according to it's position relative to the follow target
		if (currentDistance > this.MaxDistance) {
			this.Camera.GlobalPosition = followTargetPosition + cameraDirection * this.MaxDistance;
		} else if (currentDistance > this.DeadZoneFartherLimit) {
			Vector3 targetPosition = followTargetPosition + cameraDirection * this.DeadZoneFartherLimit;
			this.Camera.GlobalPosition = this.Camera.GlobalPosition.Lerp(targetPosition, this.LerpWeight);
		} else if (currentDistance < this.MinDistance) {
			this.Camera.GlobalPosition = followTargetPosition + cameraDirection * this.MinDistance;
		} else if (currentDistance < this.DeadZoneCloserLimit) {
			Vector3 targetPosition = followTargetPosition + cameraDirection * this.DeadZoneCloserLimit;
			this.Camera.GlobalPosition = this.Camera.GlobalPosition.Lerp(targetPosition, this.LerpWeight);
		}
	}
}
