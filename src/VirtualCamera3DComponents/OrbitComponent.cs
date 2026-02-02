using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;
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
	[Export(PropertyHint.None, "suffix:m")] public Vector3 PivotOffset;
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
			this.CurrentDistance = field = value.AtLeast(Mathf.Epsilon * 10);
			// if (field < this.TranslationMinDistance)
			// 	this.TranslationMinDistance = field;
			// if (field > this.TranslationMaxDistance)
			// 	this.TranslationMaxDistance = field;
			if (field < this.ZoomMinDistance)
				this.ZoomMinDistance = field;
			if (field > this.ZoomMaxDistance)
				this.ZoomMaxDistance = field;
		}
	}
		= 5f;
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
	[Export(PropertyHint.Range, "-90,90,5,radians_as_degrees")] public float RestAngle
	{
		get;
		set
		{
			field = value.Clamped(-Mathf.Pi / 2 + Mathf.Epsilon * 10, Mathf.Pi / 2 - Mathf.Epsilon * 10);
			this.CurrentDirection = Vector3.Back.Rotated(Vector3.Left, field);
			if (field < this.PlayerInputLowestAngle)
				this.PlayerInputLowestAngle = field;
			if (field > this.PlayerInputHighestAngle)
				this.PlayerInputHighestAngle = field;
		}
	}
		= Mathf.Pi / 6f;

	// [ExportGroup("Rotation Smoothing", "Rotation")]
	// [Export(PropertyHint.Range, "0,100,1")] public float RotationSmoothingFactor = 0;

	// [ExportGroup("Translation Smoothing", "Translation")]
	// [Export(PropertyHint.Range, "0,100,1")] public float TranslationSmoothingFactor = 0;
	// /// <summary>
	// /// This is the maximum distance the camera's "virtual" orbit pivot can lag behind the actual pivot's position, in
	// /// global position units. Use this to prevent the camera from being too off if the pivot moves too fast and the
	// /// smoothing weight is too low.
	// ///
	// /// Note: This is NOT the distance from the camera to the pivot. This is the distance from the point the camera is
	// /// orbiting around to the pivot's position, when they differ because of the smoothing applied.
	// /// </summary>
	// [Export(PropertyHint.None, "suffix:m")] public float TranslationMaxDistance
	// {
	// 	get;
	// 	set
	// 	{
	// 		field = value.AtLeast(this.TranslationMinDistance);
	// 		this.MaxDistanceSqr = field * field;
	// 		if (field < this.RestDistance)
	// 			this.RestDistance = field;
	// 	}
	// }
	// 	= float.PositiveInfinity;
	// [Export(PropertyHint.None, "suffix:m")] public float TranslationMinDistance
	// {
	// 	get;
	// 	set
	// 	{
	// 		field = value.Clamped(Mathf.Epsilon * 10, this.TranslationMaxDistance);
	// 		this.MinDistanceSqr = field * field;
	// 		if (field > this.RestDistance)
	// 			this.RestDistance = field;
	// 	}
	// }
	// 	= 1f;

	[ExportGroup("Automatic Orbiting", "AutoOrbit")]
	[Export(PropertyHint.GroupEnable)] public bool AutoOrbitEnabled;
	/// <summary>
	/// The speed at which the camera will automatically rotate around the pivot, in degrees per second.
	///
	/// If player-controlled spherical movement is enabled, the automatic orbiting is disabled while the player is
	/// controlling the camera.
	/// </summary>
	[Export(PropertyHint.Range, "5,360,5,or_greater,radians_as_degrees,suffix:°/s")] public float AutoOrbitAngularVelocity = Mathf.Pi / 6;
	/// <summary>
	/// The delay, in seconds, after the player stops controlling the camera before the camera starts automatically
	/// rotating around the pivot again.
	/// </summary>
	[Export(PropertyHint.None, "suffix:s")] public float AutoOrbitStartDelay
		{ get; set { field = value.AtLeast(0f); } }
		= 2f;

	[ExportGroup("Player Controls", "PlayerInput")]
	/// <summary>
	/// The highest vertical angle the camera can move, in degrees the ZX plane at the pivot's position.
	///
	/// Positive angles are rotations toward the positive Y axis, and vice-versa.
	/// </summary>
	[Export(PropertyHint.Range, "-90,90,5,radians_as_degrees")] public float PlayerInputHighestAngle
	{
		get;
		set
		{
			field = value.Clamped(this.PlayerInputLowestAngle, Mathf.Pi / 2 - Mathf.Epsilon * 10);
			if (field < this.RestAngle)
				this.RestAngle = field;
		}
	}
		= Mathf.Pi / 3f;
	/// <summary>
	/// The lowest vertical angle the camera can move, in degrees the ZX plane at the pivot's position.
	///
	/// Negative angles are rotations toward the negative Y axis, and vice-versa.
	/// </summary>
	[Export(PropertyHint.Range, "-90,90,5,radians_as_degrees")] public float PlayerInputLowestAngle
	{
		get;
		set
		{
			field = value.Clamped(-Mathf.Pi / 2 + Mathf.Epsilon * 10, this.PlayerInputHighestAngle);
			if (field > this.RestAngle)
				this.RestAngle = field;
		}
	}
		= 0f;

	[ExportSubgroup("Use Input Actions", "InputActions")]
	[Export(PropertyHint.GroupEnable)] public bool InputActionsEnabled
		{ get; set { field = value; this.NotifyPropertyListChanged(); } }
		= true;
	[Export(PropertyHint.InputName)] public string InputActionsRotateUp = "camera_look_up";
	[Export(PropertyHint.InputName)] public string InputActionsRotateRight = "camera_look_right";
	[Export(PropertyHint.InputName)] public string InputActionsRotateDown = "camera_look_down";
	[Export(PropertyHint.InputName)] public string InputActionsRotateLeft = "camera_look_left";
	[Export(PropertyHint.Link, "radians_as_degrees,suffix:°/s")] public Vector2 InputActionsAngularVelocity
		{ get; set { field = value.Clamp(Vector2.Zero, Vector2.Inf); } }
		= Mathf.Pi * Vector2.One;

	[ExportSubgroup("Use Mouse Controls", "Mouse")]
	/// <summary>
	/// If enabled, the player can control the camera's spherical movement around the pivot using the mouse.
	/// </summary>
	[Export(PropertyHint.GroupEnable)] public bool MouseEnabled
		{ get; set { field = value; this.NotifyPropertyListChanged(); } }
		= false;
	/// <summary>
	/// The number of pixels the mouse has to move to rotate the camera by one degree. (assuming mouse sensitivity is 1)
	/// </summary>
	[Export] public float MousePixelsPerDegree = 1f;
	/// <summary>
	/// The mouse's sensitivity when controlling the camera's movement. This is a multiplier applied to the mouse's
	/// horizontal movement.
	/// </summary>
	[Export(PropertyHint.None, "suffix:x")] public Vector2 MouseSensitivity = Vector2.One;
	// [Export] public Curve? DistanceMultiplierCurve; // TODO Use to change the camera distance based on the angle
	// [Export] public Curve FieldOfViewPerAngle; // TODO Use to change the camera FOV based on the angle

	[ExportSubgroup("Enable Distance Controls", "Zoom")]
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
	/// The amount of distance units the camera will zoom in or out per tick. (i.e. each time the zoom in or zoom out
	/// input action is activated)
	/// </summary>
	[Export(PropertyHint.None, "suffix:m/s")] public float ZoomVelocity = 10f;
	/// <summary>
	/// The minimum distance the player can move the camera towards the pivot.
	/// </summary>
	[Export(PropertyHint.None, "suffix:m")] public float ZoomMinDistance
	{
		get;
		set
		{
			field = value.Clamped(Mathf.Epsilon * 10, ZoomMaxDistance);
			if (field > this.RestDistance)
				this.RestDistance = field;
		}
	}
		= 1f;
	/// <summary>
	/// The maximum distance the player can move the camera away from the pivot.
	/// </summary>
	[Export(PropertyHint.None, "suffix:m")] public float ZoomMaxDistance
	{
		get;
		set
		{
			field = value.AtLeast(ZoomMinDistance);
			if (field < this.RestDistance)
				this.RestDistance = field;
		}
	}
		= float.PositiveInfinity;

	//==================================================================================================================
	// FIELDS
	//==================================================================================================================

	private Tween? AutoOrbitDelay;
	private Vector2 AccumulatedMouseMovement = Vector2.Zero;
	private Vector3 CurrentDirection = Vector3.Back;
	private float CurrentDistance = 0f;

	//==================================================================================================================
	// COMPUTED PROPERTIES
	//==================================================================================================================

	// public float TranslationSmoothingLerpWeight
	// 	=> 1f / (this.TranslationSmoothingFactor + 1f);
	// public float RotationSmoothingLerpWeight
	// 	=> 1f / (this.RotationSmoothingFactor + 1f);
	public float HighAngleLimit => this.InputActionsEnabled || this.MouseEnabled
		? this.PlayerInputHighestAngle
		: this.RestAngle;
	public float LowAngleLimit => this.InputActionsEnabled || this.MouseEnabled
		? this.PlayerInputLowestAngle
		: this.RestAngle;
	public float MinDistanceLimit => this.ZoomEnabled
		? this.ZoomMinDistance
		: this.RestDistance;
	public float MaxDistanceLimit => this.ZoomEnabled
		? this.ZoomMaxDistance
		: this.RestDistance;

	//==================================================================================================================
	// OVERRIDES
	//==================================================================================================================

	public override void _ValidateProperty(Dictionary property)
	{
		base._ValidateProperty(property);
		switch (property["name"].AsString())
		{
			case nameof(this.PlayerInputHighestAngle):
			case nameof(this.PlayerInputLowestAngle):
				property["usage"] = this.InputActionsEnabled || this.MouseEnabled
					? (long) PropertyUsageFlags.Default
					: (long) PropertyUsageFlags.None;
				break;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		this.CurrentDistance = this.RestDistance;
		this.CurrentDirection = Vector3.Back.Rotated(Vector3.Right, this.RestAngle);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		// Calculate the pivot's position with offset
		Transform3D pivot = this.Pivot?.GlobalTransform.Translated(this.PivotOffset) ?? Transform3D.Identity;

		Vector3 cameraDirection = this.CurrentDirection;
		float cameraDistance = this.CurrentDistance;
		Vector3 crossAxis = cameraDirection.Cross(Vector3.Up).Normalized();

		// Apply automatic orbiting
		if (this.AutoOrbitEnabled && this.AutoOrbitDelay?.IsRunning() != true)
			cameraDirection = cameraDirection.Rotated(Vector3.Up, this.AutoOrbitAngularVelocity * (float) delta);

		// Apply player control
		if (!Engine.IsEditorHint() && this.Camera.AsVirtualCamera().IsLive)
		{
			// Apply player-input rotation
			{
				Vector2 playerInput =
					Input.GetVector(
						this.InputActionsRotateLeft,
						this.InputActionsRotateRight,
						this.InputActionsRotateDown,
						this.InputActionsRotateUp
					)
					* this.InputActionsAngularVelocity
					* (float) delta
					* this.InputActionsEnabled.ToInt()
					+ this.AccumulatedMouseMovement
						* new Vector2(1, -1) // Invert Y because mouse movement +Y is down
						* this.MouseSensitivity / Mathf.RadToDeg(this.MousePixelsPerDegree);

				// Flush accumulated mouse movement
				this.AccumulatedMouseMovement = Vector2.Zero;

				// Rotate the camera based on the player's input
				if (!playerInput.IsZeroApprox())
				{
					// Rotate horizontally
					{
						cameraDirection = cameraDirection.Rotated(Vector3.Down, playerInput.X);
					}

					// Rotate vertically
					{
						float currentVerticalAngle = cameraDirection.AngleTo(Vector3.Down) - Mathf.Pi / 2;
						float newVerticalAngle = Mathf.Clamp(
							currentVerticalAngle - playerInput.Y,
							this.PlayerInputLowestAngle,
							this.PlayerInputHighestAngle
						);
						cameraDirection = cameraDirection.Rotated(crossAxis, newVerticalAngle - currentVerticalAngle);
					}
				}
			}

			// Apply player-input zoom
			if (this.ZoomEnabled)
			{
				float playerInput = Input.GetAxis(this.ZoomInputMoveCloser, this.ZoomInputMoveFarther);
				cameraDistance = cameraDistance + playerInput * this.ZoomVelocity * (float) delta;
			}
		}

		// Bring the camera within distance limits
		cameraDistance = cameraDistance.Clamped(this.MinDistanceLimit, this.MaxDistanceLimit);

		// Bring the camera within angle limits
		{
			float currentAngle = cameraDirection.AngleTo(Vector3.Down) - Mathf.Pi / 2;
			float clampedAngle = currentAngle.Clamped(this.LowAngleLimit, this.HighAngleLimit);
			cameraDirection = cameraDirection.Rotated(crossAxis, clampedAngle - currentAngle);
		}

		this.Camera.GlobalPosition = pivot.Origin + cameraDirection * cameraDistance;
		this.CurrentDirection = cameraDirection;
		this.CurrentDistance = cameraDistance;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (Engine.IsEditorHint())
		{
			this.SetProcessUnhandledInput(false);
			return;
		}
		if (this.MouseEnabled && @event is InputEventMouseMotion mouseMotion)
		{
			this.AccumulatedMouseMovement += mouseMotion.Relative;
			if (this.AutoOrbitEnabled)
				if (this.AutoOrbitDelay?.IsRunning() == true)
				{
					this.AutoOrbitDelay.Stop();
					this.AutoOrbitDelay.Play();
				}
				else
					this.AutoOrbitDelay = this.CreateTween().AddIntervalTweener(this.AutoOrbitStartDelay);
		}
	}

	protected override void _IsLiveEnter()
	{
		base._IsLiveEnter();
		if (!this.MouseEnabled)
			return;
		Input.MouseModeEnum previousMouseMode = Input.MouseMode;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		this.Camera.Connect(
			VirtualCamera3D.SignalName.IsLiveChanged,
			Callable.From<bool>(isLive => Input.MouseMode = previousMouseMode),
			(uint) ConnectFlags.OneShot
		);
	}

	//==================================================================================================================
	// METHODS
	//==================================================================================================================
}
