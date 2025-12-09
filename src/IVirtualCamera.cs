using System;
using System.Threading;
using Godot;

namespace Raele.GDirector;

public interface IVirtualCamera
{
	public double Priority { get; }
	public bool IsInGroup(string groupName);
	public void StartTransition();
	public void CancelTransition();
	public void ConnectPriorityChanged(
		Action<double, double> callback,
		CancellationToken? cancellationToken = null,
		params GodotObject.ConnectFlags[] flags
	);
	// public event Action<VirtualCamera, long> ExitTransitionStart;
	// public event Action<VirtualCamera> ExitTransitionFinish;
	// public event Action<VirtualCamera> ExitTransitionCancel;
	// public event Action<VirtualCamera> ExitTransitionEnd;
}
