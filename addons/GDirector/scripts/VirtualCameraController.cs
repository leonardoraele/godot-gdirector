#nullable enable
using Godot;

namespace Raele.GDirector;

public partial class VirtualCameraController : Node
{
    protected VirtualCamera Camera { get; private set; } = null!;

    public override void _EnterTree()
	{
		base._EnterTree();
		if (this.GetParent() is not VirtualCamera camera) {
			GD.PushError(nameof(VirtualCamera) + " node must be a child of a " + nameof(VirtualCamera) + " node.");
			this.QueueFree();
			return;
		}
		this.Camera = camera;
	}
}
