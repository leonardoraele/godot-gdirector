#if TOOLS
using Godot;
using Raele.GDirector.VirtualCamera2DComponents;
using Raele.GDirector.VirtualCamera3DComponents;

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
		this.AddAutoloadSingleton(nameof(GDirectorServer), $"res://addons/{nameof(GDirector)}/src/{nameof(GDirectorServer)}.cs");

		// Custom Nodes
		this.AddCustomType(nameof(VirtualCamera2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2D)}.cs"), vcamIcon);
		this.AddCustomType(nameof(VirtualCamera3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3D)}.cs"), vcamIcon);

		// 2D Camera Components
		this.AddCustomType(nameof(VirtualCamera2DComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2DComponent)}.cs"), vcamIcon);
		this.AddCustomType(nameof(FramingComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera2DComponents)}/{nameof(FramingComponent2D)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(VirtualCamera2DComponents.TweenComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera2DComponents)}/{nameof(VirtualCamera2DComponents.TweenComponent)}.cs"), vcamTraIcon);

		// 3D Camera Components
		this.AddCustomType(nameof(VirtualCamera3DComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponent)}.cs"), vcamIcon);
		this.AddCustomType(nameof(FollowComponent3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera3DComponents)}/{nameof(FollowComponent3D)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(FramingComponent3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera3DComponents)}/{nameof(FramingComponent3D)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(FramingPriority3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera3DComponents)}/{nameof(FramingPriority3D)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(LineOfSightPriority3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera3DComponents)}/{nameof(LineOfSightPriority3D)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(LookAtTarget3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera3DComponents)}/{nameof(LookAtTarget3D)}.cs"), vcamRotIcon);
		this.AddCustomType(nameof(MimicMovement3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera3DComponents)}/{nameof(MimicMovement3D)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(MimicRotation3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera3DComponents)}/{nameof(MimicRotation3D)}.cs"), vcamRotIcon);
		this.AddCustomType(nameof(OrbitalMovement3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera3DComponents)}/{nameof(OrbitalMovement3D)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(ProximityPriority3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera3DComponents)}/{nameof(ProximityPriority3D)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(VirtualCamera3DComponents.TweenComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera3DComponents)}/{nameof(VirtualCamera3DComponents.TweenComponent)}.cs"), vcamTraIcon);

		// Camera Preview Window
		// this.SetupCameraPreviewWindow();
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		// Autoload Singleton
		this.RemoveAutoloadSingleton(nameof(GDirectorServer));

		// Custom Nodes
		this.RemoveCustomType(nameof(VirtualCamera2D));
		this.RemoveCustomType(nameof(VirtualCamera3D));

		// 2D Camera Components
		this.RemoveCustomType(nameof(VirtualCamera2DComponent));
		this.RemoveCustomType(nameof(FramingComponent2D));
		this.RemoveCustomType(nameof(VirtualCamera2DComponents.TweenComponent));

		// 3D Camera Components
		this.RemoveCustomType(nameof(VirtualCamera3DComponent));
		this.RemoveCustomType(nameof(FollowComponent3D));
		this.RemoveCustomType(nameof(FramingComponent3D));
		this.RemoveCustomType(nameof(FramingPriority3D));
		this.RemoveCustomType(nameof(LineOfSightPriority3D));
		this.RemoveCustomType(nameof(LookAtTarget3D));
		this.RemoveCustomType(nameof(MimicMovement3D));
		this.RemoveCustomType(nameof(MimicRotation3D));
		this.RemoveCustomType(nameof(OrbitalMovement3D));
		this.RemoveCustomType(nameof(ProximityPriority3D));
		this.RemoveCustomType(nameof(VirtualCamera3DComponents.TweenComponent));
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
