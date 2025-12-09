using Godot;

namespace Raele.GDirector.VirtualCameraControllers;

public partial class LookAtTarget : VirtualCameraController
{
	[Export] public Node3D? LookTarget;
	/// <summary>
	/// This is the offset from the target position. The offset takes into account the global transform of the target,
	/// including its basis of rotation and scale.
	///
	/// For example, if this property is set to (1, 0, 0), the camera will look at a location that is 1 unit of distance
	/// to the right of the target, according to their global basis of rotation. This property takes into account the
	/// global transform of the target, including its basis of rotation and scale.
	/// </summary>
	[Export] public Vector3 OffsetPosition;
	/// <summary>
	/// This is the rotation offset of the camera, in degrees. This rotation angle is applied to the camera after
	/// calculating the rotation to face the look target.
	///
	/// For example, If it is set to (0, 30), the camera will look slightly to the right of the look target (it rotates
	/// 30 degrees in the Y axis), framing the target in the left side of the screen.
	/// </summary>
	[Export] public Vector2 OffsetRotationDeg;

	[ExportGroup("Angle Limit")]
	/// <summary>
	/// This is the maximum angle the camera is able to rotate from it's initial facing direction,
	/// in degrees.
	///
	/// By default, this property is set to infinity, meaning the camera is able to rotate freely in any direction.
	/// </summary>
	[Export] public float MaxAngleDeg = float.PositiveInfinity;
	/// <summary>
	/// If <see cref="MaxAngleDeg"/> is set to a value other than its default, this property is used to determine the
	/// reference angle from which the angle limit is calculated from. If this property is null, then the neutral angle
	/// is the initial facing direction of the camera. If this property is set to a node, then the neutral angle is the
	/// direction to that node. This means the camera will attempt to look at the <see cref="LookTarget"/> without
	/// rotating more than <see cref="MaxAngleDeg"/> degrees from the direction to the node set in this property.
	/// </summary>
	[Export] public Node3D? LookAnchor;

	[ExportGroup("Smoothing")]
	[Export(PropertyHint.Range, "0.01,1,0.01")] public float LerpWeight = 1f;
	// [Export] public float MaxAngleDiffDeg = float.PositiveInfinity; // TODO

	private Vector3 lookPosition;
	private Vector3 initialDirection;

	public float MaxAngleRad => Mathf.DegToRad(this.MaxAngleDeg);
	public float OffsetRotationRadX => Mathf.DegToRad(this.OffsetRotationDeg.X);
	public float OffsetRotationRadY => Mathf.DegToRad(this.OffsetRotationDeg.Y);

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
		this.initialDirection = this.Camera.GlobalBasis.Z * -1;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.LookTarget == null || this.OffsetedLookTargetPosition == this.Camera.GlobalPosition) {
			return;
		}

		this.lookPosition = this.lookPosition.Lerp(this.OffsetedLookTargetPosition, this.LerpWeight);

		if (this.lookPosition.IsEqualApprox(this.Camera.GlobalPosition)) {
			return;
		}

		Vector3 lookPositionDirection = (this.lookPosition - this.Camera.GlobalPosition).Normalized();

		if (this.MaxAngleDeg < float.PositiveInfinity) {
			Vector3 neutralDirection = this.LookAnchor != null
				? (this.LookAnchor.GlobalPosition - this.Camera.GlobalPosition).Normalized()
				: this.initialDirection;
			float angle = neutralDirection.AngleTo(lookPositionDirection);
			if (angle > this.MaxAngleRad) {
				lookPositionDirection = GodotUtil.RotateToward(neutralDirection, lookPositionDirection, this.MaxAngleRad);
			}
		}

		Vector3 upwardAxis = GodotUtil.CheckNormalsAreParallel(lookPositionDirection, Vector3.Up)
				? this.Camera.GlobalBasis.Y
				: Vector3.Up;
		Basis newLookPositionBasis = Basis.LookingAt(lookPositionDirection, upwardAxis);
		Basis newBasis = newLookPositionBasis
			.Rotated(newLookPositionBasis.X, this.OffsetRotationRadY)
			.Rotated(Vector3.Up, this.OffsetRotationRadX);
		this.Camera.GlobalTransform = new Transform3D(newBasis, this.Camera.GlobalPosition);
	}
}
