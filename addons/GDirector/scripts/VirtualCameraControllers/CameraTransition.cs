#nullable enable
using Godot;

namespace Raele.GDirector.VirtualCameraControllers;

// TODO Create a GroupTransitionController class that works just like this one, but that can handle transitions from
// any camera in a node group instead of a single specific group. We could go even further and also create a
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
public partial class CameraTransition : VirtualCameraController
{
	/// <summary>
	/// The camera for which this transition controller is responsible to transition from.
	///
	/// If this is not set, this transition controller will be the default transition controller for the camera it is
	/// attached to, and will handle transitions from any camera for which there is no specific transition controller.
	/// </summary>
	[Export] public VirtualCamera? FromCamera;
	/// <summary>
	/// The duration of the transition in seconds.
	/// </summary>
	[Export] public float DurationSec = 0.5f;
	/// <summary>
	/// This curve controls the transition's progress over time.
	///
	/// If this is not set, it will interpolate the cameras linearly.
	/// </summary>
	[Export] public Curve? Curve;

	[Signal] public delegate void TransitionStartEventHandler();
	[Signal] public delegate void TransitionFinishEventHandler();
	[Signal] public delegate void TransitionCancelEventHandler();
	[Signal] public delegate void TransitionEndEventHandler();

    public bool Ongoing => this.Tween?.IsRunning() == true;

    public override void _EnterTree()
	{
		base._EnterTree();
		if (this.FromCamera != null) {
			this.Camera.TransitionControllers[this.FromCamera] = this;
		} else {
			this.Camera.DefaultTransitionController = this;
		}
	}

	private Tween? Tween;

    public void StartTransition()
    {
		// Cancel any previous transition that might be ongoing before starting a new one
		this.CancelTransition();

		// Get the previous camera
		VirtualCamera? previousCamera = this.FromCamera ?? GDirectorServer.Instance.PreviousActiveCamera;

		// If there is no previous camera, we can skip the transition
		if (previousCamera == null) {
			this.EmitSignal(SignalName.TransitionStart);
			GDirectorServer.Instance.ManagedCamera.GlobalPosition = this.Camera.GlobalPosition;
			GDirectorServer.Instance.ManagedCamera.GlobalRotation = this.Camera.GlobalRotation;
			this.FinishTransition();
			return;
		}

		// Setup the transition using Tweens
		this.Tween = this.CreateTween();
		this.Tween.TweenMethod(
			Callable.From((float progress) => {
				float lerpWeight = this.Curve?.Sample(progress) ?? progress;
				GDirectorServer.Instance.ManagedCamera.GlobalPosition
					= previousCamera.GlobalPosition.Lerp(this.Camera.GlobalPosition, lerpWeight);
				GDirectorServer.Instance.ManagedCamera.GlobalRotation
					= previousCamera.GlobalRotation.Lerp(this.Camera.GlobalRotation, lerpWeight);
			}),
			0f,
			1f,
			this.DurationSec
		);
		this.Tween.Finished += this.FinishTransition;

		// Emit signals
		this.EmitSignal(SignalName.TransitionStart);
    }

    public void CancelTransition()
    {
        if (this.Tween == null) {
			return;
		}

		this.Tween.Kill();
		this.Tween = null;

		// Emit signals
		this.EmitSignal(SignalName.TransitionCancel);
		this.EmitSignal(SignalName.TransitionEnd);
    }

	private void FinishTransition()
	{
		if (this.Tween == null) {
			return;
		}
		this.Tween.CustomStep(float.PositiveInfinity);
	}
}
