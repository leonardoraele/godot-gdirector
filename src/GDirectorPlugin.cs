#if TOOLS
using Godot;
using Raele.GDirector.VirtualCameraComponents;

namespace Raele.GDirector;

[Tool]
public partial class GDirectorPlugin : EditorPlugin
{
	[Signal] public delegate void SelectionChangedEventHandler(long cameraId);

	public override void _EnterTree()
	{
		// Texture2D noIcon = ImageTexture.CreateFromImage(Image.Create(16, 16, false, Image.Format.L8));
		Texture2D vcamIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam.svg");
		Texture2D vcamPosIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam_pos_white.svg");
		Texture2D vcamRotIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam_rot_white.svg");
		Texture2D vcamPriIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam_pri_white.svg");
		Texture2D vcamTraIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam_tra_white.svg");

		// Autoload Singleton
		this.AddAutoloadSingleton(nameof(GDirectorServer), $"res://addons/{nameof(GDirector)}/scripts/{nameof(GDirectorServer)}.cs");

		// Custom Nodes
		this.AddCustomType(nameof(VirtualCamera3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3D)}.cs"), vcamIcon);

		this.AddCustomType(nameof(FollowPosition3D), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(FollowPosition3D)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(OrbitalMovement3D), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(OrbitalMovement3D)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(MimicMovement3D), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(MimicMovement3D)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(FramingConstraint3D), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(FramingConstraint3D)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(LookAtTarget3D), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(LookAtTarget3D)}.cs"), vcamRotIcon);
		this.AddCustomType(nameof(MimicRotation3D), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(MimicRotation3D)}.cs"), vcamRotIcon);
		this.AddCustomType(nameof(FramingPriority3D), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(FramingPriority3D)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(LineOfSightPriority3D), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(LineOfSightPriority3D)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(ProximityPriority3D), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(ProximityPriority3D)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(CameraTransition3D), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(CameraTransition3D)}.cs"), vcamTraIcon);

		// Camera Preview Window
		// this.SetupCameraPreviewWindow();
	}

	// private void SetupCameraPreviewWindow()
	// {
	//	 CameraPreviewWindow preview = new CameraPreviewWindow();
	//	 this.AddChild(preview);

	//	 EditorSelection editorSelection = EditorInterface.Singleton.GetSelection();
	//	 editorSelection.SelectionChanged += () => {
	//		 Node? selectedNode = editorSelection.GetSelectedNodes().Count > 0
	//			 ? editorSelection.GetSelectedNodes()[0]
	//			 : null;
	//		 preview.SelectedCamera = Regex.IsMatch(
	//				 selectedNode?.GetScript().AsGodotObject()?.Get("resource_path").AsString() ?? "",
	//				 $"\\/{nameof(GDirector)}\\/scripts\\/{nameof(VirtualCamera)}\\.cs$"
	//			 )
	//			 ? selectedNode as Node3D
	//			 : null;
	//	 };
	// }
}
#endif
