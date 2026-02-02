using Godot;

namespace Raele.GDirector;

[Tool]
public partial class VirtualCamera3D : Node3D, IVirtualCamera
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTED VARS
	// -----------------------------------------------------------------------------------------------------------------

	[ExportToolButton("Force Go Live/Release")] public Callable ToogleIsLiveOverrideToolButton
		=> Callable.From(() => this.AsVirtualCamera().ForceGoLive = !this.AsVirtualCamera().ForceGoLive);
	[Export] public double BasePriority { get; private set; } = 0;
	[Export] public double AdditionalLivePriority { get; private set; } = 0;

	[ExportGroup("Debug", "Debug")]
	[Export] public bool DebugShow3DGizmo
		{ get; set { field = value; this.Debug3DGizmo?.Visible = field; } }
		= false;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void PriorityChangedEventHandler(double oldPriority);
	[Signal] public delegate void IsLiveChangedEventHandler(bool isLive);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public double Priority { get; set; }
	private Node3D? Debug3DGizmo;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		base._EnterTree();
		this.AsVirtualCamera().NotifyEnteredTree();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		this.AsVirtualCamera().NotifyExitedTree();
	}

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint())
		{
			this.Debug3DGizmo = GD.Load<PackedScene>($"res://addons/{nameof(GDirector)}/assets/CameraPreview.tscn")
				.Instantiate<Node3D>();
			this.Debug3DGizmo.Visible = this.DebugShow3DGizmo;
			this.AddChild(this.Debug3DGizmo);
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (Engine.IsEditorHint())
		{
			this.SetProcess(false);
			return;
		}
		this.AsVirtualCamera().NotifyProcessing();
		this.UpdateGodotCamera3D();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void UpdateGodotCamera3D()
	{
		if (!this.AsVirtualCamera().IsLive || GDirectorServer.Instance.GodotCamera3D is not Camera3D rcam)
			return;
		rcam.GlobalPosition = this.GlobalPosition;
		rcam.GlobalRotation = this.GlobalRotation;
	}
}
