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
		=> Callable.From(() =>
		{
			if (Engine.IsEditorHint()) return;
			this.AsInterface().ForceGoLive = !this.AsInterface().ForceGoLive;
		});
	[Export] public double BasePriority { get; private set; } = 0;
	[Export(PropertyHint.Link)] public Vector2 Zoom
		{ get => field; set { field = value; this.QueueRedraw(); } }
		= Vector2.One;

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

	public Vector2 ScreenSize => GDirectorServer.Instance.ScreenSize / this.Zoom;
	public Rect2 ScreenRect => new Rect2(this.GlobalPosition - this.ScreenSize / 2, this.ScreenSize);

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		base._EnterTree();
		this.AsInterface()._EnterTree(this.ServerRegistrationCancelSource.Token);
		this.TopLevel = true;
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
		if (Engine.IsEditorHint())
		{
			this.SetProcess(false);
			return;
		}
		this.AsInterface()._Process();
		this.CallDeferred(MethodName.CheckPriorityChange, this.Priority);
		this.UpdateGodotCamera2D();
	}

	public override void _Draw()
	{
		base._Draw();

		if (!Engine.IsEditorHint())
		{
			return;
		}

		Vector2 screenSize = GDirectorServer.Instance.ScreenSize / this.Zoom;
		this.DrawRect(new Rect2(screenSize / -2, screenSize), Colors.Blue with { A = 0.5f }, false);
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public IVirtualCamera AsInterface() => this;
	public VirtualCamera2D As2D() => this;

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
}
