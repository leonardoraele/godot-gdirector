using Godot;

namespace Raele.GDirector.VirtualCameraControllers;

public partial class OrbitalPositionController : VirtualCameraController
{

	// -------------------------------------------------------------------------
	// EXPORTED FIELDS
	// -------------------------------------------------------------------------

	/// <summary>
	/// The pivot around which the camera will orbit. If this is not set, the camera will orbit around the world origin.
	/// </summary>
	[Export] public Node3D? Pivot;
	/// <summary>
	/// Offsets the camera's position relative to the pivot's local space.
	///
	/// If the pivot is null, this is ignored.
	/// </summary>
	[Export] public Vector3 LocalOffset;
	/// <summary>
	/// Offsets the camera's position relative to the world space.
	/// </summary>
	[Export] public Vector3 GlobalOffset;
	/// <summary>
	/// The camera's initial distance from the pivot when the scene starts.
	///
	/// This can be modified at runtime by the player if player-controlled camera zoom is enabled.
	/// </summary>
	[Export] public float Distance = 4f;
	/// <summary>
	/// The camera's initial angle, in degrees, from the ZX plane at the pivot's position when the scene starts.
	/// Positive values rotate the camera upwards, negative values rotate it downwards.
	/// </summary>
	[Export(PropertyHint.Range, "-90,90,1,or_greater,or_less,degrees")] public float AngleDeg = 0f;

	[ExportGroup("Automatic Orbiting")]
	/// <summary>
	/// The speed at which the camera will automatically rotate around the pivot, in degrees per second.
	///
	/// The automatic orbiting is disabled while the player is controlling the camera.
	/// </summary>
	[Export] public float DegreesPerSecond = 0f;
	/// <summary>
	/// The delay, in seconds, after the player stops controlling the camera before the camera starts automatically
	/// rotating around the pivot again.
	/// </summary>
	[Export] public float DelayAfterPlayerControlSec = 2f; // TODO

	[ExportGroup("Player Controls")]
	// TODO
	// [ExportSubgroup("Spherical Movement (Input Actions)")]
	// [Export] public string? LookLeftAction;
	// [Export] public string? LookRightAction;
	// [Export] public string? LookDownAction;
	// [Export] public string? LookUpAction;
	// [Export] public float VerticalRotationSpeedDegPSec = 1;
	// [Export] public float HorizontalRotationSpeedDegPSec = 1;
	[ExportSubgroup("Spherical Movement (Mouse Motion)")]
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
	[ExportSubgroup("Spherical Movement Smoothing")]
	/// <summary>
	/// The weight of the camera's spherical movement smoothing. A value of 1 means no smoothing, and values closer to 0
	/// mean more smoothing.
	/// </summary>
	[Export(PropertyHint.Range, "0.01,1,0.01")] public float LerpWeight = 1f;
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

	// -------------------------------------------------------------------------
	// PRIVATE FIELDS
	// -------------------------------------------------------------------------

	private float TargetZoomDistance = 0f;
	private Vector3 TargetCameraDirection = Vector3.Back;
	private ulong LastSphericalMovementPlayerInputTimestamp = 0;
	private ulong TimeSinceLastSphericalMovementPlayerInputMs
		=> Time.GetTicksMsec() - this.LastSphericalMovementPlayerInputTimestamp;

    // -------------------------------------------------------------------------
    // PROPERTIES
    // -------------------------------------------------------------------------

    private float MinAngleRad => Mathf.DegToRad(this.MinAngleDeg);
	private float MaxAngleRad => Mathf.DegToRad(this.MaxAngleDeg);
	private Vector3 PivotOffsetedPosition
		=> this.Pivot == null
			? Vector3.Zero
			: this.Pivot.GlobalPosition
				+ this.GlobalOffset
				+ this.LocalOffset * this.Pivot.Basis;
	private float InitialAngleRad => Mathf.DegToRad(this.AngleDeg);
	private float AutomaticHorizontalMovementRadPSec => Mathf.DegToRad(this.DegreesPerSecond);

    // -------------------------------------------------------------------------
    // METHODS
    // -------------------------------------------------------------------------

