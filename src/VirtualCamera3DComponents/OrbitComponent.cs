using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.GDirector.VirtualCamera3DComponents;

// TODO Instead of forcing the camera to orbit around the Y axis, allow the player to select the axis around
// which the camera should orbit.
// TODO Add the ability to make Automatic Orbiting move toward a specific direction (e.g. the player's character's back)
// instead of always orbiting around the pivot's Y axis.
// TODO Add a bool parameter to allow the user to select the camera behavior when the camera disaligns from the pivot.
// Right now, the behavior is to preserve the angle of the camera from the pivot, while changing the camera's position
// as needed. The alternative would be to make the move as little as possible while remaining in the orbit, and changing
// the camera angle as much as needed.
[Tool][GlobalClass]
public partial class OrbitComponent : VirtualCamera3DComponent
{
	//==================================================================================================================
	// STATICS & CONSTRUCTORS
	//==================================================================================================================

	//==================================================================================================================
	// EXPORTS
	//==================================================================================================================

	/// <summary>
	/// The pivot around which the camera will orbit.
	///
	/// If this property is not set, the camera will orbit around the world origin.
	/// </summary>
	[Export] public Node3D? Pivot; // Used by pivotPositionCalculation
	/// <summary>
	/// Offset applied to the pivot's position. This is the position the camera will orbit around.
	///
	/// This takes the pivot's global transform into account. i.e. If the pivot is rotated or scaled, this offset will
	/// also be rotated or scaled.
	///
	/// If the pivot is not set, this property offsets from the world origin to determine the position the camera will
	/// orbit around.
	/// </summary>
	[Export] public Vector3 PivotOffset; // Used by pivotPositionCalculation
	/// <summary>
	/// The camera distance from the pivot.
	///
	/// If player-controller zoom is enabled, this is the initial distance the camera will be from the pivot when the
	/// scene starts, and the player will be able to modify this distance at runtime.
	/// </summary>
	[Export(PropertyHint.None, "suffix:m")] public float RestDistance
	{
		get;
		set
		{
			field = value.AtLeast(0);
			if (field < this.MinDistance)
				this.MinDistance = field;
			if (field > this.MaxDistance)
				this.MaxDistance = field;
		}
	}
		= 4f;
	// TODO
	// /// <summary>
	// /// The axis around which the camera will orbit the pivot.
	// /// </summary>
	// [Export] public Vector3.Axis OrbitAxis = Vector3.Axis.Y;
	// /// <summary>
	// /// If enabled, the camera will orbit the pivot around its local axis defined by <see cref="OrbitAxis"> instead of
	// /// the world axis.
	// /// </summary>
	// [Export] public bool OrbitLocalAxis = false;
	/// <summary>
	/// By default, the camera orbits the pivot around the world Y axis. This property determines the angle, from the XZ
	/// plane at the pivot's position, where the camera will inhabit, in degrees.
	///
	/// Positive values rotate the camera upwards, negative values rotate it downwards. A value of 0 (zero) means the
	/// camera will be at the same height as the pivot.
	///
	/// If player-controlled spherical movement is enabled, this is the initial angle the camera will be when the scene
	/// starts, and the player will be able to move the camera around the orbit at runtime.
	/// </summary>
	[Export(PropertyHint.Range, "-90,90,5,radians_as_degrees")] public float Angle = 0f; // Used by cameraDirectionCalculation

