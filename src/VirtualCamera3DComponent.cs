using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.GDirector;

[Tool][GlobalClass]
public abstract partial class VirtualCamera3DComponent : Node3D
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

	public VirtualCamera3D Camera => this.GetParent<VirtualCamera3D>();
	public bool IsLive => this.Camera.AsVirtualCamera().IsLive;

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

	public override string[] _GetConfigurationWarnings()
		=> new List<string>()
			.Concat(this.GetParentOrNull<VirtualCamera3D>() == null ? [$"This node should be a child of a {nameof(VirtualCamera3D)} node."] : [])
			.ToArray();

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
		if (Engine.IsEditorHint())
		{
			if (!this.Position.IsZeroApprox())
			{
				this.Camera.Position += this.Position;
				this.Position = Vector3.Zero;
			}
			if (!this.Rotation.IsZeroApprox())
			{
				this.Camera.Rotation += this.Rotation;
				this.Rotation = Vector3.Zero;
			}
		}
		if (this.IsLive && GDirectorServer.Instance.GodotCamera3D is Camera3D rcam)
			this.CallDeferred(MethodName._ProcessIsLive, rcam, delta);
	}

	protected virtual void _IsLiveEnter() {}
	protected virtual void _IsLiveExit() {}
	protected virtual void _ProcessIsLive(Camera3D rcam, double delta) {}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void OnIsLiveChanged(bool isLive)
	{
		if (isLive)
			this._IsLiveEnter();
		else
			this._IsLiveExit();
	}
}
