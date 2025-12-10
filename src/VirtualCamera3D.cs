using System.Threading;
using Godot;

namespace Raele.GDirector;

[Tool]
public partial class VirtualCamera3D : Node3D, IVirtualCamera
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTED VARS
	// -----------------------------------------------------------------------------------------------------------------

	[ExportToolButton("Force Go Live/Release")] public Callable ToogleIsLiveOverrideToolButton
		=> Callable.From(() => this.AsInterface().ForceGoLive = !this.AsInterface().ForceGoLive);
	[Export] public double BasePriority { get; private set; } = 0;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void PriorityChangedEventHandler(double newPriority, double oldPriority);
	[Signal] public delegate void IsLiveChangedEventHandler(bool isLive);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public double Priority { get; set; }

	private CancellationTokenSource ServerRegistrationCancelSource = new();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		base._EnterTree();
		this.AsInterface()._EnterTree(this.ServerRegistrationCancelSource.Token);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		this.AsInterface()._ExitTree();
		this.ServerRegistrationCancelSource.Cancel();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		this.AsInterface()._Process();
		this.CallDeferred(MethodName.CheckPriorityChange, this.Priority);
	}

	private void CheckPriorityChange(double oldPriority)
	{
		if (this.Priority != oldPriority) {
			this.EmitSignal(SignalName.PriorityChanged, this.Priority, oldPriority);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public IVirtualCamera AsInterface() => this;
	public VirtualCamera3D As3D() => this;
}
