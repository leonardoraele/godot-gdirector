using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.GDirector;

public partial class GDirectorServer : Node
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATIC STUFF
	// -----------------------------------------------------------------------------------------------------------------

	public static GDirectorServer Instance { get; private set; } = null!;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void TransitionStartEventHandler(ulong nextCameraId, ulong previousCameraId, ulong controllerId);
	[Signal] public delegate void TransitionFinishEventHandler(ulong nextCameraId, ulong previousCameraId);
	[Signal] public delegate void TransitionCancelEventHandler(ulong nextCameraId, ulong previousCameraId);
	[Signal] public delegate void TransitionEndEventHandler(ulong nextCameraId, ulong previousCameraId);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

    public Camera3D ManagedCamera { get; private set; } = null!;
	public VirtualCamera? CurrentActiveCamera { get; private set; }
	public VirtualCamera? PreviousActiveCamera { get; private set; }
	private VirtualCamera? _activeCameraOverride;
    private HashSet<VirtualCamera> ManagedCameras = new();
    private string? _activeGroup;

    // -----------------------------------------------------------------------------------------------------------------
    // PROPERTIES
    // -----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// The active camera override. If this is set to a non-null value, this camera will be the active camera,
    /// regardless of its priority or group. Set this to null to make GDirector go back to its normal camera selection
    /// process, based on camera priority.
    /// </summary>
    public VirtualCamera? ActiveCameraOverride {
		get => this._activeCameraOverride;
		set {
			if (this._activeCameraOverride == value) {
				return;
			}
			this._activeCameraOverride = value;
			this.ReevaluateCameraSelection();
		}
	}

	/// <summary>
	/// The active group of cameras. If this is set to a non-null value, only cameras in this group will be electible
	/// for activation. If this is set to null, all cameras will be electible.
	///
	/// If this is set to a group that doesn't contain the active camera, the active camera will be changed to the next
	/// best camera available.
	///
	/// Note that ActiveCameraOverride takes precedence over this property. If ActiveCameraOverride is set to a non-null
	/// value, the active camera will be the camera set in ActiveCameraOverride, regardless of the active group.
	/// </summary>
    public string? ActiveGroup {
		get => this._activeGroup;
		set {
			// If the active group is the same as the current active group, nothing changes, so we don't need to do
			// anything.
			if (this._activeGroup == value) {
				return;
			}

			// Update the active group.
			this._activeGroup = value;

			// If the currently active camera is already in the newly set active group, it's remains a valid active
			// camera, so we don't need to do anything else. Otherwise, we need to reevaluate the camera selection.
			if (
				string.IsNullOrEmpty(this._activeGroup)
				|| this.CurrentActiveCamera?.IsInGroup(this._activeGroup) != true
			) {
				this.ReevaluateCameraSelection();
			}
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

    public override void _EnterTree()
    {
        base._EnterTree();
		if (GDirectorServer.Instance != null) {
			GD.PushError("There can only be one " + nameof(GDirectorServer) + " in the scene.");
			this.QueueFree();
			return;
		}
		GDirectorServer.Instance = this;
    }

    public override void _Ready()
    {
        base._Ready();
		this.ManagedCamera = this.GetTree().Root.GetCamera3D();
		if (this.ManagedCamera == null) {
			this.ManagedCamera = new Camera3D();
			this.AddChild(this.ManagedCamera);
		}
		this.ReevaluateCameraSelection();
	}

    public void Register(VirtualCamera camera)
	{
		this.ManagedCameras.Add(camera);
		this.EvaluateCameraPriority(camera);
		camera.PriorityChange += (_newPriority, _oldPriority) => this.EvaluateCameraPriority(camera);
		camera.ExitTransitionStart += (newCamera, controllerId) => this.EmitSignal(SignalName.TransitionStart, newCamera, camera, controllerId);
		camera.ExitTransitionFinish += (newCamera) => this.EmitSignal(SignalName.TransitionFinish, newCamera, camera);
		camera.ExitTransitionCancel += (newCamera) => this.EmitSignal(SignalName.TransitionCancel, newCamera, camera);
		camera.ExitTransitionEnd += (newCamera) => this.EmitSignal(SignalName.TransitionEnd, newCamera, camera);
	}

	public void Unregister(VirtualCamera camera)
	{
		this.ManagedCameras.Remove(camera);
		if (camera == this.ActiveCameraOverride) {
			this.ActiveCameraOverride = null;
		} else if (camera == this.CurrentActiveCamera) {
			this.ReevaluateCameraSelection();
		}
	}

	private void EvaluateCameraPriority(VirtualCamera camera)
	{
        if (
			this.ActiveCameraOverride == null
			&& camera.Priority > (this.CurrentActiveCamera?.Priority ?? float.NegativeInfinity)
			&& (string.IsNullOrEmpty(this.ActiveGroup) || camera.IsInGroup(this.ActiveGroup))
		) {
			this.SetActiveCamera(camera);
		}
	}

    private void ReevaluateCameraSelection()
    {
        if (this.ActiveCameraOverride != null) {
			this.SetActiveCamera(this.ActiveCameraOverride);
			return;
		}
		this.SetActiveCamera(this.FindHighestPriorityCameraInActiveGroup());
    }

	private VirtualCamera? FindHighestPriorityCameraInActiveGroup()
		=> this.ManagedCameras.Count > 0
			? this.ManagedCameras.Where(camera =>
					string.IsNullOrEmpty(this.ActiveGroup)
					|| camera.IsInGroup(this.ActiveGroup)
				)
				.Aggregate(
					this.ManagedCameras.First(),
					(cameraA, cameraB) => cameraA.Priority > cameraB.Priority ? cameraA : cameraB
				)
			: null;

	private void SetActiveCamera(VirtualCamera? newActiveCamera)
	{
		// Won't change the active camera if the GDirectorServer is not ready because we need to access the
		// ManagedCamera, which is set on the _Ready method.
		// TODO Instead of just returning, we could queue the new active camera to be set when the GDirectorServer is
		// ready so that we don't need to reevaluate all the cameras when the GDirectorServer becomes ready.
		if (!this.IsNodeReady()) {
			return;
		}

		// If the new active camera is the same as the current active camera, there's no need to do anything.
		if (newActiveCamera == this.CurrentActiveCamera) {
			return;
		}

		this.CurrentActiveCamera?.CancelTransition();
		(this.CurrentActiveCamera, this.PreviousActiveCamera) = (newActiveCamera, this.CurrentActiveCamera);
		this.CurrentActiveCamera?.StartTransition();
	}
}
