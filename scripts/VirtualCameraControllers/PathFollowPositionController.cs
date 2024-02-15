using Godot;

namespace Raele.GDirector.VirtualCameraControllers;

public partial class PathFollowPositionController : VirtualCameraController
{
	[ExportGroup("Automatic Movement")]
	[Export] public float Duration = 0;
	[Export] public float ProgressUnPSec = 4;

	private PathFollow3D PathFollow = null!;

	public override void _EnterTree()
	{
		base._EnterTree();
		if (this.Camera.GetParent() is not PathFollow3D pathFollow) {
			GD.PushError("A " + nameof(VirtualCamera) + " node with a " + nameof(PathFollowPositionController) + " must be child of a " + nameof(PathFollow3D) + " node.");
			this.QueueFree();
			return;
		}
		this.PathFollow = pathFollow;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		this.PathFollow.Progress += (float) (this.ProgressUnPSec * delta);
		if (this.Duration > Mathf.Epsilon) {
			this.PathFollow.ProgressRatio += (float) (delta / this.Duration);
		}
		this.Camera.Position = Vector3.Zero;
	}
}
