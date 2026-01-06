using System.Linq;
using Godot;

namespace Raele.GDirector.VirtualCamera2DComponents;

[GlobalClass]
public partial class ObjectDetectionComponent : VirtualCamera2DComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Area2D? Area = null;
	[Export] public float PriorityAdd = 1f;
	[Export] public bool MultiplyByObjectCountInArea = false;

	[ExportGroup("Filter", "Filter")]
	[Export] public string FilterByNodeGroup = "";

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------



	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------



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

		if (this.Area == null)
		{
			return;
		}

		if (this.MultiplyByObjectCountInArea)
		{
			this.Camera.Priority += this.PriorityAdd
				* this.Area.GetOverlappingBodies()
					.Where(
						string.IsNullOrWhiteSpace(this.FilterByNodeGroup)
							? _ => true
							: body => body.IsInGroup(this.FilterByNodeGroup)
					)
					.Count();
		}
		else if (
			string.IsNullOrWhiteSpace(this.FilterByNodeGroup)
				? this.Area.HasOverlappingBodies() == true
				: this.Area.GetOverlappingBodies().Any(body => body.IsInGroup(this.FilterByNodeGroup))
		)
		{
			this.Camera.Priority += this.PriorityAdd;
		}
	}

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
	// 	switch (property["name"].AsString())
	// 	{
	// 		case nameof():
	// 			break;
	// 	}
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

}
