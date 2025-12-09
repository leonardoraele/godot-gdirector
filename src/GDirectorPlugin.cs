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
		this.AddCustomType(nameof(VirtualCamera), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera)}.cs"), vcamIcon);

		this.AddCustomType(nameof(FollowPosition), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(FollowPosition)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(OrbitalMovement), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(OrbitalMovement)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(MimicMovement), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(MimicMovement)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(FramingConstraint), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(FramingConstraint)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(LookAtTarget), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(LookAtTarget)}.cs"), vcamRotIcon);
		this.AddCustomType(nameof(MimicRotation), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(MimicRotation)}.cs"), vcamRotIcon);
		this.AddCustomType(nameof(FramingPriority), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(FramingPriority)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(LineOfSightPriority), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(LineOfSightPriority)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(ProximityPriority), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(ProximityPriority)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(CameraTransition), nameof(Node), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCameraComponents)}/{nameof(CameraTransition)}.cs"), vcamTraIcon);

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
