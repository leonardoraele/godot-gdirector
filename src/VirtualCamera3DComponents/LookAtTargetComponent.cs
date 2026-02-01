using System.Linq;
using Godot;
using Godot.Collections;
using Raele.GodotUtils.Extensions;

namespace Raele.GDirector.VirtualCamera3DComponents;

[Tool][GlobalClass]
public partial class LookAtTargetComponent : VirtualCamera3DComponent
{
	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	[Export] public Node3D? LookTarget;
	[Export] public UpDirectionEnum UpDirection
		{ get; set { field = value; this.UpdateConfigurationWarnings(); } }
		= UpDirectionEnum.WorldUp;
	[Export] public PhysicsBody3D? ReferenceBody
		{ get; set { field = value; this.UpdateConfigurationWarnings(); } }

	[ExportGroup("Offset")]
	/// <summary>
	/// This is the offset from the target position. The offset takes into account the global transform of the target,
	/// including its basis of rotation and scale.
	///
	/// For example, if this property is set to (1, 0, 0), the camera will look at a location that is 1 unit of distance
	/// to the right of the target, according to their global basis of rotation. This property takes into account the
	/// global transform of the target, including its basis of rotation and scale.
	/// </summary>
	[Export(PropertyHint.None, "suffix:m")] public Vector3 OffsetPosition;
	/// <summary>
	/// This is the rotation offset of the camera, in degrees. This rotation angle is applied to the camera after
	/// calculating the rotation to face the look target.
	///
	/// For example, If it is set to (0, 30), the camera will look slightly to the right of the look target (it rotates
	/// 30 degrees in the Y axis), framing the target in the left side of the screen.
	/// </summary>
	[Export(PropertyHint.None, "radians_as_degrees,suffix:°")] public Vector2 OffsetRotation;

	[ExportGroup("Smoothing")]
	[Export(PropertyHint.Range, "0,100,1")] public float SmoothingFactor = 0f;
	// [Export] public float MaxAngleDiffDeg = float.PositiveInfinity; // TODO

	[ExportGroup("Limit Rotation")]
	[Export(PropertyHint.GroupEnable)] public bool LimitEnabled = false;
	/// <summary>
	/// This property is used to determine the rest look direction of the camera. This means the camera will attempt to
	/// look at the <see cref="LookTarget"/> without rotating more than <see cref="MaxAngle"/> degrees from the
	/// direction to the node set in this property. If this property is left null, then the rest direction is the
	/// initial facing direction of the camera.
	///
	/// Use this property when you want to lock the camera's direction toward this look anchor while still allowing the
	/// camera to rotate toward the look target within the allowed angle.
	/// </summary>
	[Export] public Node3D? LookAnchor;
	/// <summary>
	/// This is the maximum angle the camera is able to rotate from it's initial facing direction,
	/// in degrees.
	///
	/// By default, this property is set to infinity, meaning the camera is able to rotate freely in any direction.
	/// </summary>
	[Export(PropertyHint.Range, "0,180,5,radians_as_degrees,suffix:°")] public float MaxAngle = Mathf.Pi / 6;

	// TODO
	// // The options in this group are used to limit how the camera can rotate. They can be useful, for example, to
	// // simulate a securtiy camera that can only rotate in 8 directions and at a fixed speed.
	// [ExportGroup("Limit Movement")]
	// [Export(PropertyHint.GroupEnable)] public bool LimitMovementEnabled = false;
	// [Export(PropertyHint.None, "suffix:°/s")] public float AngularVelocity = float.PositiveInfinity;
	// [Export(PropertyHint.None, "suffix:°")] public float AngleStep = Mathf.Pi / 4;

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	private Vector3 SmoothedTargetPosition;
	private Vector3 InitialDirection;

	//==================================================================================================================
	// COMPUTED PROPERTIES
	//==================================================================================================================

	private float LerpWeight => 1f / (this.SmoothingFactor + 1f);

	//==================================================================================================================
	// INTERNAL TYPES
	//==================================================================================================================

	public enum UpDirectionEnum : sbyte
	{
		WorldUp = 16,
		TargetUp = 32,
		GravityUp = 64,
		None = 127,
	}

	//==================================================================================================================
	// OVERRIDES
	//==================================================================================================================

	public override string[] _GetConfigurationWarnings()
		=> (base._GetConfigurationWarnings() ?? [])
			.AppendIf(
				this.UpDirection == UpDirectionEnum.GravityUp &&  this.ReferenceBody == null,
				$"Field '{nameof(this.ReferenceBody)}' is mandatory when {nameof(this.UpDirection)} is set to {UpDirectionEnum.GravityUp}."
			)
			.ToArray();

	public override void _ValidateProperty(Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.UpDirection):
				property["usage"] = (long) PropertyUsageFlags.Default | (long) PropertyUsageFlags.UpdateAllIfModified;
				break;
			case nameof(this.ReferenceBody):
				if (this.UpDirection != UpDirectionEnum.GravityUp)
					property["usage"] = (long) PropertyUsageFlags.None;
				break;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		this.InitialDirection = this.Camera.GlobalBasis.Z * -1;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		Vector3 cameraPos = this.Camera.GlobalPosition;
		if (this.LookTarget == null)
			return;
		Vector3 lookTarget = this.GetOffsetedLookTarget();
		if (lookTarget.IsEqualApprox(cameraPos))
			return;
		this.SmoothedTargetPosition = this.SmoothedTargetPosition.Lerp(lookTarget, this.LerpWeight);
		if (this.SmoothedTargetPosition.IsEqualApprox(cameraPos))
			return;
		Vector3 lookDirection = (this.SmoothedTargetPosition - cameraPos).Normalized();
		if (this.LimitEnabled)
		{
			Vector3 neutralDirection = this.GetDirectionToLookAnchor();
			float angle = neutralDirection.AngleTo(lookDirection);
			if (angle > this.MaxAngle) {
				lookDirection = neutralDirection.RotateToward(lookDirection, this.MaxAngle);
			}
		}
		this.Camera.GlobalBasis = this.ApplyOffsetRotation(Basis.LookingAt(lookDirection, this.GetUpDirection()));
	}

	//==================================================================================================================
	// METHODS
	//==================================================================================================================

	public Vector3 GetOffsetedLookTarget()
	{
		if (this.LookTarget == null)
			return Vector3.Zero;
		return this.LookTarget.GlobalTransform * this.OffsetPosition;
	}

	public Vector3 GetDirectionToLookAnchor()
	{
		if (this.LookAnchor == null)
			return this.InitialDirection;
		Vector3 diff = this.LookAnchor.GlobalPosition - this.Camera.GlobalPosition;
		return diff.IsZeroApprox()
			? this.Camera.GlobalBasis.Z * -1
			: diff.Normalized();
	}

	private Basis ApplyOffsetRotation(Basis basis)
		=> basis
			.Rotated(basis.X, this.OffsetRotation.Y)
			.Rotated(Vector3.Up, this.OffsetRotation.X);

	private Vector3 GetUpDirection()
		=> this.UpDirection switch
		{
			UpDirectionEnum.WorldUp => Vector3.Up,
			UpDirectionEnum.TargetUp when this.LookTarget != null => this.LookTarget.GlobalBasis.Up,
			UpDirectionEnum.GravityUp
				when this.ReferenceBody != null
				&& this.ReferenceBody.GetGravity() is Vector3 gravity
				&& !gravity.IsZeroApprox()
					=> gravity * -1,
			_ => this.Camera.GlobalBasis.Y,
		};
}
