using Godot;

namespace Raele.GDirector;

public partial class VirtualCameraComponent3D : Node3D
{
	public VirtualCamera3D Camera => this.GetParent<VirtualCamera3D>();
}
