using Godot;

namespace Raele.GDirector.VirtualCameraComponents;

public partial class FramingPriority3D : VirtualCameraComponent3D
{
	/// <summary>
	/// The target to frame.
	///
	/// If this property is not set, the controller will track the framing of a specific point in world-coordinates, as
	/// determined by the <see cref="FramingTargetOffset"/> property.
	/// </summary>
	[Export] public Node3D? FramingTarget;
	/// <summary>
	/// The offset from the target's position to frame.
	///
	/// If the framing target is not set, this controller will track the framing of a point in space that is this offset
	/// from the world origin.
	/// </summary>
	[Export] public Vector3 FramingTargetOffset;
	[Export] public int PriorityOnCenter = 1;
	/// <summary>
	/// The fall-off of the priority as the target moves away from the center of the frame.
	///
	/// In the horizontal axis, the start of the curve is when the framing target is on the center of the camera, and
	/// the end is when the character is 90 degrees or more from the forward direction of the camera.
	///
	/// In the vertical axis, high values mean the priority added to the virtual camera is closer to the value set in
	/// the <see cref="PriorityOnCenter"/> property, and low values mean the priority added to the virtual camera will
	/// be closer to zero.
	/// </summary>
	[Export(PropertyHint.ExpEasing, "attenuation")] public float PriorityFallOff = 1f;

	public Vector3 FramingTargetPosition {
		get {
			Transform3D? framingTargetTransform = this.FramingTarget?.GlobalTransform;
			Vector3 lookTarget = framingTargetTransform != null
				? framingTargetTransform.Value.Origin + framingTargetTransform.Value.Basis * this.FramingTargetOffset
				: this.FramingTargetOffset;
			return lookTarget;
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		Vector3 forward = this.Camera.GlobalTransform.Basis.Z * -1;
		Vector3 lineToTarget = this.FramingTargetPosition - this.Camera.GlobalPosition;
		Vector3 directionToTarget = lineToTarget.Normalized();
		float dot = forward.Dot(directionToTarget);
		float priorityMultiplier = Mathf.Ease(Mathf.Clamp(dot, 0, 1), this.PriorityFallOff);
		this.Camera.Priority += this.PriorityOnCenter * priorityMultiplier;
	}
}
