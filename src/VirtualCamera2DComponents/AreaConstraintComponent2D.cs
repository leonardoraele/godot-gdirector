using Godot;

namespace Raele.GDirector.VirtualCamera2DComponents;

[Tool]
public partial class AreaConstraintComponent2D : VirtualCamera2DComponent
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

	// public Rect2 PositionConstraintRect => this.Area == null
	// 	? new Rect2(
	// 		new Vector2(float.NegativeInfinity, float.NegativeInfinity),
	// 		new Vector2(float.PositiveInfinity, float.PositiveInfinity)
	// 	)
	// 	: this.Area.Coll

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

	public override void _Draw()
	{
		base._Draw();

		if (!Engine.IsEditorHint())
		{
			return;
		}

		Rect2 regionRect = new Rect2(this.ToLocal(this.Region.Position), this.Region.Size);
		Vector2[] points = [
			regionRect.Position,
			new Vector2(regionRect.End.X, regionRect.Position.Y),
			regionRect.End,
			new Vector2(regionRect.Position.X, regionRect.End.Y),
		];
		float width = 8f;
		float gap = 12f;
		this.DrawDashedLine(points[0], points[1], Colors.Red, width, gap);
		this.DrawDashedLine(points[1], points[2], Colors.Red, width, gap);
		this.DrawDashedLine(points[2], points[3], Colors.Red, width, gap);
		this.DrawDashedLine(points[3], points[0], Colors.Red, width, gap);
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------


}