	[ExportGroup("Smoothing")]
	[Export(PropertyHint.GroupEnable)] public bool SmoothingEnabled = false;
	/// <summary>
	/// The weight of the camera's spherical movement smoothing. A value of 1 means no smoothing, and values closer to 0
	/// mean more smoothing.
	/// </summary>
	[Export(PropertyHint.Range, "1,100,1")] public int SmoothingFactor = 1;
	[Export(PropertyHint.None, "suffix:m")] public float MinDistance
	{
		get;
		set
		{
			field = value.Clamped(0, this.MaxDistance);
			this.MinDistanceSqr = field * field;
			if (field > this.RestDistance)
				this.RestDistance = field;
		}
	}
		= 0f; // Used by pivotPositionCalculation
	/// <summary>
	/// This is the maximum distance the camera's "virtual" orbit pivot can lag behind the actual pivot's position, in
	/// global position units. Use this to prevent the camera from being too off if the pivot moves too fast and the
	/// smoothing weight is too low.
	///
	/// Note: This is NOT the distance from the camera to the pivot. This is the distance from the point the camera is
	/// orbiting around to the pivot's position, when they differ because of the smoothing applied.
	/// </summary>
	[Export(PropertyHint.None, "suffix:m")] public float MaxDistance
	{
		get;
		set
		{
			field = value.AtLeast(this.MinDistance);
			this.MaxDistanceSqr = field * field;
			if (field < this.RestDistance)
				this.RestDistance = field;
		}
	}
		= float.PositiveInfinity; // Used by pivotPositionCalculation

	[ExportGroup("Automatic Orbiting")]
	/// <summary>
	/// The speed at which the camera will automatically rotate around the pivot, in degrees per second.
	///
	/// If player-controlled spherical movement is enabled, the automatic orbiting is disabled while the player is
	/// controlling the camera.
	/// </summary>
	[Export(PropertyHint.None, "radians_as_degrees,suffix:°/s")] public float AngularVelocity = 0f; // Used by cameraDirectionCalculation
	/// <summary>
	/// The delay, in seconds, after the player stops controlling the camera before the camera starts automatically
	/// rotating around the pivot again.
	/// </summary>
	[Export(PropertyHint.None, "suffix:s")] public float Delay = 0f; // Used by cameraDirectionCalculation

	[ExportGroup("Player Controls")]
	[Export(PropertyHint.GroupEnable)] public bool EnablePlayerControls = false;
	[Export(PropertyHint.None, "radians_as_degrees,suffix:°/s")] public float VerticalVelocity = 1f;
	[Export(PropertyHint.None, "radians_as_degrees,suffix:°/s")] public float HorizontalVelocity = 1f;

	[ExportSubgroup("Input Mapping")]
	[Export(PropertyHint.InputName)] public string InputRotateUp = "camera_look_up";
	[Export(PropertyHint.InputName)] public string InputRotateRight = "camera_look_right";
	[Export(PropertyHint.InputName)] public string InputRotateDown = "camera_look_down";
	[Export(PropertyHint.InputName)] public string InputRotateLeft = "camera_look_left";

	[ExportSubgroup("Mouse Controls", "Mouse")]
	/// <summary>
	/// If enabled, the player can control the camera's spherical movement around the pivot using the mouse.
	/// </summary>
	[Export(PropertyHint.GroupEnable)] public bool MouseEnabled = false;
	/// <summary>
	/// The number of pixels the mouse has to move to rotate the camera by one degree. (assuming mouse sensitivity is 1)
	/// </summary>
	[Export] public float MousePixelsPerDegree = 1f;
	/// <summary>
	/// The mouse's sensitivity when controlling the camera's movement. This is a multiplier applied to the mouse's
	/// horizontal movement.
	/// </summary>
	[Export(PropertyHint.None, "suffix:x")] public Vector2 MouseSensitivity = Vector2.One;
	/// <summary>
	/// The highest vertical angle the camera can move, in degrees the ZX plane at the pivot's position.
	///
	/// Positive angles are rotations toward the positive Y axis, and vice-versa.
	/// </summary>
	[Export(PropertyHint.Range, "-90,90,5,radians_as_degrees")] public float MouseMaxVerticalAngle
		{ get; set { field = value.Clamped(this.MouseMinVerticalAngle, Mathf.Pi); } }
		= Mathf.Pi / 3f;
	/// <summary>
	/// The lowest vertical angle the camera can move, in degrees the ZX plane at the pivot's position.
	///
	/// Negative angles are rotations toward the negative Y axis, and vice-versa.
	/// </summary>
	[Export(PropertyHint.Range, "-90,90,5,radians_as_degrees")] public float MouseMinVerticalAngle
		{ get; set { field = value.Clamped(-Mathf.Pi, this.MouseMaxVerticalAngle); } }
		= 0f;
	// [Export] public Curve? DistanceMultiplierCurve; // TODO Use to change the camera distance based on the angle
	// [Export] public Curve FieldOfViewPerAngle; // TODO Use to change the camera FOV based on the angle

