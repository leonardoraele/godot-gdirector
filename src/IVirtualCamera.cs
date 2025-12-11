using Godot;

namespace Raele.GDirector;

public interface IVirtualCamera
{
	// -------------------------------------------------------------------------
	// STATICS
	// -------------------------------------------------------------------------

	public static readonly string SignalName_PriorityChanged = "PriorityChanged";
	public static readonly string SignalName_IsLiveChanged = "IsLiveChanged";

	public partial class Wrapper : GodotObject
	{
		public IVirtualCamera? Camera { get; private init; }
		public Wrapper() => this.Camera = null;
		public Wrapper(IVirtualCamera? camera) => this.Camera = camera;
	}

	// -------------------------------------------------------------------------
	// GDirector properties
	// -------------------------------------------------------------------------

	protected double BasePriority { get; }
	public double Priority { get; set; }

	// -------------------------------------------------------------------------
	// Methods
	// -------------------------------------------------------------------------

	public VirtualCamera2D? As2D() => null;
	public VirtualCamera3D? As3D() => null;

	// -------------------------------------------------------------------------
	// Implementations
	// -------------------------------------------------------------------------

	public bool IsLive => GDirectorServer.Instance.CurrentLiveCamera == this;
	public bool ForceGoLive
	{
		get => GDirectorServer.Instance.LiveCameraOverride == this;
		set
		{
			if (value)
			{
				GDirectorServer.Instance.LiveCameraOverride = this;
			}
			else if (!value && GDirectorServer.Instance.LiveCameraOverride == this)
			{
				GDirectorServer.Instance.LiveCameraOverride = null;
			}
		}
	}

	public void NotifyEnteredTree()
	{
		GDirectorServer.Instance.Register(this);
		this.Priority = this.BasePriority;
	}
	public void NotifyExitedTree()
	{
		GDirectorServer.Instance.Unregister(this);
	}
	public void NotifyProcessing()
	{
		double lastFramePriority = this.Priority;
		this.Priority = this.BasePriority;
		Callable.From(() => this.CheckPriorityChange(lastFramePriority)).CallDeferred();
	}

	private void CheckPriorityChange(double oldPriority)
	{
		if (this.Priority != oldPriority) {
			this.AsNode().EmitSignal(IVirtualCamera.SignalName_PriorityChanged, oldPriority);
		}
	}

	public Node AsNode() => (Node) this;
	public bool Is2D() => this.As2D() != null;
	public bool Is3D() => this.As3D() != null;

	public void NotifyIsLiveChanged(bool isLive) => this.AsNode().EmitSignal(SignalName_IsLiveChanged, isLive);
}
