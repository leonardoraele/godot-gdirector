using Godot;

public partial class Debugger : Node
{
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("debug_pause")) {
			this.ProcessMode = ProcessModeEnum.Always;
			this.GetTree().Paused = !this.GetTree().Paused;
		}
	}
}
