using Godot;

namespace Raele.GDirector;

public partial class VirtualCameraComponent : Node
{
	public IVirtualCamera Camera => this.GetParent<IVirtualCamera>();
}
