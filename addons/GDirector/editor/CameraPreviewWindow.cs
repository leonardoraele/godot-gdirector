#nullable enable
using Godot;

public partial class CameraPreviewWindow : Window
{
    public Node3D? SelectedCamera;

	private Camera3D Camera = new();
    private bool Dragging;

    public override void _Ready()
	{
		this.AddChild(this.Camera);
		// TODO
		// this.MouseEntered += () => Input.SetCustomMouseCursor();
		// this.MouseExited += () => Input.SetDefaultMouseCursor();
	}

	public override void _Process(double delta)
	{
		this.Visible = this.SelectedCamera != null;
		if (this.SelectedCamera == null) {
			return;
		}

		this.Camera.GlobalPosition = this.SelectedCamera.GlobalPosition;
		this.Camera.GlobalRotation = this.SelectedCamera.GlobalRotation;

		if (this.Dragging) {
			Vector2 mouseMovement = Input.GetLastMouseVelocity();
			this.Position = this.Position + new Vector2I((int) mouseMovement.X, (int) mouseMovement.Y);
		}
	}

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
		if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.ButtonIndex == MouseButton.Left) {
			this.Dragging = eventMouseButton.Pressed;
		}
	}
}
