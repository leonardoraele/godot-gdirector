#if TOOLS
using System.Text.RegularExpressions;
using Godot;
using Raele.GDirector.VirtualCameraControllers;

namespace Raele.GDirector;

[Tool]
public partial class GDirectorPlugin : EditorPlugin
{
    [Signal] public delegate void SelectionChangedEventHandler(long cameraId);

    public override void _EnterTree()
    {
		this.AddAutoloadSingleton(nameof(GDirectorServer), $"res://addons/{nameof(GDirector)}/scripts/{nameof(GDirectorServer)}.cs");
        this.SetupCustomNodes();
        // this.SetupCameraPreviewWindow();
    }

    private void SetupCameraPreviewWindow()
    {
        CameraPreviewWindow preview = new CameraPreviewWindow();
        this.AddChild(preview);

        EditorSelection editorSelection = this.GetEditorInterface().GetSelection();
        editorSelection.SelectionChanged += () => {
            Node? selectedNode = editorSelection.GetSelectedNodes().Count > 0
                ? editorSelection.GetSelectedNodes()[0]
                : null;
            preview.SelectedCamera = Regex.IsMatch(
                    selectedNode?.GetScript().AsGodotObject()?.Get("resource_path").AsString() ?? "",
                    $"\\/{nameof(GDirector)}\\/scripts\\/{nameof(VirtualCamera)}\\.cs$"
                )
                ? selectedNode as Node3D
                : null;
        };
    }

    public void SetupCustomNodes()
    {
        Texture2D noIcon = ImageTexture.CreateFromImage(Image.Create(16, 16, false, Image.Format.L8));

        // this.AddCustomType(
        //     $"{nameof(GDirector)}_{nameof(GDirectorServer)}",
        //     nameof(Node),
        //     GD.Load<Script>($"res://addons/{nameof(GDirector)}/scripts/{nameof(GDirectorServer)}.cs"),
        //     noIcon
        // );

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
