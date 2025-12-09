using Godot;

namespace Raele.GDirector.VirtualCameraControllers;

public partial class ProximityPriority : VirtualCameraController
{
	[Export] public Node3D? ProximityTarget;
	[Export] public Vector3 ProximityTargetOffset;
	[Export] public float IdealDistance = 5f;
	[Export] public float MaxDistance = 10f;
	[Export] public float MinDistance = 1f;
	[Export] public int PriorityOnIdealDistance = 1;
	[Export(PropertyHint.ExpEasing, "attenuation")] public float PriorityFallOff = 1f;

	public Vector3 ProximityTargetPosition {
		get {
			Transform3D? proximityTargetTransform = this.ProximityTarget?.GlobalTransform;
			Vector3 lookTarget = proximityTargetTransform != null
				? proximityTargetTransform.Value.Origin + proximityTargetTransform.Value.Basis
					* this.ProximityTargetOffset
				: this.ProximityTargetOffset;
			return lookTarget;
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		Vector3 lineToTarget = this.ProximityTargetPosition - this.Camera.GlobalPosition;
		float distance = lineToTarget.Length();
		float priorityMultiplier = distance > this.MaxDistance || distance < this.MinDistance
			? 0
			: distance > this.IdealDistance
				? 1 - Mathf.Ease(
					Mathf.Clamp(distance - this.IdealDistance, 0, this.MaxDistance - this.IdealDistance) / (this.MaxDistance - this.IdealDistance),
					this.PriorityFallOff
				)
				: 1 - Mathf.Ease(
					Mathf.Clamp(this.IdealDistance - distance, 0, this.IdealDistance - this.MinDistance) / (this.IdealDistance - this.MinDistance),
					this.PriorityFallOff
				);
		this.Camera.Priority += this.PriorityOnIdealDistance * priorityMultiplier;
	}
}
