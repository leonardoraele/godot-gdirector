using Godot;

namespace Raele.GDirector.Editor;

public partial class ResizeHandle : Control
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

	private bool ResizeActive = false;
	Vector2 AccumulatedMouseMovement;

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------



	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void HandleMoveStartedEventHandler();
	[Signal] public delegate void HandleMovedEventHandler(Vector2 relativeResize);
	[Signal] public delegate void HandleMoveFinishedEventHandler(Vector2 accumulatedResize);
	[Signal] public delegate void HandleMoveCanceledEventHandler(Vector2 accumulatedResize);

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
		this.Size = new Vector2(8, 8);
	}

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

	public override void _Draw()
	{
		base._Draw();
		this.DrawRect(new Rect2(Vector2.Zero, this.Size), Colors.White, filled: true);
		this.DrawRect(new Rect2(Vector2.Zero, this.Size), Colors.Black, filled: false);
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);
		if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
		{
			if (mouseButton.Pressed && !this.ResizeActive)
			{
				this.ResizeActive = true;
				this.AccumulatedMouseMovement = Vector2.Zero;
				this.EmitSignalHandleMoveStarted();
			}
			else if (!mouseButton.Pressed && this.ResizeActive)
			{
				this.ResizeActive = false;
				this.EmitSignalHandleMoveFinished(this.AccumulatedMouseMovement);
			}
		}
		else if (@event is InputEventMouseMotion mouseMotion && this.ResizeActive)
		{
			this.AccumulatedMouseMovement += mouseMotion.Relative;
			this.EmitSignalHandleMoved(mouseMotion.Relative);
		}
		else if (@event is InputEventKey keyEvent && keyEvent.Keycode == Key.Escape && keyEvent.Pressed && this.ResizeActive)
		{
			this.ResizeActive = false;
			this.EmitSignalHandleMoveCanceled(this.AccumulatedMouseMovement);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------


}
