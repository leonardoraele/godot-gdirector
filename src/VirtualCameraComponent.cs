using Godot;

namespace Raele.GDirector;

[Tool]
public partial class VirtualCameraComponent : Node
{
	public IVirtualCamera Camera => this.GetParent<IVirtualCamera>();
	public Camera2D? MainCamera2D => GDirectorServer.Instance.MainCamera2D;
	public Camera3D? MainCamera3D => GDirectorServer.Instance.MainCamera3D;
	public bool IsLive => this.Camera.IsLive;

	public override void _EnterTree()
	{
		base._EnterTree();
		this.Camera.AsNode().Connect(IVirtualCamera.SignalName_IsLiveChanged, new Callable(this, MethodName._IsLiveChanged));
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		this.Camera.AsNode().Disconnect(IVirtualCamera.SignalName_IsLiveChanged, new Callable(this, MethodName._IsLiveChanged));
	}

	protected virtual void _IsLiveChanged(bool isLive) {}
}
