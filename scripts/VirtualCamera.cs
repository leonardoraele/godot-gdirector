using System.Collections.Generic;
using Godot;
using Raele.GDirector.VirtualCameraControllers;

namespace Raele.GDirector;

public partial class VirtualCamera : Node3D
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTED VARS
	// -----------------------------------------------------------------------------------------------------------------

    [Export] public float Priority;
    // [Export] public bool ResetPositionOnTransitionStart;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

    [Signal] public delegate void EnterTransitionStartEventHandler(ulong transitionControllerId);
    [Signal] public delegate void EnterTransitionFinishEventHandler();
    [Signal] public delegate void EnterTransitionCancelEventHandler();
    [Signal] public delegate void EnterTransitionEndEventHandler();
    [Signal] public delegate void ExitTransitionStartEventHandler(ulong nextCameraId, ulong transitionControllerId);
    [Signal] public delegate void ExitTransitionFinishEventHandler(ulong nextCameraId);
    [Signal] public delegate void ExitTransitionCancelEventHandler(ulong nextCameraId);
    [Signal] public delegate void ExitTransitionEndEventHandler(ulong nextCameraId);
    [Signal] public delegate void PriorityChangeEventHandler(ulong newPriority, ulong oldPriority);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

    public Dictionary<VirtualCamera, TransitionController> TransitionControllers { get; private set; } = new();
    public TransitionController? DefaultTransitionController;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

    public bool IsActive => GDirectorServer.Instance.CurrentActiveCamera == this;
    private TransitionController? ActiveTransitionController
        => GDirectorServer.Instance.PreviousActiveCamera != null
            && this.TransitionControllers.TryGetValue(GDirectorServer.Instance.PreviousActiveCamera, out TransitionController? controller)
                ? controller
                : this.DefaultTransitionController;

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

    public override void _EnterTree()
    {
        base._EnterTree();
		GDirectorServer.Instance.Register(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
		GDirectorServer.Instance.Unregister(this);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (this.IsActive && this.ActiveTransitionController?.Ongoing != true) {
            GDirectorServer.Instance.ManagedCamera.GlobalPosition = this.GlobalPosition;
            GDirectorServer.Instance.ManagedCamera.GlobalRotation = this.GlobalRotation;
        }
        this.Priority = 0;
        this.CallDeferred(nameof(this.CheckPriorityChange), this.Priority);
    }

    private void CheckPriorityChange(long oldPriority)
    {
        if (this.Priority != oldPriority) {
            this.EmitSignal(SignalName.PriorityChange, this.Priority, oldPriority);
        }
    }

    public void StartTransition()
    {
        if (!this.IsActive) {
            GDirectorServer.Instance.ActiveCameraOverride = this;
            return; // The transition will be started when the camera becomes active
        }
        GDirectorServer.Instance.PreviousActiveCamera?.EmitSignal(SignalName.ExitTransitionStart, this.GetInstanceId(), this.ActiveTransitionController?.GetInstanceId() ?? 0);
        this.EmitSignal(SignalName.EnterTransitionStart, this.ActiveTransitionController?.GetInstanceId() ?? 0);
        if (this.ActiveTransitionController != null) {
            this.ActiveTransitionController.StartTransition();
        } else {
            GDirectorServer.Instance.ManagedCamera.GlobalPosition = this.GlobalPosition;
            GDirectorServer.Instance.ManagedCamera.GlobalRotation = this.GlobalRotation;
            GDirectorServer.Instance.PreviousActiveCamera?.EmitSignal(SignalName.ExitTransitionFinish, this.GetInstanceId());
            GDirectorServer.Instance.PreviousActiveCamera?.EmitSignal(SignalName.ExitTransitionEnd, this.GetInstanceId());
            this.EmitSignal(SignalName.EnterTransitionFinish);
            this.EmitSignal(SignalName.EnterTransitionEnd);
        }
    }

    public void CancelTransition()
    {
        if (this.ActiveTransitionController?.Ongoing != true) {
			return;
		}

		// Put the transition in a neutral state
        this.ActiveTransitionController.CancelTransition();

		// Emit signals
		this.EmitSignal(SignalName.EnterTransitionCancel);
		this.EmitSignal(SignalName.EnterTransitionEnd);
		GDirectorServer.Instance.PreviousActiveCamera?.EmitSignal(SignalName.ExitTransitionCancel, this.GetInstanceId());
		GDirectorServer.Instance.PreviousActiveCamera?.EmitSignal(SignalName.ExitTransitionEnd, this.GetInstanceId());
    }
}
