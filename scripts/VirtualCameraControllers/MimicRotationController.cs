using Godot;

namespace Raele.GDirector.VirtualCameraControllers;

public partial class MimicRotationController : VirtualCameraController
{
	[Export] Node3D? RotationReference;
	[Export] Vector3 EulerOffset;

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.RotationReference == null) {
			return;
		}
		this.Camera.GlobalRotation = this.RotationReference.GlobalRotation + this.EulerOffset;
	}
}
