#if TOOLS
using Godot;
using Raele.GDirector.VirtualCameraControllers;

namespace Raele.GDirector;

[Tool]
public partial class GDirectorPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        Texture2D noIcon = ImageTexture.CreateFromImage(Image.Create(16, 16, false, Image.Format.L8));

		this.AddAutoloadSingleton(nameof(GDirectorServer), $"res://addons/{nameof(GDirector)}/scripts/{nameof(GDirectorServer)}.cs");

        this.AddCustomType(
            $"{nameof(GDirector)}_{nameof(VirtualCamera)}",
            nameof(Node3D),
            GD.Load<Script>($"res://addons/{nameof(GDirector)}/scripts/{nameof(VirtualCamera)}.cs"),
            noIcon
        );

        // Virtual Camera Controllers
        string[] states = new string[] {
            // Position controllers
			nameof(FollowPositionController),
			nameof(OrbitalPositionController),
			nameof(PathFollowPositionController),

			// Rotation controllers
			nameof(LookAtTargetController),
			nameof(MimicRotationController),

			// Priority controllers
			// ...

			// Transition controllers
			nameof(TransitionController),

			// Other controllers
			// ...
        };
        foreach (string stateName in states) {
            Script script = GD.Load<Script>($"res://addons/{nameof(GDirector)}/scripts/{nameof(VirtualCameraControllers)}/{stateName}.cs");
            this.AddCustomType($"{nameof(GDirector)}_{nameof(VirtualCameraController)}_{stateName}", nameof(Node), script, noIcon);
        }
    }
}
#endif
