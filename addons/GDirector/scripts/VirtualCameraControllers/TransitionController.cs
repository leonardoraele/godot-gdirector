using Godot;

namespace Raele.GDirector.VirtualCameraControllers;

public partial class TransitionController : VirtualCameraController
{
	[Export] public VirtualCamera? FromCamera;
	[Export] public float DurationSec = 0.5f;
	[Export] public Curve? Curve;

	private float ProgressSec = 0;
	private VirtualCamera? PreviousCamera;
    private Camera3D? RealCamera;

    public bool Ongoing => this.ProgressSec < 1;
	private float ProgressPct => this.DurationSec > 0
		? Mathf.Clamp(this.ProgressSec / this.DurationSec, 0, 1)
		: 1;
	private float Weight => this.Curve?.Sample(this.ProgressPct) ?? this.ProgressPct;

    public override void _EnterTree()
	{
		base._EnterTree();
		if (this.FromCamera != null) {
			this.Camera.TransitionControllers[this.FromCamera] = this;
		} else {
			this.Camera.DefaultTransitionController = this;
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (!this.Ongoing || this.RealCamera == null || this.PreviousCamera == null) {
			return;
		}
		this.ProgressSec += (float) delta;
		this.RealCamera.GlobalPosition = this.PreviousCamera.GlobalPosition.Lerp(this.Camera.GlobalPosition, this.Weight);
		this.RealCamera.GlobalRotation = this.PreviousCamera.GlobalRotation.Lerp(this.Camera.GlobalRotation, this.Weight);
	}

    public void StartTransition(Camera3D realCamera, VirtualCamera previousCamera)
    {
        this.PreviousCamera = previousCamera;
		this.RealCamera = realCamera;
		this.ProgressSec = 0;
		this.RealCamera.GlobalPosition = this.PreviousCamera.GlobalPosition.Lerp(this.Camera.GlobalPosition, 0);
		this.RealCamera.GlobalRotation = this.PreviousCamera.GlobalRotation.Lerp(this.Camera.GlobalRotation, 0);
    }
}
