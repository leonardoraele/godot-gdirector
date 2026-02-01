#if TOOLS
using Godot;
using Raele.GDirector.VirtualCamera3DComponents;

namespace Raele.GDirector.Editor;

[Tool]
public partial class OrbitPreviewer : Control
{
	//==================================================================================================================
	#region STATICS
	//==================================================================================================================

	// public static readonly string MyConstant = "";

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region EXPORTS
	//==================================================================================================================

	// [Export] public

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region FIELDS
	//==================================================================================================================

	public OrbitComponent? PreviewTarget
		{ get; set { field = value; this.QueueRedraw(); } }
	public EditorUndoRedoManager? UndoRedo;

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region COMPUTED PROPERTIES
	//==================================================================================================================



	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region EVENTS & SIGNALS
	//==================================================================================================================

	// [Signal] public delegate void EventHandler();

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region INTERNAL TYPES
	//==================================================================================================================

	// public enum Type {
	// 	Value1,
	// }

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region OVERRIDES & VIRTUALS
	//==================================================================================================================

	// public override string[] _GetConfigurationWarnings()
	// 	=> (base._GetConfigurationWarnings() ?? [])
	// 		.AppendIf(false "This node is not configured correctly. Did you forget to assign a required field?")
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

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.PreviewTarget != null)
			this.QueueRedraw();
	}

	//==================================================================================================================
	#endregion
	//==================================================================================================================
	#region METHODS
	//==================================================================================================================

	public override void _Draw()
	{
		base._Draw();
		// TODO // FIXME For some reason, both `this.PreviewTarget.GetViewport()` and
		// `((SubViewportContainer) this.PreviewTarget.GetViewport().GetParent()).Size` return a Vector2(2, 2), instead
		// of the size of the scene subviewport. So this editor is disabled for now.
		return;
		// if (this.PreviewTarget == null)
		// 	return;
		// Viewport? sceneView = this.PreviewTarget.Owner?.GetParent() as Viewport;
		// if (sceneView == null)
		// 	return;
		// Vector2 screenPos = this.PreviewTarget.GetViewport().GetCamera3D().UnprojectPosition(this.PreviewTarget.GlobalPosition);
		// this.DrawCircle(screenPos, 16, Colors.Red);
	}

	//==================================================================================================================
		#endregion
	//==================================================================================================================
}
#endif