	[ExportSubgroup("Zoom", "Zoom")]
	/// <summary>
	/// If enabled, the player can control the camera's zoom.
	/// </summary>
	[Export(PropertyHint.GroupEnable)] public bool ZoomEnabled = false;
	/// <summary>
	/// The input action the player can use to zoom in the camera.
	/// </summary>
	[Export(PropertyHint.InputName)] public string ZoomInputMoveCloser = "camera_zoom_in";
	/// <summary>
	/// The input action the player can use to zoom out the camera.
	/// </summary>
	[Export(PropertyHint.InputName)] public string ZoomInputMoveFarther = "camera_zoom_out";
	/// <summary>
	/// The minimum distance the player can move the camera towards the pivot.
	/// </summary>
	[Export(PropertyHint.None, "suffix:m")] public float ZoomMinDistance
		{ get; set { field = value.AtMost(ZoomMaxDistance); } }
		= 1f;
	/// <summary>
	/// The maximum distance the player can move the camera away from the pivot.
	/// </summary>
	[Export(PropertyHint.None, "suffix:m")] public float ZoomMaxDistance
		{ get; set { field = value.AtLeast(ZoomMinDistance); } }
		= 10f;
	/// <summary>
	/// The amount of distance units the camera will zoom in or out per tick. (i.e. each time the zoom in or zoom out
	/// input action is activated)
	/// </summary>
	[Export(PropertyHint.None, "suffix:m/s")] public float ZoomVelocity = 10f;

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	private float MaxDistanceSqr;
	private float MinDistanceSqr;

	//==================================================================================================================
	// COMPUTED PROPERTIES
	//==================================================================================================================

	public float SmoothingLerpWeight
		=> this.SmoothingFactor == 0 ? 1f : 1f / this.SmoothingFactor;
	public IEnumerable<Func<Vector3, Vector3>> AllModifiers
	{
		get
		{
			yield return this.ApplyOrbit;
			// yield return this.ApplyAngle;
			yield return this.ApplySmoothing;
			yield return this.ApplyAutomaticOrbiting;
		}
	}

	//==================================================================================================================
	// OVERRIDES
	//==================================================================================================================

	public override void _Process(double delta)
	{
		base._Process(delta);
		this.Camera.GlobalPosition = this.AllModifiers.Aggregate(this.Camera.GlobalPosition, (pos, func) => func(pos));
	}

	// public override void _Input(InputEvent @event)
	// {
	// 	base._Input(@event);
	// 	this.CameraDirectionController.HandleInput(@event);
	// }

	//==================================================================================================================
	// METHODS
	//==================================================================================================================

	/// <summary>
	/// Gets the position closest to the camera that is in the orbit around the pivot, at the rest distance.
	/// </summary>
	private Vector3 ApplyOrbit(Vector3 cameraPosition)
	{
		// TODO pivotPosition is being recalculated for each modifier. It should be cached instead.
		Vector3 pivotPosition = (this.Pivot?.GlobalTransform ?? Transform3D.Identity) * this.PivotOffset;
		return pivotPosition
			+ (pivotPosition.IsEqualApprox(cameraPosition) ? Vector3.Back : pivotPosition.DirectionTo(cameraPosition))
			* this.RestDistance;
	}

