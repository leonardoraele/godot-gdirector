using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.GDirector.VirtualCamera2DComponents;

[Tool]
public partial class VCam2DFramingComponent : VirtualCamera2DComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Node2D? FramingTarget
		{ get => field; set { field = value; this.NotifyPropertyListChanged(); }}
		= null;
	[Export(PropertyHint.Range, "0,1")] public Vector2 ScreenPosition
		{ get => field; set { field = value; this.QueueRedraw(); } }
		= new Vector2(0.5f, 0.5f);

	[ExportGroup("Target Offset")]
	[Export] public Vector2 TargetOffset;
	[Export] public bool OffsetIsGlobal = false;

	[ExportGroup("Dead Zone")]
	// [Export] public DeadZoneTypeEnum DeadZoneType = DeadZoneTypeEnum.Rectangle;
	[Export(PropertyHint.Range, "0,1")] public Vector2 DeadZoneSize
		{ get => field; set { field = value; this.QueueRedraw(); } }
		= Vector2.Zero;
	[Export] public bool CenterOnActivation = false;

	[ExportGroup("Damping")]
	[Export(PropertyHint.Range, "0,1")] public float SoftZoneTopMargin
		{ get => field; set { field = value; this.QueueRedraw(); } }
		= 0f;
	[Export(PropertyHint.Range, "0,1")] public float SoftZoneRightMargin
		{ get => field; set { field = value; this.QueueRedraw(); } }
		= 0f;
	[Export(PropertyHint.Range, "0,1")] public float SoftZoneBottomMargin
		{ get => field; set { field = value; this.QueueRedraw(); } }
		= 0f;
	[Export(PropertyHint.Range, "0,1")] public float SoftZoneLeftMargin
		{ get => field; set { field = value; this.QueueRedraw(); } }
		= 0f;
	[Export(PropertyHint.Range, "0.001,1")] public float LerpWeight
		{ get => field; set { field = value; this.QueueRedraw(); } }
		= 0.15f;

	[ExportGroup("Debug", "Debug")]
	[Export(PropertyHint.Flags, "InEditor:1,InRuntime:2")] public byte DebugShowFramingGuides
		{ get => field; set { field = value; this.QueueRedraw(); } }
		= 1;
	[Export(PropertyHint.Range, "0,1")] public float DebugGuideOpacity
		{ get => field; set { field = value; this.QueueRedraw(); } }
		= 0.25f;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// The global position of the framing target, or Vector2.Zero if no target is set.
	/// </summary>
	public Vector2 FramingTargetGlobalPosition => this.FramingTarget?.GlobalPosition ?? Vector2.Zero;
	/// <summary>
	/// The global position of the framing target plus the target offset. This world position should be in the defined
	/// screen position or the dead zone.
	/// </summary>
	public Vector2 OffesetFramingTargetGlobalPosition
		=> this.FramingTargetGlobalPosition
			+ (
				this.OffsetIsGlobal
					? this.TargetOffset
					: this.TargetOffset.Rotated(this.FramingTarget?.GlobalRotation ?? 0)
			);
	/// <summary>
	/// The global position that is in <see cref="ScreenPosition"/> at this time.
	/// </summary>
	public Vector2 GlobalFocusPointPosition => this.Camera.GlobalPosition
		+ (this.ScreenPosition - new Vector2(0.5f, 0.5f)) * this.Camera.ScreenSize;
	/// <summary>
	/// The dead zone rectangle in world coordinates.
	/// </summary>
	public Rect2 GlobalDeadZoneRect => new Rect2(
			this.GlobalFocusPointPosition - this.DeadZoneSize * 0.5f * this.Camera.ScreenSize,
			this.DeadZoneSize * this.Camera.ScreenSize
		)
		.Intersection(this.GlobalScreenRect);
	public Rect2 GlobalHardLimitRect => new Rect2()
		{
			Position = this.GlobalDeadZoneRect.Position
				- new Vector2(this.SoftZoneLeftMargin, this.SoftZoneTopMargin)
				* this.Camera.ScreenSize,
			End = this.GlobalDeadZoneRect.End
				+ new Vector2(this.SoftZoneRightMargin, this.SoftZoneBottomMargin)
				* this.Camera.ScreenSize
		}
		.Intersection(this.GlobalScreenRect);
	private Rect2 GlobalScreenRect => new Rect2(
			this.Camera.GlobalPosition - this.Camera.ScreenSize / 2,
			this.Camera.ScreenSize
		);

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	// [Signal] public delegate void EventHandler()

	// -----------------------------------------------------------------------------------------------------------------
	// INTERNAL TYPES
	// -----------------------------------------------------------------------------------------------------------------

	// public enum DeadZoneTypeEnum
	// {
	// 	Rectangle,
	// 	Ellipse
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	// public override void _EnterTree()
	// {
	// 	base._EnterTree();
	// }

	// public override void _Ready()
	// {
	// 	base._Ready();
	// }

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (this.FramingTarget == null) {
			return;
		}

		Vector2 targetPos = this.OffesetFramingTargetGlobalPosition;
		Rect2 deadZone = this.GlobalDeadZoneRect;
		Rect2 hardLimit = this.GlobalHardLimitRect;
		Vector2 cameraPos = this.Camera.GlobalPosition;

		// Lerp to dead zone
		cameraPos.X += targetPos.X < deadZone.Position.X ? Mathf.Lerp(0, targetPos.X - deadZone.Position.X, this.LerpWeight)
			: targetPos.X > deadZone.End.X ? Mathf.Lerp(0, targetPos.X - deadZone.End.X, this.LerpWeight)
			: 0;
		cameraPos.Y += targetPos.Y < deadZone.Position.Y ? Mathf.Lerp(0, targetPos.Y - deadZone.Position.Y, this.LerpWeight)
			: targetPos.Y > deadZone.End.Y ? Mathf.Lerp(0, targetPos.Y - deadZone.End.Y, this.LerpWeight)
			: 0;

		// Clamp to hard limit
		cameraPos.X += targetPos.X < hardLimit.Position.X ? this.OffesetFramingTargetGlobalPosition.X - hardLimit.Position.X
			: targetPos.X > hardLimit.End.X ? this.OffesetFramingTargetGlobalPosition.X - hardLimit.End.X
			: 0;
		cameraPos.Y += targetPos.Y < hardLimit.Position.Y ? this.OffesetFramingTargetGlobalPosition.Y - hardLimit.Position.Y
			: targetPos.Y > hardLimit.End.Y ? this.OffesetFramingTargetGlobalPosition.Y - hardLimit.End.Y
			: 0;

		// Lerp to new position
		this.Camera.GlobalPosition = cameraPos;
	}

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	// }

	public override string[] _GetConfigurationWarnings()
		=> new List<string>()
			.Concat(this.FramingTarget == null ? [$"{nameof(FramingTarget)} field is null."] : [])
			.ToArray();

	// public override void _ValidateProperty(Godot.Collections.Dictionary property)
	// {
	// 	base._ValidateProperty(property);
	// 	float maxDeadZoneX = Math.Min(1 - this.ScreenPosition.X, this.ScreenPosition.X);
	// 	float maxDeadZoneY = Math.Min(1 - this.ScreenPosition.Y, this.ScreenPosition.Y);
	// 	switch (property["name"].AsString())
	// 	{
	// 		case nameof(this.DeadZoneX):
	// 			property.DefineProperty(hintString: $"0,{maxDeadZoneX}");
	// 			break;
	// 		case nameof(this.DeadZoneY):
	// 			property.DefineProperty(hintString: $"0,{maxDeadZoneY}");
	// 			break;
	// 		case nameof(this.HardLimitX):
	// 			float maxHardLimitX = maxDeadZoneX - this.DeadZoneX;
	// 			property.DefineProperty(hintString: $"0,{maxHardLimitX}");
	// 			break;
	// 		case nameof(this.HardLimitY):
	// 			float maxHardLimitY = maxDeadZoneY - this.DeadZoneY;
	// 			property.DefineProperty(hintString: $"0,{maxHardLimitY}");
	// 			break;
	// 	}
	// }

	protected override void _IsLiveEnter()
	{
		base._IsLiveEnter();
		if (this.CenterOnActivation)
		{
			this.Camera.GlobalPosition = this.OffesetFramingTargetGlobalPosition
				- (this.ScreenPosition - new Vector2(0.5f, 0.5f)) * this.Camera.ScreenSize;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Draw()
	{
		base._Draw();

		if (
			Engine.IsEditorHint() && (this.DebugShowFramingGuides & 1) == 0
			|| !Engine.IsEditorHint() && (this.DebugShowFramingGuides & 2) == 0
			|| OS.HasFeature("release")
		)
		{
			return;
		}

		Rect2 screenRect = new Rect2(this.Camera.ScreenSize / -2, this.Camera.ScreenSize);
		Rect2 hardLimitRect = new Rect2(
			this.ToLocal(this.GlobalHardLimitRect.Position),
			this.GlobalHardLimitRect.Size.Max(Vector2.One)
		);
		Rect2 deadZoneRect = new Rect2(
			this.ToLocal(this.GlobalDeadZoneRect.Position),
			this.GlobalDeadZoneRect.Size.Max(Vector2.One)
		);

		// Draw hard limit stroke
		this.DrawRect(hardLimitRect, Colors.Red with { A = this.DebugGuideOpacity * 2 }, filled: false, 8f);
		// Draw hard limit area (top)
		this.DrawColoredPolygon(
			[
				new Vector2(screenRect.Position.X, screenRect.Position.Y),
				new Vector2(screenRect.End.X, screenRect.Position.Y),
				new Vector2(hardLimitRect.End.X, hardLimitRect.Position.Y),
				new Vector2(hardLimitRect.Position.X, hardLimitRect.Position.Y)
			],
			Colors.Red with { A = this.DebugGuideOpacity }
		);
		// Draw hard limit area (right)
		this.DrawColoredPolygon(
			[
				new Vector2(hardLimitRect.End.X, hardLimitRect.Position.Y),
				new Vector2(screenRect.End.X, screenRect.Position.Y),
				new Vector2(screenRect.End.X, screenRect.End.Y),
				new Vector2(hardLimitRect.End.X, hardLimitRect.End.Y)
			],
			Colors.Red with { A = this.DebugGuideOpacity }
		);
		// Draw hard limit area (bottom)
		this.DrawColoredPolygon(
			[
				new Vector2(hardLimitRect.Position.X, hardLimitRect.End.Y),
				new Vector2(hardLimitRect.End.X, hardLimitRect.End.Y),
				new Vector2(screenRect.End.X, screenRect.End.Y),
				new Vector2(screenRect.Position.X, screenRect.End.Y)
			],
			Colors.Red with { A = this.DebugGuideOpacity }
		);
		// Draw hard limit area (left)
		this.DrawColoredPolygon(
			[
				new Vector2(screenRect.Position.X, screenRect.Position.Y),
				new Vector2(hardLimitRect.Position.X, hardLimitRect.Position.Y),
				new Vector2(hardLimitRect.Position.X, hardLimitRect.End.Y),
				new Vector2(screenRect.Position.X, screenRect.End.Y)
			],
			Colors.Red with { A = this.DebugGuideOpacity }
		);

		// Draw soft zone stroke
		this.DrawRect(deadZoneRect, Colors.Blue with { A = this.DebugGuideOpacity * 2 }, filled: false, 8f);
		// Draw soft zone area (top)
		this.DrawColoredPolygon(
			[
				new Vector2(hardLimitRect.Position.X, hardLimitRect.Position.Y),
				new Vector2(hardLimitRect.End.X, hardLimitRect.Position.Y),
				new Vector2(deadZoneRect.End.X, deadZoneRect.Position.Y),
				new Vector2(deadZoneRect.Position.X, deadZoneRect.Position.Y)
			],
			Colors.Blue with { A = this.DebugGuideOpacity }
		);
		// Draw soft zone area (right)
		this.DrawColoredPolygon(
			[
				new Vector2(deadZoneRect.End.X, deadZoneRect.Position.Y),
				new Vector2(hardLimitRect.End.X, hardLimitRect.Position.Y),
				new Vector2(hardLimitRect.End.X, hardLimitRect.End.Y),
				new Vector2(deadZoneRect.End.X, deadZoneRect.End.Y)
			],
			Colors.Blue with { A = this.DebugGuideOpacity }
		);
		// Draw soft zone area (bottom)
		this.DrawColoredPolygon(
			[
				new Vector2(deadZoneRect.Position.X, deadZoneRect.End.Y),
				new Vector2(deadZoneRect.End.X, deadZoneRect.End.Y),
				new Vector2(hardLimitRect.End.X, hardLimitRect.End.Y),
				new Vector2(hardLimitRect.Position.X, hardLimitRect.End.Y)
			],
			Colors.Blue with { A = this.DebugGuideOpacity }
		);
		// Draw soft zone area (left)
		this.DrawColoredPolygon(
			[
				new Vector2(hardLimitRect.Position.X, hardLimitRect.Position.Y),
				new Vector2(deadZoneRect.Position.X, deadZoneRect.Position.Y),
				new Vector2(deadZoneRect.Position.X, deadZoneRect.End.Y),
				new Vector2(hardLimitRect.Position.X, hardLimitRect.End.Y)
			],
			Colors.Blue with { A = this.DebugGuideOpacity }
		);

		// Draw focus point
		Vector2 focusPoint = this.ToLocal(this.GlobalFocusPointPosition);
		float crosshairSize = 32f;
		float crosshairWidth = 4f;
		this.DrawLine(focusPoint + Vector2.Left * crosshairSize / 2, focusPoint + Vector2.Right * crosshairSize / 2, Colors.White, crosshairWidth);
		this.DrawLine(focusPoint + Vector2.Up * crosshairSize / 2, focusPoint + Vector2.Down * crosshairSize / 2, Colors.White, crosshairWidth);
	}
}
