using Godot;
using Raele.GDirector.VirtualCamera2DComponents;

namespace Raele.GDirector.Editor;

public partial class ConfiningRegionEditor : ResizeableRect
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

	public ConfinementComponent? EditTarget
	{
		get => field;
		set
		{
			field = value;
			this.Visible = field != null;
			this.Refresh();
		}
	} = null;
	public EditorUndoRedoManager? UndoRedo;

	private UndoRedo.MergeMode MergeMode = Godot.UndoRedo.MergeMode.Disable;

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private Transform2D OverlayToWorld => (this.EditTarget?.GetViewport().GetScreenTransform().AffineInverse() ?? Transform2D.Identity)
		* (this.GetViewport()?.GetScreenTransform() ?? Transform2D.Identity);

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
		this.RectResizing += this.OnRectResizing;
		this.RectResized += this.OnRectResized;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		this.RectResizing -= this.OnRectResizing;
		this.RectResized -= this.OnRectResized;
	}

	// public override void _Ready()
	// {
	// 	base._Ready();
	// }

	public override void _Process(double delta)
	{
		base._Process(delta);
		this.Refresh();
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

	private void Refresh()
	{
		if (this.EditTarget == null)
		{
			return;
		}

		this.GlobalPosition = this.OverlayToWorld.AffineInverse() * this.EditTarget.Region.Position;
		this.Size = this.OverlayToWorld.AffineInverse() * this.EditTarget.Region.End - this.GlobalPosition;
	}

	private void OnRectResizing(Rect2 newRect, Rect2 oldRect)
	{
		if (this.EditTarget == null)
			return;

		Rect2 newRegion = this.OverlayToWorld * newRect;
		Rect2 oldRegion = this.OverlayToWorld * oldRect;

		this.UndoRedo?.CreateAction(
			$"Resize region of \"{this.EditTarget.Name}\" node ({nameof(ConfinementComponent)})",
			this.MergeMode,
			customContext: this.EditTarget
		);
		this.UndoRedo?.AddDoProperty(this.EditTarget, ConfinementComponent.PropertyName.Region, newRegion);
		this.UndoRedo?.AddUndoProperty(this.EditTarget, ConfinementComponent.PropertyName.Region, oldRegion);
		this.UndoRedo?.CommitAction();
		this.MergeMode = Godot.UndoRedo.MergeMode.Ends; // subsequent changes will be merged
	}

	private void OnRectResized(Rect2 newRect, Rect2 oldRect)
		=> this.MergeMode = Godot.UndoRedo.MergeMode.Disable; // stops merging changes; next change will be a new history item
}
