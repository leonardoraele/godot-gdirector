using Godot;

namespace Raele.GDirector.VirtualCameraControllers;

public partial class PathFollowPositionController : VirtualCameraController
{
	[ExportGroup("Automatic Movement")]
	[Export] public float LapDurationSec = 0;
	[Export] public float ProgressUnPSec = 0;

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
		if (this.LapDurationSec > Mathf.Epsilon) {
			this.PathFollow.ProgressRatio += (float) (delta / this.LapDurationSec);
		}
		this.Camera.Position = Vector3.Zero;
	}
}
