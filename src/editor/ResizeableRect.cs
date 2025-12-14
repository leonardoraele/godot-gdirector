using Godot;

namespace Raele.GDirector.Editor;

public partial class ResizeableRect : Control
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

	public Color StrokeColor = Colors.Orange;

	private ResizeHandle TopLeftHandle = new() { MouseDefaultCursorShape = CursorShape.Fdiagsize };
	private ResizeHandle TopRightHandle = new() { MouseDefaultCursorShape = CursorShape.Bdiagsize };
	private ResizeHandle BottomLeftHandle = new() { MouseDefaultCursorShape = CursorShape.Bdiagsize };
	private ResizeHandle BottomRightHandle = new() { MouseDefaultCursorShape = CursorShape.Fdiagsize };

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	private Rect2 Rect => new Rect2(this.GlobalPosition, this.Size);

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void RectResizingEventHandler(Rect2 newRect, Rect2 oldRect);
	[Signal] public delegate void RectResizedEventHandler(Rect2 newRect, Rect2 oldRect);
	[Signal] public delegate void RectResizeCanceledEventHandler(Rect2 currentRect, Rect2 canceledRect);

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	// private enum Type {
	// 	Value1,
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	// public override void _EnterTree()
	// {
	// 	base._EnterTree();
	// }

	// public override void _ExitTree()
	// {
	// 	base._ExitTree();
	// }

	public override void _Ready()
	{
		base._Ready();
		this.MouseFilter = MouseFilterEnum.Ignore;
		this.AddChild(this.TopLeftHandle);
		this.AddChild(this.TopRightHandle);
		this.AddChild(this.BottomLeftHandle);
		this.AddChild(this.BottomRightHandle);

		// ---------------------------------------------------------------------
		// Configure TopLeftHandle
		// ---------------------------------------------------------------------

		this.TopLeftHandle.HandleMoved += (Vector2 rawResize) =>
		{
			Rect2 oldRect = this.Rect;
			this.Position += rawResize;
			this.Size -= rawResize;
			this.EmitSignalRectResizing(newRect: this.Rect, oldRect);
		};
		this.TopLeftHandle.HandleMoveFinished += (Vector2 accumulatedResize) =>
		{
			Rect2 oldRect = new Rect2(
				position: this.GlobalPosition - accumulatedResize,
				size: this.Size + accumulatedResize
			);
			this.EmitSignalRectResized(newRect: this.Rect, oldRect);
		};
		this.TopLeftHandle.HandleMoveCanceled += (Vector2 accumulatedResize) =>
		{
			Rect2 canceledRect = this.Rect;
			this.Position -= accumulatedResize;
			this.Size += accumulatedResize;
			this.EmitSignalRectResizeCanceled(currentRect: this.Rect, canceledRect);
		};

		// ---------------------------------------------------------------------
		// Configure TopRightHandle
		// ---------------------------------------------------------------------

		this.TopRightHandle.HandleMoved += (Vector2 rawResize) =>
		{
			Rect2 oldRect = this.Rect;
			this.Position = new Vector2(this.Position.X, this.Position.Y + rawResize.Y);
			this.Size = new Vector2(this.Size.X + rawResize.X, this.Size.Y - rawResize.Y);
			this.EmitSignalRectResizing(newRect: this.Rect, oldRect);
		};
		this.TopRightHandle.HandleMoveFinished += (Vector2 accumulatedResize) =>
		{
			Rect2 oldRect = new Rect2(
				position: new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y - accumulatedResize.Y),
				size: new Vector2(this.Size.X - accumulatedResize.X, this.Size.Y + accumulatedResize.Y)
			);
			this.EmitSignalRectResized(newRect: this.Rect, oldRect);
		};
		this.TopRightHandle.HandleMoveCanceled += (Vector2 accumulatedResize) =>
		{
			Rect2 canceledRect = this.Rect;
			this.Position = new Vector2(this.Position.X, this.Position.Y - accumulatedResize.Y);
			this.Size = new Vector2(this.Size.X - accumulatedResize.X, this.Size.Y + accumulatedResize.Y);
			this.EmitSignalRectResizeCanceled(currentRect: this.Rect, canceledRect);;
		};

		// ---------------------------------------------------------------------
		// Configure BottomLeftHandle
		// ---------------------------------------------------------------------

		this.BottomLeftHandle.HandleMoved += (Vector2 rawResize) =>
		{
			Rect2 oldRect = this.Rect;
			this.Position = new Vector2(this.Position.X + rawResize.X, this.Position.Y);
			this.Size = new Vector2(this.Size.X - rawResize.X, this.Size.Y + rawResize.Y);
			this.EmitSignalRectResizing(newRect: this.Rect, oldRect);
		};
		this.BottomLeftHandle.HandleMoveFinished += (Vector2 accumulatedResize) =>
		{
			Rect2 oldRect = new Rect2(
				position: new Vector2(this.GlobalPosition.X - accumulatedResize.X, this.GlobalPosition.Y),
				size: new Vector2(this.Size.X + accumulatedResize.X, this.Size.Y - accumulatedResize.Y)
			);
			this.EmitSignalRectResized(newRect: this.Rect, oldRect);
		};
		this.BottomLeftHandle.HandleMoveCanceled += (Vector2 accumulatedResize) =>
		{
			Rect2 canceledRect = this.Rect;
			this.Position = new Vector2(this.Position.X - accumulatedResize.X, this.Position.Y);
			this.Size = new Vector2(this.Size.X + accumulatedResize.X, this.Size.Y - accumulatedResize.Y);
			this.EmitSignalRectResizeCanceled(currentRect: this.Rect, canceledRect);
		};

		// ---------------------------------------------------------------------
		// Configure BottomRightHandle
		// ---------------------------------------------------------------------

		this.BottomRightHandle.HandleMoved += (Vector2 rawResize) =>
		{
			Rect2 oldRect = this.Rect;
			this.Size += rawResize;
			this.EmitSignalRectResizing(newRect: this.Rect, oldRect);
		};
		this.BottomRightHandle.HandleMoveFinished += (Vector2 accumulatedResize) =>
		{
			Rect2 oldRect = new Rect2(
				position: this.GlobalPosition,
				size: this.Size - accumulatedResize
			);
			this.EmitSignalRectResized(newRect: this.Rect, oldRect);
		};
		this.BottomRightHandle.HandleMoveCanceled += (Vector2 accumulatedResize) =>
		{
			Rect2 canceledRect = this.Rect;
			this.Size -= accumulatedResize;
			this.EmitSignalRectResizeCanceled(currentRect: this.Rect, canceledRect);
		};
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		this.TopLeftHandle.Position = this.TopLeftHandle.Size / -2;
		this.TopRightHandle.Position = new Vector2(this.Size.X, 0) + this.TopLeftHandle.Size / -2;
		this.BottomLeftHandle.Position = new Vector2(0, this.Size.Y) + this.TopLeftHandle.Size / -2;
		this.BottomRightHandle.Position = this.Size + this.TopLeftHandle.Size / -2;
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

	public override void _Draw()
	{
		base._Draw();
		float width = 3f;
		float gap = 6f;
		this.DrawDashedLine(Vector2.Zero, Vector2.Right * this.Size.X, this.StrokeColor, width, gap);
		this.DrawDashedLine(Vector2.Right * this.Size.X, this.Size, this.StrokeColor, width, gap);
		this.DrawDashedLine(this.Size, Vector2.Down * this.Size.Y, this.StrokeColor, width, gap);
		this.DrawDashedLine(Vector2.Down * this.Size.Y, Vector2.Zero, this.StrokeColor, width, gap);
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------


}
