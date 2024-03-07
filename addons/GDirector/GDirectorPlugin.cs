#if TOOLS
using System;
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
        // Texture2D noIcon = ImageTexture.CreateFromImage(Image.Create(16, 16, false, Image.Format.L8));
        Texture2D vcamIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam.svg");
        Texture2D vcamPosIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam_pos_white.svg");
        Texture2D vcamRotIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam_rot_white.svg");
        Texture2D vcamPriIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam_pri_white.svg");
        Texture2D vcamTraIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam_tra_white.svg");

        Func<string, Script> GetControllerScript = (className) => GD.Load<Script>(
            $"res://addons/{nameof(GDirector)}/scripts/{nameof(VirtualCameraControllers)}/{className}.cs"
        );

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
            vcamIcon
        );

        // Position controllers
        foreach (
            string className
            in new string[] {
                nameof(FollowPosition),
                nameof(OrbitalMovement),
            }
        ) {
            this.AddCustomType($"{nameof(GDirector)}_{className}", nameof(Node), GetControllerScript(className), vcamPosIcon);
        }

        // Rotation controllers
        foreach (
            string className
            in new string[] {
                nameof(LookAtTarget),
                nameof(MimicRotation),
            }
        ) {
            this.AddCustomType($"{nameof(GDirector)}_{className}", nameof(Node), GetControllerScript(className), vcamRotIcon);
        }

        // Priority controllers
        foreach (
            string className
            in new string[] {
                nameof(FramingPriority),
                nameof(LineOfSightPriority),
                nameof(ProximityPriority),
            }
        ) {
            this.AddCustomType($"{nameof(GDirector)}_{className}", nameof(Node), GetControllerScript(className), vcamPriIcon);
        }

        // Transition controllers
        foreach (
            string className
            in new string[] {
                nameof(CameraTransition),
            }
        ) {
            this.AddCustomType($"{nameof(GDirector)}_{className}", nameof(Node), GetControllerScript(className), vcamTraIcon);
        }
    }
}
#endif
