using System;
using Godot;

namespace Raele.GDirector.VirtualCamera3DComponents;

[Tool]
public partial class FramingComponent3D : VirtualCamera3DComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Node3D? FramingTarget;
	[Export] public Vector3 TargetOffset;
	// [Export] public bool OffsetIsGlobal = false; // TODO
	[Export(PropertyHint.Range, "0,1")] public Vector2 ScreenPosition = new Vector2(0.5f, 0.5f);
	[Export] public MovementModeEnum MovementPlane = MovementModeEnum.Global_XZ_Plane;

	// TODO
	// [Export(PropertyHint.Range, "0,1")] public float DeadZoneRadius = 0;

	// TODO
	// [Export] public float MinDistance = 4;
	// [Export] public float MaxDistance = float.PositiveInfinity;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------



	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private Vector2 ScreenPositionPx => this.GetViewport().GetWindow().Size * this.ScreenPosition;
	private Plane MovementPlaneAsPlane => this.MovementPlane switch {
		MovementModeEnum.Global_XZ_Plane => new Plane(Vector3.Up, this.Camera.As3D()!.GlobalPosition),
		MovementModeEnum.Global_XY_Plane => new Plane(Vector3.Forward, this.Camera.As3D()!.GlobalPosition),
		MovementModeEnum.Global_YZ_Plane => new Plane(Vector3.Right, this.Camera.As3D()!.GlobalPosition),
		MovementModeEnum.Local_XY_Plane => new Plane(this.Camera.As3D()!.GlobalBasis.Z * -1, this.Camera.As3D()!.GlobalPosition),
		_ => throw new NotImplementedException("VirtualCamera's FramingConstraint node is set to an invalid MovementPlane option."),
	};
	private Vector3 FramingTargetOffsetedPosition => this.FramingTarget != null
		? this.FramingTarget.GlobalPosition + this.TargetOffset * this.FramingTarget.GlobalBasis
		: Vector3.Zero;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	public enum MovementModeEnum {
		Global_XZ_Plane,
		Global_XY_Plane,
		Global_YZ_Plane,
		Local_XY_Plane,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	// public override void _EnterTree()
	// {
	// 	base._EnterTree();
	// }

	// public override void _Ready()
	// {
	// 	base._Ready();
	// }

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.FramingTarget == null) {
			return;
		}

		// TODO
		// Vector2 framingTargetScreenPosition = GDirectorServer.Instance.ManagedCamera.UnprojectPosition(this.FramingTargetOffsetedPosition) / this.GetViewport().GetWindow().Size;
		// if (framingTargetScreenPosition.DistanceTo(this.ScreenPosition) < this.DeadZoneRadius) {
		// 	return;
		// }

		Vector3 screenPositionNormal = this.Camera.GlobalBasis * GDirectorServer.Instance.GodotCamera3D?.ProjectLocalRayNormal(this.ScreenPositionPx) ?? Vector3.Zero;
		this.Camera.GlobalPosition = this.MovementPlaneAsPlane.IntersectsRay(this.FramingTargetOffsetedPosition, screenPositionNormal * -1) ?? this.Camera.GlobalPosition;
	}

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------


}
