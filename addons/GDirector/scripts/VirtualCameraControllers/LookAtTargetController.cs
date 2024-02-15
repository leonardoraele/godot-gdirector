using Godot;

namespace Raele.GDirector.VirtualCameraControllers;

public partial class LookAtTargetController : VirtualCameraController
{
	[Export] public Node3D? LookTarget;
	[Export] public Vector3 LocalOffset;
	[Export] public Vector3 GlobalOffset;

	[ExportGroup("Smoothing")]
	[Export] public float LerpWeight = 1f;
	[Export] public float MaxAngleDiffDeg = float.PositiveInfinity;

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.LookTarget == null) {
			return;
		}
		Vector3 targetPosition = this.LookTarget.GlobalPosition
			+ this.GlobalOffset
			+ this.LocalOffset * this.LookTarget.Basis;
		Vector3 targetDirection = (targetPosition - this.Camera.GlobalPosition).Normalized();
		if (targetDirection != Vector3.Zero) {
			this.Camera.LookAt(
				targetPosition,
				GodotUtil.CheckNormalsAreParallel(targetDirection, Vector3.Up)
					? this.Camera.GlobalTransform.Basis.Y
					: Vector3.Up
			);
		}
	}
}
