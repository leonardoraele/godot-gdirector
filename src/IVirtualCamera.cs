using System.Threading;
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
	// Blendable camera properties
	// -------------------------------------------------------------------------

	public Vector2 Position2D { get; set; }
	public Vector3 Position3D { get; set; }
	public float Rotation2D { get; set; }
	public Vector3 Rotation3D { get; set; }
	// public Transform2D Transform2D { get; set; }
	// public Transform3D Transform3D { get; set; }

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

	public void _EnterTree(CancellationToken token) => GDirectorServer.Instance.Register(this, token);
	public void _ExitTree() {}

	public void _Process()
	{
		if (Engine.IsEditorHint())
		{
			this.AsNode().SetProcess(false);
			return;
		}
		this.AsNode().CallDeferred(nameof(CheckPriorityChange), this.Priority);
		this.Priority = this.BasePriority;
	}

	private void CheckPriorityChange(double oldPriority)
	{
		if (this.Priority != oldPriority) {
			this.AsNode().EmitSignal(SignalName_PriorityChanged, this.Priority, oldPriority);
		}
	}

	public Node AsNode() => (Node) this;
	public bool Is2D() => this.As2D() != null;
	public bool Is3D() => this.As3D() != null;

	public void NotifyIsLiveChanged(bool isLive) => this.AsNode().EmitSignal(SignalName_IsLiveChanged, isLive);
}