	/// <summary>
	/// Rotates the camera position around the pivot so that it is in the right angle.
	/// </summary>
	private Vector3 ApplyAngle(Vector3 cameraPosition)
	{
		// TODO pivotPosition is being recalculated for each modifier. It should be cached instead.
		Vector3 pivotPosition = (this.Pivot?.GlobalTransform ?? Transform3D.Identity) * this.PivotOffset;
		Vector3 relativeCameraPosition = cameraPosition - pivotPosition;
		float currentAngle = relativeCameraPosition.AngleTo(Vector3.Up);
		float targetAngle = Mathf.Pi - this.Angle + Mathf.Pi / 2;
		return pivotPosition + relativeCameraPosition.RotateToward(Vector3.Up, currentAngle - targetAngle);
	}

	/// <summary>
	/// Applies smoothing to the camera's position.
	/// </summary>
	private Func<Vector3, Vector3> ApplySmoothing
	{
		get
		{
			if (field != null)
				return field;
			Vector3 smoothedPosition = this.Camera.GlobalPosition;
			return field = cameraPosition =>
			{
				// TODO pivotPosition is being recalculated for each modifier. It should be cached instead.
				Vector3 pivotPosition = (this.Pivot?.GlobalTransform ?? Transform3D.Identity) * this.PivotOffset;
				smoothedPosition = smoothedPosition.Lerp(cameraPosition, this.SmoothingLerpWeight);
				if (pivotPosition.DistanceSquaredTo(smoothedPosition) > this.MaxDistanceSqr)
					smoothedPosition = pivotPosition.MoveToward(smoothedPosition, this.MaxDistance);
				else if (pivotPosition.DistanceSquaredTo(smoothedPosition) < this.MinDistanceSqr)
					smoothedPosition = pivotPosition + pivotPosition.DirectionTo(smoothedPosition) * this.MinDistance;
				return smoothedPosition;
			};
		}
	}

	private Vector3 ApplyAutomaticOrbiting(Vector3 cameraPosition)
	{
		// TODO pivotPosition is being recalculated for each modifier. It should be cached instead.
		Vector3 pivotPosition = (this.Pivot?.GlobalTransform ?? Transform3D.Identity) * this.PivotOffset;
		return cameraPosition.IsEqualApprox(pivotPosition)
			? cameraPosition
			: cameraPosition.Rotated(Vector3.Up, this.AngularVelocity * (float) this.GetProcessDeltaTime());
	}

	// //==================================================================================================================
	// // CALCULATE CAMERA DIRECTION
	// //==================================================================================================================

	// private class CameraDirectionControllerClass
	// {
	// 	/// <summary>
	// 	/// This is the position the camera was positioned in relation to the pivot the last frame, in global space. If the
	// 	/// pivot moves, the actual direction of the camera in relation to pivot will change, so we use this value to
	// 	/// preserve the camera's direction in relation to the pivot from one frame to the next.
	// 	/// </summary>
	// 	private Vector3 AnchorCameraDirection = Vector3.Back;
	// 	/// <summary>
	// 	/// This is the direction the camera should be positioned in relation to the pivot, in global space.
	// 	/// </summary>
	// 	private Vector3 TargetCameraDirection = Vector3.Back;
	// 	/// <summary>
	// 	/// The timestamp of the last time the player moved the camera around the pivot. This is used to determine when the
	// 	/// camera should start automatically rotating around the pivot again.
	// 	/// </summary>
	// 	private ulong LastOrbitalMovementPlayerInputTimestamp = 0;
	// 	private OrbitComponent Controller;
	// 	private Vector2 MouseInput;

