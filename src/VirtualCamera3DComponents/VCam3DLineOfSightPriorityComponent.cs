using Godot;

namespace Raele.GDirector.VirtualCamera3DComponents;

/// <summary>
/// This controller adds priority to the virtual camera when the target is in the line of sight of the camera.
///
/// If the target is not in the line of sight, the priority will fall off over time until it reaches zero. You can
/// configure how quickly the priority falls off  with the <see cref="FallOffDurationSec"/> and
/// <see cref="PriorityFallOff"/> properties. This is useful to prevent a camera cut when a target just breafly passes
/// behind an obstacle that breaks line of sight.
///
/// Note: This controller determines line of sight by casting a ray from the camera to the target's origin position (and
/// an optional offset). If the target has a mesh (e.g. a character) this controller might determine the target is out
/// of sight even though parts of it's body other than it's origin position might still be visible.
/// </summary>
public partial class VCam3DLineOfSightPriorityComponent : VirtualCamera3DComponent
{
	[Export] public Node3D? LineOfSightTarget;
	[Export] public Vector3 LineOfSightTargetOffset;
	[Export] public float PriorityOnLineOfSight = 1;
	[Export] public float FallOffDurationSec = 0.25f;
	[Export(PropertyHint.ExpEasing, "attenuation")] public float PriorityFallOff = 1f;

	[ExportGroup("Collision Mask")]
	[Export(PropertyHint.Layers3DPhysics)] public uint ObstaclesMask = 1;

	public Vector3 LineOfSightTargetPosition {
		get {
			Transform3D? lineOfSightTargetTransform = this.LineOfSightTarget?.GlobalTransform;
			Vector3 lookTarget = lineOfSightTargetTransform != null
				? lineOfSightTargetTransform.Value.Origin + lineOfSightTargetTransform.Value.Basis
					* this.LineOfSightTargetOffset
				: this.LineOfSightTargetOffset;
			return lookTarget;
		}
	}

	private float LOSBrokenTimerSec = 0;
	private bool HasLOS = true;

	public override void _Process(double delta)
	{
		base._Process(delta);
		this.LOSBrokenTimerSec = this.HasLOS
			? 0
			: this.LOSBrokenTimerSec + (float) delta;
		float fallOffMultiplier = this.FallOffDurationSec > 0
			? 1 - Mathf.Ease(
				Mathf.Clamp(this.LOSBrokenTimerSec, 0, this.FallOffDurationSec) / this.FallOffDurationSec,
				this.PriorityFallOff
			)
			: this.LOSBrokenTimerSec > 0
				? 0
				: 1;
		this.Camera.Priority += this.PriorityOnLineOfSight * fallOffMultiplier;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		PhysicsDirectSpaceState3D spaceState = this.Camera.GetWorld3D().DirectSpaceState;
		Godot.Collections.Dictionary result = spaceState.IntersectRay(new() {
			From = this.Camera.GlobalPosition,
			To = this.LineOfSightTargetPosition,
			CollisionMask = this.ObstaclesMask,
		});
		this.HasLOS = result.Count == 0;
	}
}
