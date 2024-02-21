using Godot;

namespace Raele.GDirector.VirtualCameraControllers;

public partial class LookAtTargetController : VirtualCameraController
{
	[Export] public Node3D? LookTarget;
	/// <summary>
	/// This is the offset from the target position. The offset takes into account the global transform of the target,
	/// including its basis of rotation and scale.
	///
	/// For example, if this is (1, 0, 0), the camera will look at a location that is 1 global unit of distance to the
	/// right of the target, according to their global basis of rotation, times the target's scale on the X axis.
	/// </summary>
	[Export] public Vector3 OffsetPosition;
	/// <summary>
	/// This is the rotation offset of the camera, in degrees. This is a rotation applied to the camera after
	/// calculating the direction toward the look target.
	/// </summary>
	[Export] public Vector2 RotationOffsetDeg;

	[ExportGroup("Smoothing")]
	[Export] public float LerpWeight = 1f;
	// [Export] public float MaxAngleDiffDeg = float.PositiveInfinity; // TODO

	private Vector3 LookPosition;
	public float RotationOffsetRadX => Mathf.DegToRad(this.RotationOffsetDeg.X);
	public float RotationOffsetRadY => Mathf.DegToRad(this.RotationOffsetDeg.Y);

    public Vector3 OffsetedLookTargetPosition {
		get {
			if (this.LookTarget == null) {
				return Vector3.Zero;
			}
			Transform3D transform = this.LookTarget.GlobalTransform;
			return transform.Origin + transform.Basis * this.OffsetPosition;
		}
	}

    public override void _Ready()
	{
		base._Ready();
		this.LookPosition = this.OffsetedLookTargetPosition;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.LookTarget == null) {
			return;
		}
		this.LookPosition = this.LookPosition.Lerp(this.OffsetedLookTargetPosition, this.LerpWeight);
		Vector3 lookPositionDirection = (this.LookPosition - this.Camera.GlobalPosition).Normalized();
		if (lookPositionDirection != Vector3.Zero) {
			Basis lookPositionBasis = Basis.LookingAt(
				lookPositionDirection,
				GodotUtil.CheckNormalsAreParallel(lookPositionDirection, Vector3.Up)
					? this.Camera.GlobalTransform.Basis.Y
					: Vector3.Up
			);
			Basis newBasis = lookPositionBasis
				.Rotated(lookPositionBasis.X, this.RotationOffsetRadY)
				.Rotated(Vector3.Up, this.RotationOffsetRadX);
			this.Camera.GlobalTransform = new Transform3D(newBasis, this.Camera.GlobalPosition);
		}
	}
}