	// 	private ulong TimeSinceLastOrbitalMovementPlayerInputMs
	// 		=> Time.GetTicksMsec() - this.LastOrbitalMovementPlayerInputTimestamp;
	// 	private float AutomaticHorizontalMovementRadPSec => Mathf.DegToRad(this.Controller.DegreesPerSecond);
	// 	private float InitialAngleRad => Mathf.DegToRad(this.Controller.AngleDeg);
	// 	private float MinAngleRad => Mathf.DegToRad(this.Controller.MinAngleDeg);
	// 	private float MaxAngleRad => Mathf.DegToRad(this.Controller.MaxAngleDeg);
	// 	public CameraDirectionControllerClass(OrbitComponent controller) => Controller = controller;
	// 	public Vector3 InitializeCameraDirection(Vector3 pivotPosition)
	// 	{
	// 		if (this.Controller.Camera.GlobalPosition.DistanceSquaredTo(pivotPosition) < Mathf.Epsilon) {
	// 			return this.TargetCameraDirection = this.AnchorCameraDirection = Vector3.Back;
	// 		}
	// 		Vector3 direction = ((this.Controller.Camera.GlobalPosition - pivotPosition) with { Y = 0}).Normalized();
	// 		return this.TargetCameraDirection
	// 			= this.AnchorCameraDirection
	// 			= direction.Rotated(Basis.LookingAt(direction).X, this.InitialAngleRad);
	// 	}
	// 	/// <summary>
	// 	/// This method calculates the direction the camera should be positioned in relation to the pivot in the next
	// 	/// frame. It reads the player's input, applies smoothing, and applies automatic orbiting movement. (whichever
	// 	/// of these features are enabled)
	// 	/// </summary>
	// 	public Vector3 NextCameraDirection()
	// 	{
	// 		Vector2 mouseInput = this.ConsumeMouseInput();

	// 		// Rotate the camera based on the player's input
	// 		if (this.Controller.EnableMouseMotionControls && mouseInput.LengthSquared() > Mathf.Epsilon) {
	// 			float xAngleInputRad = Mathf.DegToRad(mouseInput.X / this.Controller.PixelsPerDegree) * this.Controller.MouseSensibilityX;
	// 			float yAngleInputRad = Mathf.DegToRad(mouseInput.Y / this.Controller.PixelsPerDegree) * this.Controller.MouseSensibilityY;
	// 			Vector3 xRotatedCamDir = !GDirectorUtil.CheckNormalsAreParallel(this.TargetCameraDirection, Vector3.Up)
	// 				? this.TargetCameraDirection.Rotated(Vector3.Down, xAngleInputRad)
	// 				: Vector3.Back; // Reset the camera direction if it's looking straight up or down
	// 			Vector3 crossAxis = Basis.LookingAt(xRotatedCamDir).X;
	// 			float currentVerticalAngle = xRotatedCamDir.AngleTo(Vector3.Down) - Mathf.Pi / 2;
	// 			float newVerticalAngle = Mathf.Clamp(currentVerticalAngle + yAngleInputRad, this.MinAngleRad, this.MaxAngleRad);
	// 			this.TargetCameraDirection = xRotatedCamDir.Rotated(crossAxis, newVerticalAngle - currentVerticalAngle);
	// 			this.LastOrbitalMovementPlayerInputTimestamp = Time.GetTicksMsec();

	// 		// Rotate the camera based on automatic rotation settings
	// 		} else if (
	// 			this.AutomaticHorizontalMovementRadPSec != 0
	// 			&& (
	// 				!this.Controller.EnableMouseMotionControls
	// 				|| this.TimeSinceLastOrbitalMovementPlayerInputMs > this.Controller.DelayAfterPlayerControlSec * 1000
	// 			)
	// 		) {
	// 			this.TargetCameraDirection = this.TargetCameraDirection.Rotated(
	// 				Vector3.Up,
	// 				this.AutomaticHorizontalMovementRadPSec * (float) this.Controller.GetProcessDeltaTime()
	// 			);
	// 		}

	// 		// Calculate the new camera direction by approximating the current direction to the expected direction
	// 		// (determined by this.TargetCameraDirection).
	// 		float angle = this.AnchorCameraDirection.AngleTo(this.TargetCameraDirection);
	// 		this.AnchorCameraDirection = angle < Mathf.Epsilon
	// 			? this.TargetCameraDirection
	// 			: this.AnchorCameraDirection.Slerp(this.TargetCameraDirection, this.Controller.OrbitalMovementLerpWeight);

