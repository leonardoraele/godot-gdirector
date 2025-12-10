using Godot;

namespace Raele.GDirector.VirtualCameraComponents;

// TODO Instead of forcing the camera to orbit around the Y axis, allow the player to select the axis around
// which the camera should orbit.
// TODO Add the ability to make Automatic Orbiting move toward a specific direction (e.g. the player's character's back)
// instead of always orbiting around the pivot's Y axis.
// TODO Add a bool parameter to allow the user to select the camera behavior when the camera disaligns from the pivot.
// Right now, the behavior is to preserve the angle of the camera from the pivot, while changing the camera's position
// as needed. The alternative would be to make the move as little as possible while remaining in the orbit, and changing
// the camera angle as much as needed.
public partial class OrbitalMovement3D : VirtualCameraComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTED FIELDS
	// -----------------------------------------------------------------------------------------------------------------

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
	[Export] public float Distance = 4f; // Used by cameraDistanceCalculation
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
	[Export(PropertyHint.Range, "-90,90,1,or_greater,or_less,degrees")] public float AngleDeg = 0f; // Used by cameraDirectionCalculation

	[ExportGroup("Pivot Movement Smoothing")]
	/// <summary>
	/// The weight of the pivot position smoothing. If smoothing is applied, there will be a lag before the camera moves
	/// to the pivot's new position, as the pivot moves. This property determines the weight of the smoothing. A value
	/// of 1 means no smoothing, and values closer to 0 mean more smoothing.
	/// </summary>
	[Export(PropertyHint.Range, "0.01,1,0.01")] public float PivotMovementLerpLerp = 1; // Used by pivotPositionCalculation
	/// <summary>
	/// This is the maximum distance the camera's "virtual" orbit pivot can lag behind the actual pivot's position, in
	/// global position units. Use this to prevent the camera from being too off if the pivot moves too fast and the
	/// smoothing weight is too low.
	///
	/// Note: This is NOT the distance from the camera to the pivot. This is the distance from the point the camera is
	/// orbiting around to the pivot's position, when they differ because of the smoothing applied.
	/// </summary>
	[Export] public float MaxPivotDistanceUn = float.PositiveInfinity; // Used by pivotPositionCalculation

	[ExportGroup("Automatic Orbiting")]
	/// <summary>
	/// The speed at which the camera will automatically rotate around the pivot, in degrees per second.
	///
	/// If player-controlled spherical movement is enabled, the automatic orbiting is disabled while the player is
	/// controlling the camera.
	/// </summary>
	[Export] public float DegreesPerSecond = 0f; // Used by cameraDirectionCalculation
	/// <summary>
	/// The delay, in seconds, after the player stops controlling the camera before the camera starts automatically
	/// rotating around the pivot again.
	/// </summary>
	[Export] public float DelayAfterPlayerControlSec = 2f; // Used by cameraDirectionCalculation

	[ExportGroup("Player Controls")]
	// TODO
	// [ExportSubgroup("Orbital Movement (Input Actions)")]
	// [Export] public string? LookLeftAction;
	// [Export] public string? LookRightAction;
	// [Export] public string? LookDownAction;
	// [Export] public string? LookUpAction;
	// [Export] public float VerticalRotationSpeedDegPSec = 1;
	// [Export] public float HorizontalRotationSpeedDegPSec = 1;
	[ExportSubgroup("Orbital Movement (Mouse Motion)")]
	/// <summary>
	/// If enabled, the player can control the camera's spherical movement around the pivot using the mouse.
	/// </summary>
	[Export] public bool EnableMouseMotionControls = false;
	/// <summary>
	/// The number of pixels the mouse has to move to rotate the camera by one degree. (assuming mouse sensitivity is 1)
	/// </summary>
	[Export] public float PixelsPerDegree = 1;
	/// <summary>
	/// The mouse's sensibility when controlling the camera's horizontal movement. This is a multiplier applied to the
	/// mouse's horizontal movement.
	/// </summary>
	[Export] public float MouseSensibilityX = 1;
	/// <summary>
	/// The mouse's sensibility when controlling the camera's vertical movement. This is a multiplier applied to the
	/// mouse's vertical movement.
	/// </summary>
	[Export] public float MouseSensibilityY = 1;
	/// <summary>
	/// The highest vertical angle the camera can move, in degrees the ZX plane at the pivot's position.
	///
	/// Positive angles are rotations toward the positive Y axis, and vice-versa.
	/// </summary>
	[Export(PropertyHint.Range, "-60,75,1,or_greater,or_less,degrees")] public float MaxAngleDeg = 60f;
	/// <summary>
	/// The lowest vertical angle the camera can move, in degrees the ZX plane at the pivot's position.
	///
	/// Negative angles are rotations toward the negative Y axis, and vice-versa.
	/// </summary>
	[Export(PropertyHint.Range, "-60,75,1,or_greater,or_less,degrees")] public float MinAngleDeg = -15f;
	// [Export] public Curve? DistanceMultiplierCurve; // TODO Use to change the camera distance based on the angle
	// [Export] public Curve FieldOfViewPerAngle; // TODO Use to change the camera FOV based on the angle
	[ExportSubgroup("Orbital Movement Smoothing")]
	/// <summary>
	/// The weight of the camera's spherical movement smoothing. A value of 1 means no smoothing, and values closer to 0
	/// mean more smoothing.
	/// </summary>
	[Export(PropertyHint.Range, "0.01,1,0.01")] public float OrbitalMovementLerpWeight = 1f;
	[ExportSubgroup("Zoom")]
	/// <summary>
	/// If enabled, the player can control the camera's zoom.
	/// </summary>
	[Export] public bool EnableZoomControls = false;
	/// <summary>
	/// The input action the player can use to zoom out the camera.
	/// </summary>
	[Export] public string? ZoomOutAction;
	/// <summary>
	/// The input action the player can use to zoom in the camera.
	/// </summary>
	[Export] public string? ZoomInAction;
	/// <summary>
	/// The amount of distance units the camera will zoom in or out per tick. (i.e. each time the zoom in or zoom out
	/// input action is activated)
	/// </summary>
	[Export] public float ZoomAmountUnPerTick = 0.5f;
	/// <summary>
	/// The minimum distance the player can move the camera towards the pivot.
	/// </summary>
	[Export] public float MinDistance = 3f;
	/// <summary>
	/// The maximum distance the player can move the camera away from the pivot.
	/// </summary>
	[Export] public float MaxDistance = 8f;
	/// <summary>
	/// The weight of the camera's zoom smoothing. A value of 1 means no smoothing, and values closer to 0 mean more
	/// smoothing.
	/// </summary>
	[Export(PropertyHint.Range, "0.01,1,0.01")] public float ZoomLerpWeight = 0.15f;

	// -----------------------------------------------------------------------------------------------------------------
	// PRIVATE FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private PivotPositionController pivotPositionCalculation;
	private CameraDistanceController cameraDistanceCalculation;
	private CameraDirectionController cameraDirectionCalculation;

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public OrbitalMovement3D()
	{
		this.pivotPositionCalculation = new(this);
		this.cameraDistanceCalculation = new(this);
		this.cameraDirectionCalculation = new(this);
	}

	public override void _Ready()
	{
		base._Ready();

		// Calculate the initial camera position
		Vector3 pivotPosition = this.pivotPositionCalculation.InitializePivotPosition();
		Vector3 cameraDirection = this.cameraDirectionCalculation.InitializeCameraDirection(pivotPosition);
		float cameraDistance = this.cameraDistanceCalculation.InitializeCameraDistance();

		// Apply the initial camera position
		this.Camera.GlobalPosition = pivotPosition + cameraDirection * cameraDistance;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		// Calculate the new camera position
		Vector3 pivotPosition = this.pivotPositionCalculation.NextPivotPosition();
		Vector3 cameraDirection = this.cameraDirectionCalculation.NextCameraDirection();
		float cameraDistance = this.cameraDistanceCalculation.NextCameraDistance();

		// Apply the new camera position
		this.Camera.GlobalPosition = pivotPosition + cameraDirection * cameraDistance;
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		this.cameraDirectionCalculation.HandleInput(@event);
	}

	// -----------------------------------------------------------------------------------------------------------------
	// CALCULATE PIVOT POSITION
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// This class is responsible for calculating the position the camera should be orbiting around. It reads the pivot
	/// position, applies the offset, and applies smoothing.
	/// </summary>
	private class PivotPositionController
	{
		/// <summary>
		/// This is the real actual position the camera orbits around, in global space. This position is lerp'ed toward
		/// the pivot position every frame. If <see cref="PivotPositionLerpWeight"> is 1, this value will be the same as
		/// <see cref="PivotPosition">.
		/// </summary>
		private Vector3 AnchorPivotPosition = Vector3.Zero;
		private OrbitalMovement3D Controller;
		public PivotPositionController(OrbitalMovement3D controller) => Controller = controller;
		public Vector3 InitializePivotPosition() => this.AnchorPivotPosition = this.GetTargetPivotPosition();
		/// <summary>
		/// Calculates the position the camera should be orbiting around in the next frame. Every time this method is
		/// called, the position is smoothed toward the pivot's position. (if smoothing is enabled)
		/// </summary>
		public Vector3 NextPivotPosition()
		{
			Vector3 pivotPosition = this.GetTargetPivotPosition();
			if (this.Controller.PivotMovementLerpLerp == 1) {
				return pivotPosition;
			}
			this.AnchorPivotPosition = this.AnchorPivotPosition.Lerp(pivotPosition, this.Controller.PivotMovementLerpLerp);
			if (this.AnchorPivotPosition.DistanceTo(pivotPosition) > this.Controller.MaxPivotDistanceUn) {
				this.AnchorPivotPosition = pivotPosition.MoveToward(
					this.AnchorPivotPosition,
					this.Controller.MaxPivotDistanceUn
				);
			}
			return this.AnchorPivotPosition;
		}
		/// <summary>
		/// Calculates the ideal position the camera should be orbiting around, which is the pivot's position with the
		/// pivot offset applied. The returned coordinates are in global space.
		/// </summary>
		private Vector3 GetTargetPivotPosition()
		{
			Transform3D? transform = this.Controller.Pivot?.GlobalTransform;
			return transform == null
				? this.Controller.PivotOffset
				: transform.Value.Origin + transform.Value.Basis * this.Controller.PivotOffset;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// CALCULATE CAMERA DIRECTION
	// -----------------------------------------------------------------------------------------------------------------

	private class CameraDirectionController
	{
		/// <summary>
		/// This is the position the camera was positioned in relation to the pivot the last frame, in global space. If the
		/// pivot moves, the actual direction of the camera in relation to pivot will change, so we use this value to
		/// preserve the camera's direction in relation to the pivot from one frame to the next.
		/// </summary>
		private Vector3 AnchorCameraDirection = Vector3.Back;
		/// <summary>
		/// This is the direction the camera should be positioned in relation to the pivot, in global space.
		/// </summary>
		private Vector3 TargetCameraDirection = Vector3.Back;
		/// <summary>
		/// The timestamp of the last time the player moved the camera around the pivot. This is used to determine when the
		/// camera should start automatically rotating around the pivot again.
		/// </summary>
		private ulong LastOrbitalMovementPlayerInputTimestamp = 0;
		private OrbitalMovement3D Controller;
		private Vector2 MouseInput;

		private ulong TimeSinceLastOrbitalMovementPlayerInputMs
			=> Time.GetTicksMsec() - this.LastOrbitalMovementPlayerInputTimestamp;
		private float AutomaticHorizontalMovementRadPSec => Mathf.DegToRad(this.Controller.DegreesPerSecond);
		private float InitialAngleRad => Mathf.DegToRad(this.Controller.AngleDeg);
		private float MinAngleRad => Mathf.DegToRad(this.Controller.MinAngleDeg);
		private float MaxAngleRad => Mathf.DegToRad(this.Controller.MaxAngleDeg);
		public CameraDirectionController(OrbitalMovement3D controller) => Controller = controller;
		public Vector3 InitializeCameraDirection(Vector3 pivotPosition)
		{
			if (this.Controller.Camera.GlobalPosition.DistanceSquaredTo(pivotPosition) < Mathf.Epsilon) {
				return this.TargetCameraDirection = this.AnchorCameraDirection = Vector3.Back;
			}
			Vector3 direction = ((this.Controller.Camera.GlobalPosition - pivotPosition) with { Y = 0}).Normalized();
			return this.TargetCameraDirection
				= this.AnchorCameraDirection
				= direction.Rotated(Basis.LookingAt(direction).X, this.InitialAngleRad);
		}
		/// <summary>
		/// This method calculates the direction the camera should be positioned in relation to the pivot in the next
		/// frame. It reads the player's input, applies smoothing, and applies automatic orbiting movement. (whichever
		/// of these features are enabled)
		/// </summary>
		public Vector3 NextCameraDirection()
		{
			Vector2 mouseInput = this.ConsumeMouseInput();

			// Rotate the camera based on the player's input
			if (this.Controller.EnableMouseMotionControls && mouseInput.LengthSquared() > Mathf.Epsilon) {
				float xAngleInputRad = Mathf.DegToRad(mouseInput.X / this.Controller.PixelsPerDegree) * this.Controller.MouseSensibilityX;
				float yAngleInputRad = Mathf.DegToRad(mouseInput.Y / this.Controller.PixelsPerDegree) * this.Controller.MouseSensibilityY;
				Vector3 xRotatedCamDir = !GDirectorUtil.CheckNormalsAreParallel(this.TargetCameraDirection, Vector3.Up)
					? this.TargetCameraDirection.Rotated(Vector3.Down, xAngleInputRad)
					: Vector3.Back; // Reset the camera direction if it's looking straight up or down
				Vector3 crossAxis = Basis.LookingAt(xRotatedCamDir).X;
				float currentVerticalAngle = xRotatedCamDir.AngleTo(Vector3.Down) - Mathf.Pi / 2;
				float newVerticalAngle = Mathf.Clamp(currentVerticalAngle + yAngleInputRad, this.MinAngleRad, this.MaxAngleRad);
				this.TargetCameraDirection = xRotatedCamDir.Rotated(crossAxis, newVerticalAngle - currentVerticalAngle);
				this.LastOrbitalMovementPlayerInputTimestamp = Time.GetTicksMsec();

			// Rotate the camera based on automatic rotation settings
			} else if (
				this.AutomaticHorizontalMovementRadPSec != 0
				&& (
					!this.Controller.EnableMouseMotionControls
					|| this.TimeSinceLastOrbitalMovementPlayerInputMs > this.Controller.DelayAfterPlayerControlSec * 1000
				)
			) {
				this.TargetCameraDirection = this.TargetCameraDirection.Rotated(
					Vector3.Up,
					this.AutomaticHorizontalMovementRadPSec * (float) this.Controller.GetProcessDeltaTime()
				);
			}

			// Calculate the new camera direction by approximating the current direction to the expected direction
			// (determined by this.TargetCameraDirection).
			float angle = this.AnchorCameraDirection.AngleTo(this.TargetCameraDirection);
			this.AnchorCameraDirection = angle < Mathf.Epsilon
				? this.TargetCameraDirection
				: this.AnchorCameraDirection.Slerp(this.TargetCameraDirection, this.Controller.OrbitalMovementLerpWeight);

			return this.AnchorCameraDirection;
		}
		private Vector2 ConsumeMouseInput()
		{
			Vector2 mouseInput = this.MouseInput;
			this.MouseInput = Vector2.Zero;
			return mouseInput;
		}
		public void HandleInput(InputEvent @event)
		{
			if (@event is InputEventMouseMotion mouseMotion) {
				this.MouseInput = mouseMotion.Relative;
			}
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// CALCULATE CAMERA DISTANCE
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// This class is responsible for calculating the distance the camera should be from the pivot. It reads the
	/// player's input if player-controlled zoom is enabled, and applies the zoom with soomthing (if enabled).
	/// </summary>
	private class CameraDistanceController
	{
		/// <summary>
		/// The distance the camera was from the pivot the last frame. If the pivot moves, the actual distance of the camera
		/// from the pivot will change, so we use this value to preserve the camera's distance from the pivot from one frame
		/// to the next.
		/// </summary>
		private float AnchorCameraDistance {
			get => this.Controller.Distance;
			set => this.Controller.Distance = value;
		}
		/// <summary>
		/// The distance the camera should be from the pivot. Every time the player changes the zoom level, the target
		/// camera distance is redefined, and every time <see cref="NextCameraDistance"/> is called, the camera distance
		/// is smoothed toward this target distance.
		/// </summary>
		private float TargetCameraDistance;
		private OrbitalMovement3D Controller;
		public CameraDistanceController(OrbitalMovement3D controller) => Controller = controller;
		public float InitializeCameraDistance()
			=> this.TargetCameraDistance = this.AnchorCameraDistance = this.Controller.Distance;
		/// <summary>
		/// This method calculates the distance the camera should be from the pivot in the next frame. It reads the
		/// player's input and applies smoothing. (whichever of these features is enabled) Every time this method is
		/// called, the distance is smoothed toward the target distance, according to the player input.
		/// If player-controlled zoom is disabled, this method will return the same distance every time.
		/// </summary>
		public float NextCameraDistance()
		{
			// Zoom in and out based on the player's input
			if (this.Controller.EnableZoomControls) {
				// Read input & update target zoom
				if (!string.IsNullOrEmpty(this.Controller.ZoomOutAction) && Input.IsActionJustPressed(this.Controller.ZoomOutAction)) {
					this.TargetCameraDistance = Mathf.Clamp(
						this.TargetCameraDistance + this.Controller.ZoomAmountUnPerTick,
						this.Controller.MinDistance,
						this.Controller.MaxDistance
					);
				} else if (!string.IsNullOrEmpty(this.Controller.ZoomInAction) && Input.IsActionJustPressed(this.Controller.ZoomInAction)) {
					this.TargetCameraDistance = Mathf.Clamp(
						this.TargetCameraDistance - this.Controller.ZoomAmountUnPerTick,
						this.Controller.MinDistance,
						this.Controller.MaxDistance
					);
				}

				// Lerp toward the target zoom
				this.AnchorCameraDistance = Mathf.Lerp(
					this.AnchorCameraDistance,
					this.TargetCameraDistance,
					this.Controller.ZoomLerpWeight
				);
			}

			return this.AnchorCameraDistance;
		}
	}
}