    public override void _Ready()
    {
        base._Ready();
		this.TargetZoomDistance = this.Distance;
		Vector3 camRelPos = this.Camera.GlobalPosition - this.PivotOffsetedPosition;
		this.TargetCameraDirection = camRelPos.LengthSquared() > Mathf.Epsilon
			&& !GodotUtil.CheckNormalsAreParallel(camRelPos.Normalized(), Vector3.Up)
				? (camRelPos with { Y = 0 })
					.Normalized()
					.Rotated(Basis.LookingAt(camRelPos).X, this.InitialAngleRad)
			: Vector3.Back.Rotated(Vector3.Left, this.InitialAngleRad);
    }

    public override void _Process(double delta)
	{
		base._Process(delta);

		Vector3 camRelPos = this.Camera.GlobalPosition - this.PivotOffsetedPosition;
		float camDistance = camRelPos.Length();

		// Rotate camera around the pivot based on the player's mouse input
		if (camDistance > Mathf.Epsilon) {
			Vector2 mouseVelocity = Input.GetLastMouseVelocity();
			if (this.EnableMouseMotionControls && mouseVelocity.LengthSquared() > Mathf.Epsilon) {
				float xAngleInput = Mathf.DegToRad(mouseVelocity.X * (float) delta / this.PixelsPerDegree) * this.MouseSensibilityX;
				float yAngleInput = Mathf.DegToRad(mouseVelocity.Y * (float) delta / this.PixelsPerDegree) * this.MouseSensibilityY;
				Vector3 xRotatedCamDir = !GodotUtil.CheckNormalsAreParallel(this.TargetCameraDirection, Vector3.Up)
					? this.TargetCameraDirection.Rotated(Vector3.Down, xAngleInput)
					: Vector3.Back; // Reset the camera direction if it's looking straight up or down
				Vector3 crossAxis = Basis.LookingAt(xRotatedCamDir).X;
				float currentVerticalAngle = xRotatedCamDir.AngleTo(Vector3.Down) - Mathf.Pi / 2;
				float newVerticalAngle = Mathf.Clamp(currentVerticalAngle + yAngleInput, this.MinAngleRad, this.MaxAngleRad);
				this.TargetCameraDirection = xRotatedCamDir.Rotated(crossAxis, newVerticalAngle - currentVerticalAngle);
				this.LastSphericalMovementPlayerInputTimestamp = Time.GetTicksMsec();
			// Apply automatic rotation
			} else if (
				!this.EnableMouseMotionControls
				|| this.TimeSinceLastSphericalMovementPlayerInputMs > this.DelayAfterPlayerControlSec * 1000
			) {
				this.TargetCameraDirection = this.TargetCameraDirection.Rotated(
					Vector3.Up,
					this.AutomaticHorizontalMovementRadPSec * (float) delta
				);
			}
		}

		Vector3 currentCamDirection = camDistance > Mathf.Epsilon
			? camRelPos.Normalized()
			: Vector3.Back;
		Vector3 newCamDirection = currentCamDirection.Slerp(this.TargetCameraDirection, this.LerpWeight).Normalized();

		// Zoom in and out based on the player's input
		if (this.EnableZoomControls) {
			if (!string.IsNullOrEmpty(this.ZoomOutAction) && Input.IsActionJustPressed(this.ZoomOutAction)) {
				this.TargetZoomDistance = Mathf.Clamp(
					this.TargetZoomDistance + this.ZoomAmountUnPerTick,
					this.MinDistance,
					this.MaxDistance
				);
			} else if (!string.IsNullOrEmpty(this.ZoomInAction) && Input.IsActionJustPressed(this.ZoomInAction)) {
				this.TargetZoomDistance = Mathf.Clamp(
					this.TargetZoomDistance - this.ZoomAmountUnPerTick,
					this.MinDistance,
					this.MaxDistance
				);
			}
		}

		float newCamDistance = Mathf.Lerp(camDistance, this.TargetZoomDistance, this.ZoomLerpWeight);

		this.Camera.GlobalPosition = this.PivotOffsetedPosition + newCamDirection * newCamDistance;
	}
}
