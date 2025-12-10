using Godot;

namespace Raele.GDirector;

[Tool]
public partial class VirtualCameraComponent : Node
{
	public IVirtualCamera Camera => this.GetParent<IVirtualCamera>();
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

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (!this.IsLive)
		{
			return;
		}
		if (GDirectorServer.Instance.MainCamera2D is Camera2D main2D)
		{
			this._ProcessIsLive(main2D, delta);
		}
		if (GDirectorServer.Instance.MainCamera3D is Camera3D main3D)
		{
			this._ProcessIsLive(main3D, delta);
		}
	}

	private void _IsLiveChanged(bool isLive)
	{
		if (isLive)
		{
			this._IsLiveEnter();
		}
		else
		{
			this._IsLiveExit();
		}
	}

	protected virtual void _IsLiveEnter() {}
	protected virtual void _IsLiveExit() {}
	protected virtual void _ProcessIsLive(Camera2D main, double delta) {}
	protected virtual void _ProcessIsLive(Camera3D main, double delta) {}
}
