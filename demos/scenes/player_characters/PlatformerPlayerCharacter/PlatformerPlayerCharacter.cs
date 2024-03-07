using Godot;

namespace Raele.GDirector.Demos;

public partial class PlatformerPlayerCharacter : CharacterBody3D
{
	/// ----------------------------------------------------------------------------------------------------------------
	/// EXPORTED PARAMETERS
	/// ----------------------------------------------------------------------------------------------------------------

	[Export] public float MaxSpeedUnPSec = 6;
	[Export] public float AccelerationUnPSecSqr = 15;
	[Export] public float DecelerationUnPSecSqr = 30;
	[Export] public float TurnSpeedDegPSec = 180;
    [Export] public float MaxFallSpeedUnPSec = 20;
    [Export] public float JumpHeightUn = 4;
    [Export] public float JumpUpDurationSec = 0.5f;
	[Export] public float SpeedMultiplierWhileAimingDownSights = 0.25f;

	/// ----------------------------------------------------------------------------------------------------------------
	/// PRIVATE FIELDS
	/// ----------------------------------------------------------------------------------------------------------------

	// Input fields
    private Vector2 DirectionalInput;
    private bool JumpQueued;
    private Vector3 CameraDirection = Vector3.Zero;
    private bool AimingDownSights;

	// Physics fields
	private float JumpForce;
	private float Gravity;
	private Vector3 GravityVector;

    /// ----------------------------------------------------------------------------------------------------------------
    /// PROPERTIES
    /// ----------------------------------------------------------------------------------------------------------------

    public float TurnSpeedRadPSec => Mathf.DegToRad(this.TurnSpeedDegPSec);

	/// ----------------------------------------------------------------------------------------------------------------
	/// METHODS
	/// ----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		base._Ready();

		// Read physics settings
		this.Gravity = (float) ProjectSettings.GetSetting("physics/3d/default_gravity").AsDouble();
		this.GravityVector = ProjectSettings.GetSetting("physics/3d/default_gravity_vector").AsVector3();

		// Calculate initial jump velocity so that the character reaches the jump height in the given time
		this.JumpForce = this.JumpHeightUn / this.JumpUpDurationSec - this.Gravity * this.JumpUpDurationSec / 2;

		// Subscribe to transition start event. This is used to reset the camera used for reference for directional
		// input when a camera cut happens.
		GDirectorServer.Instance.TransitionStart += this.OnTransitionStart;
	}

	private void OnTransitionStart(ulong nextCameraId, ulong previousCameraId, ulong transitionControllerId)
	{
		// If there is no transition controller, then it's an instantaneous camera cut. When this happens, we must save
		// the camera direction to use for input direction.
		if (transitionControllerId == 0 && this.CameraDirection == Vector3.Zero) {
			this.CameraDirection = GDirectorServer.Instance.ManagedCamera.GlobalTransform.Basis.Z with { Y = 0 };
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		// Read directional input
		this.DirectionalInput = Input.GetVector(
				"character_move_left",
				"character_move_right",
				"character_move_up",
				"character_move_down"
			)
			.Normalized();

		// Reset the camera direction reference when the player releases the movement keys.
		if (this.DirectionalInput == Vector2.Zero) {
			this.CameraDirection = Vector3.Zero;
		}

		// Buffers jump input
		this.JumpQueued = this.JumpQueued || Input.IsActionJustPressed("character_jump");

		// Change active camera group when aiming
		this.AimingDownSights = Input.IsActionPressed("character_aim");
		GDirectorServer.Instance.ActiveGroup = this.AimingDownSights ? "aim_camera" : null;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		float directionalInputLength = this.DirectionalInput.Length();

		// Calculate horizontal speed
		float currentHSpeed = (this.Velocity with { Y = 0 }).Length();
		float targetHSpeed = directionalInputLength
			* this.MaxSpeedUnPSec
			* (
				this.AimingDownSights
					? this.SpeedMultiplierWhileAimingDownSights
					: 1
			);
		float newHSpeed = targetHSpeed > currentHSpeed
			? Mathf.MoveToward(currentHSpeed, targetHSpeed, this.AccelerationUnPSecSqr * (float) delta)
			: Mathf.MoveToward(currentHSpeed, targetHSpeed, this.DecelerationUnPSecSqr * (float) delta);

		// Calculate horizontal direction
		Vector3 currentHDirection = this.GlobalTransform.Basis.Z * -1;
		Vector3 cameraDirection = (
				this.CameraDirection != Vector3.Zero
					? this.CameraDirection
					: GDirectorServer.Instance.ManagedCamera.Basis.Z with { Y = 0 }
			)
			.Normalized()
			* -1;
		Vector3 targetHDirection = directionalInputLength > Mathf.Epsilon
			? cameraDirection.Rotated(Vector3.Up, this.DirectionalInput.AngleTo(Vector2.Up))
			: currentHDirection;
		Vector3 newHDirection = this.AimingDownSights ? targetHDirection
			: currentHSpeed < Mathf.Epsilon ? targetHDirection
			: GodotUtil.RotateToward(currentHDirection, targetHDirection, this.TurnSpeedRadPSec * (float) delta);

		// Calculate vertical speed
		float newVSpeed;
		if (this.IsOnFloor()) {
			newVSpeed = this.JumpQueued ? this.JumpForce : 0;
			this.JumpQueued = false;
		} else {
			float currentVSpeed = this.Velocity.Y;
			float targetVSpeed = this.MaxFallSpeedUnPSec * -1;
			newVSpeed = Mathf.MoveToward(currentVSpeed, targetVSpeed, this.Gravity * (float) delta);
		}

		// Apply new velocity and move the character
		this.Velocity = newHDirection * newHSpeed + Vector3.Up * newVSpeed;
		this.MoveAndSlide();

		// Update facing direction
		Vector3 lookDirection = this.AimingDownSights
			? (GDirectorServer.Instance.ManagedCamera.GlobalTransform.Basis.Z with { Y = 0} * -1).Normalized()
			: newHDirection;
		this.LookAt(this.GlobalPosition + lookDirection, Vector3.Up);
	}
}
