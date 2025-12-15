using Godot;

namespace Raele.GDirector.VirtualCamera3DComponents;

// TODO Add option to prevent Z rotation
public partial class VCam3DMimicRotationComponent : VirtualCamera3DComponent
{
	[Export] public Node3D? RotationReference;
	[Export] public Vector3 EulerOffsetDeg;

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.RotationReference == null) {
			return;
		}
		this.Camera.GlobalRotation = this.RotationReference.GlobalRotation + this.EulerOffsetDeg;
	}
}
