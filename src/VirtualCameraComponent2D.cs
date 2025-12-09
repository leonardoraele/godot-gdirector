using Godot;

namespace Raele.GDirector;

public partial class VirtualCameraComponent2D : Node2D
{
	public VirtualCamera2D Camera => this.GetParent<VirtualCamera2D>();
}
