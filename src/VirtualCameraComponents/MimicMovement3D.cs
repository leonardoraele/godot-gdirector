using Godot;

namespace Raele.GDirector.VirtualCameraComponents;

public partial class MimicMovement3D : VirtualCameraComponent
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATICS
	// -----------------------------------------------------------------------------------------------------------------

	// public static readonly string MyConstant = "";

	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTS
	// -----------------------------------------------------------------------------------------------------------------

	[Export] public Node3D? Reference;
	/// <summary>
	/// If false, the camera will move to the exact position of the reference node at the start of the scene. If true,
	/// the camera will preserve it's starting position.
	/// </summary>
	[Export] public bool PreserveDistance = true;

	[ExportGroup("Smoothing")]
	[Export(PropertyHint.Range, "0.01,1,0.01")] public float LerpWeight = 1f;
	[Export] public float MaxDistance = 4f;

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	private Vector3 offset;

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
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
	// EVENTS
	// -----------------------------------------------------------------------------------------------------------------

	// public override void _EnterTree()
	// {
	// 	base._EnterTree();
	// }

	public override void _Ready()
	{
		base._Ready();
		this.offset = this.Reference != null ? this.Camera.GlobalPosition - this.Reference.GlobalPosition : Vector3.Zero;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.Reference == null) {
			return;
		}
		Vector3 targetPosition = this.Reference.GlobalPosition + this.offset;
		Vector3 candidateNewPosition = this.Camera.GlobalPosition.Lerp(targetPosition, this.LerpWeight);
		float distanceToTarget = candidateNewPosition.DistanceTo(targetPosition);
		this.Camera.GlobalPosition = distanceToTarget <= this.MaxDistance
			? candidateNewPosition
			: targetPosition.MoveToward(this.Camera.GlobalPosition, this.MaxDistance);
	}

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);
	// }

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------


}
