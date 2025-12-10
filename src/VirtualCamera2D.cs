using System.Linq;
using System.Threading;
using Godot;

namespace Raele.GDirector;

[Tool]
public partial class VirtualCamera2D : Node2D, IVirtualCamera
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
		this.UpdateGodotCamera2D();
	}

	private void CheckPriorityChange(double oldPriority)
	{
		if (this.Priority != oldPriority) {
			this.EmitSignal(SignalName.PriorityChanged, this.Priority, oldPriority);
		}
	}

	private void UpdateGodotCamera2D()
	{
		if (!this.AsInterface().IsLive || GDirectorServer.Instance.GodotCamera2D is not Camera2D rcam)
		{
			return;
		}
		rcam.GlobalPosition = this.GlobalPosition
			- (
				rcam.AnchorMode == Camera2D.AnchorModeEnum.DragCenter
					? Vector2.Zero
					: rcam.GetViewport().GetWindow().Size / 2 + rcam.Offset
			);
		rcam.GlobalRotation = this.GlobalRotation;
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public IVirtualCamera AsInterface() => this;
	public VirtualCamera2D As2D() => this;
}
