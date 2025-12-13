using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Raele.GDirector;

[Tool]
public partial class GDirectorServer : Node
{
	// -----------------------------------------------------------------------------------------------------------------
	// STATIC STUFF
	// -----------------------------------------------------------------------------------------------------------------

	public static GDirectorServer Instance => field ??= Engine.GetMainLoop() is SceneTree tree
		? tree.Root.GetNode<GDirectorServer>(nameof(GDirectorServer))
		: throw new System.Exception($"Autoload singleton {nameof(GDirectorServer)} not found. Make sure it is enabled in Project Settings → Globals.");

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void LiveCameraChangedEventHandler(IVirtualCamera.Wrapper newCamera, IVirtualCamera.Wrapper oldCamera);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public IVirtualCamera? CurrentLiveCamera { get; private set; } = null;
	private WeakReference<IVirtualCamera>? PreviousLiveCameraWeakRef = null;
	// private IVirtualCamera? SecondHighestPriorityCamera = null; // TODO

	private HashSet<IVirtualCamera> ManagedVirtualCameras { get; init; } = new();

	/// <summary>
	/// The active camera override. If this is set to a non-null value, this camera will be the active camera,
	/// regardless of its priority or camera group. Set this to null to make GDirector go back to its normal camera
	/// selection process, based on priority.
	/// </summary>
	public IVirtualCamera? LiveCameraOverride {
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
				|| this.CurrentLiveCamera?.AsNode().IsInGroup(field) != true
			) {
				this.ReevaluateCameraSelection();
			}
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// COMPUTED PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	public IVirtualCamera? PreviousLiveCamera
	{
		get => this.PreviousLiveCameraWeakRef?.TryGetTarget(out IVirtualCamera? cam) ?? false ? cam : null;
		set => this.PreviousLiveCameraWeakRef = value != null ? new(value) : null;
	}

	public Camera2D? GodotCamera2D => this.GetTree().Root.GetCamera2D();
	public Camera3D? GodotCamera3D => this.GetTree().Root.GetCamera3D();

	public IEnumerable<IVirtualCamera> ActiveGroupCameras
		=> string.IsNullOrEmpty(this.ActiveGroup)
			? this.ManagedVirtualCameras
			: this.ManagedVirtualCameras.Where(camera => camera.AsNode().IsInGroup(this.ActiveGroup));

	public Vector2 ScreenSize
		=> Engine.IsEditorHint()
			? new Vector2(
				ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt32(),
				ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt32()
			)
			: this.GetViewport().GetVisibleRect().Size;

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint())
		{
			return;
		}
		this.ReevaluateCameraSelection();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public void Register(IVirtualCamera camera/*, CancellationToken token*/)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		this.ManagedVirtualCameras.Add(camera);
		camera.AsNode().Connect(
			IVirtualCamera.SignalName_PriorityChanged,
			Callable.From((double oldPriority) => this.EvaluateCameraPriority(camera, oldPriority))
		);
		if (this.IsNodeReady()) {
			this.EvaluateCameraPriority(camera);
		}
	}

	public void Unregister(IVirtualCamera camera)
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		this.ManagedVirtualCameras.Remove(camera);
		if (camera == this.LiveCameraOverride) {
			this.LiveCameraOverride = null;
		} else if (camera == this.CurrentLiveCamera) {
			this.ReevaluateCameraSelection();
		}
	}

	private void EvaluateCameraPriority(IVirtualCamera camera, double oldPriority = double.NegativeInfinity)
	{
		// TODO We should not reevaluate all cameras every time the current camera reduces priority. Instead, we should
		// keep a reference to the next best camera.
		if (camera == this.CurrentLiveCamera && camera.Priority < oldPriority - Mathf.Epsilon)
		{
			this.ReevaluateCameraSelection();
			return;
		}
		if (
			this.LiveCameraOverride != null
			|| camera.Priority <= (this.CurrentLiveCamera?.Priority ?? float.NegativeInfinity)
			|| !string.IsNullOrEmpty(this.ActiveGroup) && !camera.AsNode().IsInGroup(this.ActiveGroup)
		) {
			return;
		}
		this.SetCameraLive(camera);
	}

	private void ReevaluateCameraSelection()
	{
		if (Engine.IsEditorHint())
		{
			return;
		}
		if (this.LiveCameraOverride != null) {
			this.SetCameraLive(this.LiveCameraOverride);
			return;
		}
		this.SetCameraLive(this.FindHighestPriorityCameraInActiveGroup());
	}

	private IVirtualCamera? FindHighestPriorityCameraInActiveGroup()
		=> this.ActiveGroupCameras.ToArray() is IVirtualCamera[] cameras && cameras.Length > 0
			? cameras.Aggregate((cameraA, cameraB) => cameraA.Priority > cameraB.Priority ? cameraA : cameraB)
			: null;

	private void SetCameraLive(IVirtualCamera? camera)
	{
		// If the new active camera is the same as the current active camera, there's no need to do anything.
		if (camera == this.CurrentLiveCamera) {
			return;
		}

		(this.CurrentLiveCamera, this.PreviousLiveCamera) = (camera, this.CurrentLiveCamera);

		this.PreviousLiveCamera?.NotifyIsLiveChanged(isLive: false);
		this.CurrentLiveCamera?.NotifyIsLiveChanged(isLive: true);
		this.EmitSignalLiveCameraChanged(new(this.CurrentLiveCamera), new(this.PreviousLiveCamera));
	}
}