	// 		return this.AnchorCameraDirection;
	// 	}
	// 	private Vector2 ConsumeMouseInput()
	// 	{
	// 		Vector2 mouseInput = this.MouseInput;
	// 		this.MouseInput = Vector2.Zero;
	// 		return mouseInput;
	// 	}
	// 	public void HandleInput(InputEvent @event)
	// 	{
	// 		if (@event is InputEventMouseMotion mouseMotion) {
	// 			this.MouseInput = mouseMotion.Relative;
	// 		}
	// 	}
	// }

	// //==================================================================================================================
	// // CALCULATE CAMERA DISTANCE
	// //==================================================================================================================

	// /// <summary>
	// /// This class is responsible for calculating the distance the camera should be from the pivot. It reads the
	// /// player's input if player-controlled zoom is enabled, and applies the zoom with soomthing (if enabled).
	// /// </summary>
	// private class CameraDistanceControllerClass
	// {
	// 	/// <summary>
	// 	/// The distance the camera was from the pivot the last frame. If the pivot moves, the actual distance of the camera
	// 	/// from the pivot will change, so we use this value to preserve the camera's distance from the pivot from one frame
	// 	/// to the next.
	// 	/// </summary>
	// 	private float AnchorCameraDistance {
	// 		get => this.Controller.RestDistance;
	// 		set => this.Controller.RestDistance = value;
	// 	}
	// 	/// <summary>
	// 	/// The distance the camera should be from the pivot. Every time the player changes the zoom level, the target
	// 	/// camera distance is redefined, and every time <see cref="NextCameraDistance"/> is called, the camera distance
	// 	/// is smoothed toward this target distance.
	// 	/// </summary>
	// 	private float TargetCameraDistance;
	// 	private OrbitComponent Controller;
	// 	public CameraDistanceControllerClass(OrbitComponent controller) => Controller = controller;
	// 	public float InitializeCameraDistance()
	// 		=> this.TargetCameraDistance = this.AnchorCameraDistance = this.Controller.RestDistance;
	// 	/// <summary>
	// 	/// This method calculates the distance the camera should be from the pivot in the next frame. It reads the
	// 	/// player's input and applies smoothing. (whichever of these features is enabled) Every time this method is
	// 	/// called, the distance is smoothed toward the target distance, according to the player input.
	// 	/// If player-controlled zoom is disabled, this method will return the same distance every time.
	// 	/// </summary>
	// 	public float NextCameraDistance()
	// 	{
	// 		// Zoom in and out based on the player's input
	// 		if (this.Controller.ZoomEnabled) {
	// 			// Read input & update target zoom
	// 			if (!string.IsNullOrEmpty(this.Controller.ZoomOutAction) && Input.IsActionJustPressed(this.Controller.ZoomOutAction)) {
	// 				this.TargetCameraDistance = Mathf.Clamp(
	// 					this.TargetCameraDistance + this.Controller.ZoomAmountUnPerTick,
	// 					this.Controller.MinDistance,
	// 					this.Controller.MaxDistance
	// 				);
	// 			} else if (!string.IsNullOrEmpty(this.Controller.ZoomInAction) && Input.IsActionJustPressed(this.Controller.ZoomInAction)) {
	// 				this.TargetCameraDistance = Mathf.Clamp(
	// 					this.TargetCameraDistance - this.Controller.ZoomAmountUnPerTick,
	// 					this.Controller.MinDistance,
	// 					this.Controller.MaxDistance
	// 				);
	// 			}

	// 			// Lerp toward the target zoom
	// 			this.AnchorCameraDistance = Mathf.Lerp(
	// 				this.AnchorCameraDistance,
	// 				this.TargetCameraDistance,
	// 				this.Controller.ZoomLerpWeight
	// 			);
	// 		}

	// 		return this.AnchorCameraDistance;
	// 	}
	// }
}
