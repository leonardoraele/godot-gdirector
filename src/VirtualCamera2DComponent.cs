using Godot;

namespace Raele.GDirector;

[Tool]
public partial class VirtualCamera2DComponent : Node2D
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	// TODO
	// [Export] public bool ProcessWhenNotLive = false;
	// [Export] public bool ProcessInEditor = false;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public VirtualCamera2D Camera => this.GetParent<VirtualCamera2D>();
	public bool IsLive => this.Camera.AsInterface().IsLive;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	// private enum ExampleEnum
	// {
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// VIRTUALS & OVERRIDES
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

	public override void _EnterTree()
	{
		base._EnterTree();
		this.Camera.IsLiveChanged += this.OnIsLiveChanged;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		this.Camera.IsLiveChanged -= this.OnIsLiveChanged;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.IsLive && GDirectorServer.Instance.GodotCamera2D is Camera2D rcam)
		{
			this._ProcessIsLive(rcam, delta);
		}
	}

	protected virtual void _IsLiveEnter() {}
	protected virtual void _IsLiveExit() {}
	protected virtual void _ProcessIsLive(Camera2D rcam, double delta) {}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void OnIsLiveChanged(bool isLive)
	{
		if (isLive)
		{
			this._IsLiveEnter();
		}
		else
		{
			this._IsLiveExit();
		}
	}
}
