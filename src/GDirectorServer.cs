using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.GDirector;

public partial class GDirectorServer : Node
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATIC STUFF
	// -----------------------------------------------------------------------------------------------------------------

	public static GDirectorServer Instance => field ??= Engine.GetMainLoop() is SceneTree tree
		? tree.Root.GetNode<GDirectorServer>(nameof(GDirectorServer))
		: throw new System.Exception("No GDirectorServer instance found in the scene tree. Make sure to add a GDirectorServer node to the scene tree.");

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

	public VirtualCamera? CurrentActiveCamera { get; private set; }
	public VirtualCamera? PreviousActiveCamera { get; private set; }

    private HashSet<VirtualCamera> ManagedCameras { get; init; } = new();
	private Camera3D OwnCamera { get; init; } = new();

    // -----------------------------------------------------------------------------------------------------------------
    // PROPERTIES
    // -----------------------------------------------------------------------------------------------------------------

    public Camera3D ManagedCamera => this.GetTree().Root.GetCamera3D() ?? this.OwnCamera;

    /// <summary>
    /// The active camera override. If this is set to a non-null value, this camera will be the active camera,
    /// regardless of its priority or camera group. Set this to null to make GDirector go back to its normal camera
	/// selection process, based on priority.
    /// </summary>
    public VirtualCamera? ActiveCameraOverride {
		get => field;
		set {
			if (field == value) {
				return;
			}
			field = value;
			this.ReevaluateCameraSelection();
		}
	}

	/// <summary>
	/// The active group of cameras. If this is set to a non-null value, only cameras in this group will be electible
	/// for activation. If this is set to null, all cameras will be electible.
	///
	/// When this property is set, if the current active camera is not in the newly set active group, the active camera
	/// will be changed to the best camera available in the active group.
	///
	/// Note that ActiveCameraOverride takes precedence over this property. If ActiveCameraOverride is set to a non-null
	/// value, the active camera will be the camera set in ActiveCameraOverride, regardless of the active group.
	/// </summary>
    public string? ActiveGroup {
		get => field;
		set {
			// If the active group is the same as the current active group, nothing changes, so we don't need to do
			// anything.
			if (field == value) {
				return;
			}

			// Update the active group.
			field = value;

			// If the currently active camera is already in the newly set active group, it's remains a valid active
			// camera, so we don't need to do anything else. Otherwise, we need to reevaluate the camera selection.
			if (
				string.IsNullOrEmpty(field)
				|| this.CurrentActiveCamera?.IsInGroup(field) != true
			) {
				this.ReevaluateCameraSelection();
			}
		}
	}

	public IEnumerable<VirtualCamera> ActiveGroupCameras
		=> string.IsNullOrEmpty(this.ActiveGroup)
			? this.ManagedCameras
			: this.ManagedCameras.Where(camera => camera.IsInGroup(this.ActiveGroup));

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

    public override void _Ready()
    {
        base._Ready();
		this.AddChild(this.OwnCamera);
		this.ReevaluateCameraSelection();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

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
		=> this.ActiveGroupCameras.ToArray() is VirtualCamera[] cameras && cameras.Length > 0
			? cameras.Aggregate((cameraA, cameraB) => cameraA.Priority > cameraB.Priority ? cameraA : cameraB)
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
