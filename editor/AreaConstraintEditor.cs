using Godot;
using Raele.GDirector.VirtualCamera2DComponents;

namespace Raele.GDirector.Editor;

public partial class AreaConstraintEditor : ResizeableRect
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	// [Export] public

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public AreaConstraintComponent2D? Component
	{
		get => field;
		set
		{
			field = value;
			this.Visible = field != null;
		}
	} = null;

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------



	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler();

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	// private enum Type {
	// 	Value1,
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		base._EnterTree();
		this.TargetResized += this.OnTargetResized;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		this.TargetResized -= this.OnTargetResized;
	}

	// public override void _Ready()
	// {
	// 	base._Ready();
	// }

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (this.Component == null)
		{
			return;
		}

		this.Position = this.Component.GetViewportTransform() * this.Component.Region.Position;
		this.Size = this.Component.GetViewportTransform() * this.Component.Region.End - this.Position;
	}

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	// }

	// public override string[] _GetConfigurationWarnings()
	// 	=> new List<string>()
	// 		.Concat(true ? ["Some warning"] : [])
	// 		.ToArray();

	// public override void _ValidateProperty(Godot.Collections.Dictionary property)
	// {
	// 	base._ValidateProperty(property);
	// 	switch (property["name"].AsString())
	// 	{
	// 		case nameof():
	// 			break;
	// 	}
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	private void OnTargetResized()
	{
		this.Component?.Region.Position = this.Component.GetViewport().GetScreenTransform().AffineInverse()
			* this.GetViewport().GetScreenTransform()
			* this.GlobalPosition;
		this.Component?.Region.End = this.Component.GetViewport().GetScreenTransform().AffineInverse()
			* this.GetViewport().GetScreenTransform()
			* (this.GlobalPosition + this.Size);
	}
}
