using System.Collections.Generic;
using System.Linq;
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

	// -------------------------------------------------------------------------
	// Blendable camera properties
	// -------------------------------------------------------------------------

	public Vector2 Position2D { get; set; }
	public Vector3 Position3D { get; set; }
	public float Rotation2D { get; set; }
	public Vector3 Rotation3D { get; set; }
	public Vector2 Offset { get; set; }
	public Vector2 Zoom { get; set; }

	// -------------------------------------------------------------------------
	// GDirector properties
	// -------------------------------------------------------------------------

	protected double BasePriority { get; }
	public double Priority { get; set; }
	public HashSet<BlendComponent> Blenders { get; }

	// // -------------------------------------------------------------------------
	// // Godot group methods
	// // -------------------------------------------------------------------------

	// public bool IsInGroup(StringName groupName);
	// public void AddToGroup(StringName groupName);
	// public void RemoveFromGroup(StringName groupName);

	// // -------------------------------------------------------------------------
	// // Godot signal methods
	// // -------------------------------------------------------------------------

	// public Error Connect(StringName signal, Callable callable, uint flags = 0);
	// public void Disconnect(StringName signal, Callable callable);
	// public bool IsConnected(StringName signal, Callable callable);
	// public bool HasConnections(StringName signal);
	// protected Error EmitSignal(StringName signal, params Variant[] args);

	// -------------------------------------------------------------------------
	// Utility properties & methods
	// -------------------------------------------------------------------------

	public bool IsLive => GDirectorServer.Instance.LiveCamera == this;
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

	public Node AsNode() => (Node) this;

	public void Connect(StringName signal, Callable callable, params GodotObject.ConnectFlags[] flags)
		=> this.AsNode()
			.Connect(signal, callable, flags.Select(flag => (uint) flag).Aggregate((a, b) => a | b));

	public void ConnectSignalPriorityChanged(Callable callable, params GodotObject.ConnectFlags[] flags)
		=> this.Connect(SignalName_PriorityChanged, callable, flags);
	public void DisconnectSignalPriorityChanged(Callable callable)
		=> this.AsNode().Disconnect(SignalName_PriorityChanged, callable);
	public void IsSignalPriorityChangedConnected(Callable callable)
		=> this.AsNode().IsConnected(SignalName_PriorityChanged, callable);
	public bool HasSignalPriorityChangedConnections()
		=> this.AsNode().HasConnections(SignalName_PriorityChanged);
	protected void EmitSignalPriorityChanged(double newPriority, double oldPriority)
		=> this.AsNode().EmitSignal(SignalName_PriorityChanged, [newPriority, oldPriority]);
	public void ConnectSignalIsLiveChanged(Callable callable, params GodotObject.ConnectFlags[] flags)
		=> this.Connect(SignalName_IsLiveChanged, callable, flags);
	public void DisconnectSignalIsLiveChanged(Callable callable)
		=> this.AsNode().Disconnect(SignalName_IsLiveChanged, callable);
	public void IsSignalIsLiveChangedConnected(Callable callable)
		=> this.AsNode().IsConnected(SignalName_IsLiveChanged, callable);
	public bool HasSignalIsLiveChangedConnections() => this.AsNode().HasConnections(SignalName_IsLiveChanged);
	protected void EmitSignalIsLiveChanged(bool isLive)
		=> this.AsNode().EmitSignal(SignalName_IsLiveChanged, [isLive]);

	// -------------------------------------------------------------------------
	// Godot lifecycle methods
	// -------------------------------------------------------------------------

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
			this.EmitSignalPriorityChanged(this.Priority, oldPriority);
		}
	}
}
