using System;
using Godot;

namespace Raele.GDirector.VirtualCamera3DComponents;

// TODO Create a GroupTransitionController class that works just like this one, but that can handle transitions from
// any camera in a node group instead of a single specific camera. We could go even further and also create a
// GlobalGroupTransitionController that should be attached to the GDirectorServer directly instead of to a specific
// camera, and provides a default transition for any camera in a node group that is not handled by a specific transition
// controller.
// TODO Some times, a camera transition might be triggered while another transition is already happening.
// When this happens, the camera is cut from it's position directly to the position of the virtual camera that is both
// the end of the previous transition and the start of the new transition, and then the new transition starts.
// To avoid this cut, instead of this behavior,
// the new transition should be anchored at it's start not to that virtual camera,
// but to the target positoin of the previous transition.
// (i.e. the position the camera should be positioned if that transition had not be canceled)
// This way the camera will smoothly blend from one transition to the next without an abrupt cut.
public partial class VCam3DTweenComponent : VirtualCamera3DComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// The camera for which this transition controller is responsible to transition from.
	///
	/// If this is not set, this transition controller will be the default transition controller for the camera it is
	/// attached to, and will handle transitions from any camera for which there is no specific transition controller.
	/// </summary>
	// [Export] public VirtualCamera3D? FromCamera; // TODO
	/// <summary>
	/// The duration of the transition in seconds.
	/// </summary>
	[Export] public float DurationMs = 500f;
	/// <summary>
	/// This curve controls the transition's progress over time.
	///
	/// If this is not set, it will interpolate the cameras linearly.
	/// </summary>
	[Export] public Curve? Curve;

	// [Export(PropertyHint.Flags, "Position:1,Rotation:2")] public long UpdateFields = 3; // TODO

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Tween? Tween;

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool Ongoing => this.Tween?.IsRunning() == true;
	public TimeSpan Duration => TimeSpan.FromMilliseconds(this.DurationMs);

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void TweenStartedEventHandler();
	[Signal] public delegate void TweenFinishedEventHandler();
	[Signal] public delegate void TweenCanceledEventHandler();
	[Signal] public delegate void TweenEndedEventHandler();

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	// private enum Type {
	// 	Value1,
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	// public override void _EnterTree()
	// {
	// 	base._EnterTree();
	// }

	// public override void _ExitTree()
	// {
	// 	base._ExitTree();
	// }

	// public override void _Ready()
	// {
	// 	base._Ready();
	// }

	// public override void _Process(double delta)
	// {
	// 	base._Process(delta);
	// }

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	// }

	// public override string[] _GetConfigurationWarnings()
	// 	=> new List<string>()
	// 		.Concat(true ? [] : ["Some warning"])
	// 		.ToArray();

	// public override void _ValidateProperty(Godot.Collections.Dictionary property)
	// {
	// 	base._ValidateProperty(property);
	// }

	protected override void _IsLiveEnter()
	{
		base._IsLiveEnter();
		this.StartTransition();
	}

	protected override void _IsLiveExit()
	{
		base._IsLiveExit();
		this.CancelTransition();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public void StartTransition()
	{
		// Cancel any previous transition that might be ongoing before starting a new one
		this.CancelTransition();

		// Get the previous camera
		VirtualCamera3D? previousCamera = GDirectorServer.Instance.PreviousLiveCamera?.As3D();

		// If there is no previous camera, we can skip the transition
		if (previousCamera == null) {
			this.EmitSignal(SignalName.TweenStarted);
			GDirectorServer.Instance.GodotCamera3D?.GlobalPosition = this.Camera.GlobalPosition;
			GDirectorServer.Instance.GodotCamera3D?.GlobalRotation = this.Camera.GlobalRotation;
			this.FinishTransition();
			return;
		}

		// Setup the tween
		this.Tween = this.CreateTween();
		this.Tween.TweenMethod(
			Callable.From((float progress) =>
			{
				float lerpWeight = this.Curve?.Sample(progress) ?? progress;
				GDirectorServer.Instance.GodotCamera3D?.GlobalPosition
					= previousCamera.GlobalPosition.Lerp(this.Camera.GlobalPosition, lerpWeight);
				GDirectorServer.Instance.GodotCamera3D?.GlobalRotation
					= previousCamera.GlobalRotation.Slerp(this.Camera.GlobalRotation, lerpWeight);
			}),
			0f,
			1f,
			this.Duration.TotalSeconds
		);
		this.Tween.Finished += this.FinishTransition;

		// Emit signals
		this.EmitSignal(SignalName.TweenStarted);
	}

	public void CancelTransition()
	{
		if (this.Tween == null) {
			return;
		}

		this.Tween.Kill();
		this.Tween = null;

		// Emit signals
		this.EmitSignal(SignalName.TweenCanceled);
		this.EmitSignal(SignalName.TweenEnded);
	}

	private void FinishTransition()
	{
		if (this.Tween == null) {
			return;
		}
		this.Tween.CustomStep(float.PositiveInfinity);
		this.EmitSignal(SignalName.TweenFinished);
		this.EmitSignal(SignalName.TweenEnded);
	}
}
