using Godot;

namespace Raele.GDirector.VirtualCamera2DComponents;

[Tool][GlobalClass]
public partial class ConfinementComponent : VirtualCamera2DComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Rect2 Region = new Rect2();

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public Vector2[] ControlPoints => [
		this.Region.Position,
		new Vector2(this.Region.End.X, this.Region.Position.Y),
		this.Region.End,
		new Vector2(this.Region.Position.X, this.Region.End.Y),
	];

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

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

	// public override void _Ready()
	// {
	// 	base._Ready();
	// }

	public override void _Process(double delta)
	{
		base._Process(delta);
		Rect2 cameraRect = this.Camera.ScreenRect;
		if (Engine.IsEditorHint())
		{
			this.Region.Size = this.Region.Size.Max(cameraRect.Size);
			this.QueueRedraw();
		}
		if (cameraRect.Position.X < this.Region.Position.X)
		{
			this.Camera.GlobalPosition = new Vector2(
				this.Region.Position.X + cameraRect.Size.X / 2,
				this.Camera.GlobalPosition.Y
			);
		}
		if (cameraRect.End.X > this.Region.End.X)
		{
			this.Camera.GlobalPosition = new Vector2(
				this.Region.End.X - cameraRect.Size.X / 2,
				this.Camera.GlobalPosition.Y
			);
		}
		if (cameraRect.Position.Y < this.Region.Position.Y)
		{
			this.Camera.GlobalPosition = new Vector2(
				this.Camera.GlobalPosition.X,
				this.Region.Position.Y + cameraRect.Size.Y / 2
			);
		}
		if (cameraRect.End.Y > this.Region.End.Y)
		{
			this.Camera.GlobalPosition = new Vector2(
				this.Camera.GlobalPosition.X,
				this.Region.End.Y - cameraRect.Size.Y / 2
			);
		}
	}

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	// }

	// public override string[] _GetConfigurationWarnings()
	// 	=> new List<string>()
	// 		.Concat(this.Area == null ? [$"{nameof(Area)} field is null."] : [])
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

	// public override void _Draw()
	// {
	// 	base._Draw();

	// 	if (!Engine.IsEditorHint())
	// 	{
	// 		return;
	// 	}

	// 	Vector2[] drawPoints = this.ControlPoints.Select(this.ToLocal).ToArray();
	// 	float width = (this.GetViewport().CanvasTransform * Vector2.Up * -12f).Y;
	// 	float gap = width * 2f;
	// 	this.DrawDashedLine(drawPoints[0], drawPoints[1], Colors.Red, width, gap);
	// 	this.DrawDashedLine(drawPoints[1], drawPoints[2], Colors.Red, width, gap);
	// 	this.DrawDashedLine(drawPoints[2], drawPoints[3], Colors.Red, width, gap);
	// 	this.DrawDashedLine(drawPoints[3], drawPoints[0], Colors.Red, width, gap);
	// }

	// public void DrawOverViewport(Control viewport)
	// {
	// 	Transform2D transform = this.GetViewport().GetFinalTransform();
	// 	Vector2[] drawPoints = this.ControlPoints.Select(point => transform * point).ToArray();

	// 	float width = 3f;
	// 	float gap = 6f;
	// 	float handleSize = 6f;
	// 	viewport.DrawDashedLine(drawPoints[0], drawPoints[1], Colors.Red, width, gap);
	// 	viewport.DrawDashedLine(drawPoints[1], drawPoints[2], Colors.Red, width, gap);
	// 	viewport.DrawDashedLine(drawPoints[2], drawPoints[3], Colors.Red, width, gap);
	// 	viewport.DrawDashedLine(drawPoints[3], drawPoints[0], Colors.Red, width, gap);
	// 	viewport.DrawRect(new Rect2(drawPoints[0], Vector2.Zero).Grow(handleSize), Colors.White, filled: true);
	// 	viewport.DrawRect(new Rect2(drawPoints[0], Vector2.Zero).Grow(handleSize), Colors.Black, width: 2f, filled: false);
	// 	viewport.DrawRect(new Rect2(drawPoints[1], Vector2.Zero).Grow(handleSize), Colors.White, filled: true);
	// 	viewport.DrawRect(new Rect2(drawPoints[1], Vector2.Zero).Grow(handleSize), Colors.Black, width: 2f, filled: false);
	// 	viewport.DrawRect(new Rect2(drawPoints[2], Vector2.Zero).Grow(handleSize), Colors.White, filled: true);
	// 	viewport.DrawRect(new Rect2(drawPoints[2], Vector2.Zero).Grow(handleSize), Colors.Black, width: 2f, filled: false);
	// 	viewport.DrawRect(new Rect2(drawPoints[3], Vector2.Zero).Grow(handleSize), Colors.White, filled: true);
	// 	viewport.DrawRect(new Rect2(drawPoints[3], Vector2.Zero).Grow(handleSize), Colors.Black, width: 2f, filled: false);
	// }

	// public override void _Input(InputEvent @event)
	// {
	// 	base._Input(@event);
	// 	if (@event is InputEventMouse)
	// 	{
	// 		Vector2[] controlPoints = this.ControlPoints;
	// 		float controlDistanceSqr = 12f * 12f;
	// 		if (@event is InputEventMouseMotion mouseMotion)
	// 		{
	// 			switch (this.CurrentActiveControlPointIndex)
	// 			{
	// 				case 0:
	// 					this.Region.Position += mouseMotion.Relative;
	// 					this.Region.Size -= mouseMotion.Relative;
	// 					break;
	// 				case 1:
	// 					this.Region.Position += new Vector2(0, mouseMotion.Relative.Y);
	// 					this.Region.Size += new Vector2(mouseMotion.Relative.X, -mouseMotion.Relative.Y);
	// 					break;
	// 				case 2:
	// 					this.Region.Size += mouseMotion.Relative;
	// 					break;
	// 				case 3:
	// 					this.Region.Position += new Vector2(mouseMotion.Relative.X, 0);
	// 					this.Region.Size += new Vector2(-mouseMotion.Relative.X, mouseMotion.Relative.Y);
	// 					break;
	// 				default:
	// 					if (
	// 						this.GetGlobalMousePosition().DistanceSquaredTo(controlPoints[0]) <= controlDistanceSqr
	// 						|| this.GetGlobalMousePosition().DistanceSquaredTo(controlPoints[2]) <= controlDistanceSqr
	// 					)
	// 					{
	// 						Input.SetDefaultCursorShape(Input.CursorShape.Bdiagsize);
	// 					}
	// 					else if (
	// 						this.GetGlobalMousePosition().DistanceSquaredTo(controlPoints[1]) <= controlDistanceSqr
	// 						|| this.GetGlobalMousePosition().DistanceSquaredTo(controlPoints[3]) <= controlDistanceSqr
	// 					)
	// 					{
	// 						Input.SetDefaultCursorShape(Input.CursorShape.Fdiagsize);
	// 					}
	// 					else
	// 					{
	// 						Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
	// 					}
	// 					break;
	// 			}
	// 		}
	// 		else if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
	// 		{
	// 			this.CurrentActiveControlPointIndex = -1;
	// 			if (mouseButton.Pressed)
	// 			{
	// 				for (int i = 0; i < controlPoints.Length; i++)
	// 				{
	// 					if (this.GetGlobalMousePosition().DistanceSquaredTo(controlPoints[i]) <= controlDistanceSqr)
	// 					{
	// 						this.CurrentActiveControlPointIndex = i;
	// 						this.GetViewport().SetInputAsHandled();
	// 						break;
	// 					}
	// 				}
	// 			}
	// 		}
	// 	}
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

}
