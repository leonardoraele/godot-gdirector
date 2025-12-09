using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Godot;
using Raele.GDirector.VirtualCameraComponents;

namespace Raele.GDirector;

public partial class VirtualCamera2D : Node2D, IVirtualCamera
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTED VARS
	// -----------------------------------------------------------------------------------------------------------------

	[ExportGroup("Priority")]
	[Export] public float BasePriority = 0;
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
	[Signal] public delegate void PriorityChangedEventHandler(double newPriority, double oldPriority);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public double Priority { get; set; }
	public Dictionary<VirtualCamera3D, CameraTransition3D> TransitionControllers { get; private set; } = new();
	public CameraTransition3D? DefaultTransitionController;
	private CancellationTokenSource cancelSource = new();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public bool IsActive => GDirectorServer.Instance.CurrentActiveCamera2D == this;
	private CameraTransition3D? ActiveTransitionController
		=> GDirectorServer.Instance.PreviousActiveCamera3D != null
			&& this.TransitionControllers.TryGetValue(GDirectorServer.Instance.PreviousActiveCamera3D, out CameraTransition3D? controller)
				? controller
				: this.DefaultTransitionController;

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		base._EnterTree();
		GDirectorServer.Instance.Register(this, this.cancelSource.Token);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		this.cancelSource.Cancel();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.IsActive && this.ActiveTransitionController?.Ongoing != true) {
			GDirectorServer.Instance.ManagedCamera2D.GlobalPosition = this.GlobalPosition;
			GDirectorServer.Instance.ManagedCamera2D.GlobalRotation = this.GlobalRotation;
		}
		this.CallDeferred(nameof(this.CheckPriorityChange), this.Priority);
		this.Priority = this.BasePriority;
	}

	bool IVirtualCamera.IsInGroup(string groupName) => base.IsInGroup(groupName);
	void IVirtualCamera.ConnectPriorityChanged(
		Action<double, double> callback,
		CancellationToken? cancellationToken,
		params ConnectFlags[] flags
	)
	{
		Callable callable = Callable.From(callback);
		uint uflags = flags.Select(f => (uint) f).Aggregate(0u, (a, b) => a | b);
		this.Connect(SignalName.PriorityChanged, callable, uflags);
		cancellationToken?.Register(() => this.Disconnect(SignalName.PriorityChanged, callable));
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------


	private void CheckPriorityChange(long oldPriority)
	{
		if (this.Priority != oldPriority) {
			this.EmitSignal(SignalName.PriorityChanged, this.Priority, oldPriority);
		}
	}

	public void StartTransition()
	{
		if (!this.IsActive) {
			GDirectorServer.Instance.ActiveCameraOverride = this;
			return; // The transition will be started when the camera becomes active
		}
		GDirectorServer.Instance.PreviousActiveCamera3D?.EmitSignal(SignalName.ExitTransitionStart, this.GetInstanceId(), this.ActiveTransitionController?.GetInstanceId() ?? 0);
		this.EmitSignal(SignalName.EnterTransitionStart, this.ActiveTransitionController?.GetInstanceId() ?? 0);
		if (this.ActiveTransitionController != null) {
			this.ActiveTransitionController.StartTransition();
		} else {
			GDirectorServer.Instance.ManagedCamera2D.GlobalPosition = this.GlobalPosition;
			GDirectorServer.Instance.ManagedCamera2D.GlobalRotation = this.GlobalRotation;
			GDirectorServer.Instance.PreviousActiveCamera3D?.EmitSignal(SignalName.ExitTransitionFinish, this.GetInstanceId());
			GDirectorServer.Instance.PreviousActiveCamera3D?.EmitSignal(SignalName.ExitTransitionEnd, this.GetInstanceId());
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
		GDirectorServer.Instance.PreviousActiveCamera3D?.EmitSignal(SignalName.ExitTransitionCancel, this.GetInstanceId());
		GDirectorServer.Instance.PreviousActiveCamera3D?.EmitSignal(SignalName.ExitTransitionEnd, this.GetInstanceId());
	}
}
