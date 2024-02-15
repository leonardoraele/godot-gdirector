using System.Collections.Generic;
using System.Linq;
using Godot;
using Raele.GDirector.VirtualCameraControllers;

namespace Raele.GDirector;

public partial class GDirectorServer : Node
{
    private Camera3D RealCamera = null!;
	private VirtualCamera? CurrentActiveCamera;
	private VirtualCamera? PreviousActiveCamera;
    private HashSet<VirtualCamera> Cameras = new();

	public static void Register(VirtualCamera camera) => GetInstance(camera)._Register(camera);
	public static void Unregister(VirtualCamera camera) => GetInstance(camera)._Unregister(camera);
	public static GDirectorServer GetInstance(Node3D node) => GetInstance(node.GetTree());
	public static GDirectorServer GetInstance(SceneTree tree)
		=> tree.Root.GetNode<GDirectorServer>(nameof(GDirectorServer));

    public override void _Ready()
    {
        base._Ready();
		this.RealCamera = this.GetTree().Root.GetCamera3D();
		if (this.RealCamera == null) {
			this.RealCamera = new Camera3D();
			this.GetTree().Root.CallDeferred("add_child", this.RealCamera);
		}
    }

    private void _Register(VirtualCamera camera)
	{
		this.Cameras.Add(camera);
	}

	private void _Unregister(VirtualCamera camera)
	{
		this.Cameras.Remove(camera);
	}

    public override void _Process(double delta)
    {
        base._Process(delta);

		// Find the highest priority camera
		VirtualCamera? highestPriorityCamera = this.Cameras.Count == 0 ? null
			: this.Cameras.Count == 1 ? this.Cameras.First()
			: this.Cameras.Aggregate((a, b) => a.Priority > b.Priority ? a : b);

		// If the highest priority camera is not the current active camera, start the transition
		if (highestPriorityCamera != this.CurrentActiveCamera) {
			this.SetActiveCamera(highestPriorityCamera);
		}

		// Update the real camera position and rotation unless there is a state transition ongoing. In the latter case,
		// the transition controller will handle it, we don't need to do anything here.
		if (this.CurrentActiveCamera != null && this.GetActiveTransitionController()?.Ongoing != true) {
			this.RealCamera.GlobalPosition = this.CurrentActiveCamera.GlobalPosition;
			this.RealCamera.GlobalRotation = this.CurrentActiveCamera.GlobalRotation;
		}
    }

	public void SetActiveCamera(VirtualCamera? newActiveCamera)
	{
		(this.CurrentActiveCamera, this.PreviousActiveCamera) = (newActiveCamera, this.CurrentActiveCamera);
		TransitionController? transitionController = this.GetActiveTransitionController();
		if (transitionController != null && this.PreviousActiveCamera != null) {
			transitionController.StartTransition(this.RealCamera, this.PreviousActiveCamera);
		}
	}

	public TransitionController? GetActiveTransitionController()
	{
		if (this.CurrentActiveCamera == null) {
			return null;
		}
		if (
			this.PreviousActiveCamera != null
			&& this.CurrentActiveCamera?.TransitionControllers.TryGetValue(
					this.PreviousActiveCamera,
					out TransitionController? transitionController
				)
				== true
			|| (transitionController = this.CurrentActiveCamera?.DefaultTransitionController) != null
		) {
			return transitionController;
		}
		return null;
	}
}
