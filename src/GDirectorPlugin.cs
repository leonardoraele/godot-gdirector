#if TOOLS
using Godot;
using Raele.GDirector.Editor;
using Raele.GDirector.VirtualCamera2DComponents;
using Raele.GDirector.VirtualCamera3DComponents;

namespace Raele.GDirector;

[Tool]
public partial class GDirectorPlugin : EditorPlugin
{
	private ConfinementComponent? AreaEditTarget = null;
	private ConfiningRegionEditor AreaEditor = new();
	public override bool _Handles(GodotObject @object) => @object is ConfinementComponent;
	public override void _Edit(GodotObject @object)
	{
		base._Edit(@object);
		this.AreaEditor.EditTarget = this.AreaEditTarget = @object as ConfinementComponent;
		this.AreaEditor.UndoRedo = this.GetUndoRedo();
	}
	public override void _ForwardCanvasDrawOverViewport(Control viewportControl)
	{
		base._ForwardCanvasDrawOverViewport(viewportControl);
		if (this.AreaEditor?.GetParent() != viewportControl)
		{
			viewportControl.AddChild(this.AreaEditor);
		}
	}

	public override void _EnterTree()
	{
		// Texture2D noIcon = ImageTexture.CreateFromImage(Image.Create(16, 16, false, Image.Format.L8));
		Texture2D cam_bg = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/{nameof(cam_bg)}.png");
		Texture2D vcamIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam.svg");
		Texture2D vcamPosIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam_pos_white.svg");
		Texture2D vcamRotIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam_rot_white.svg");
		Texture2D vcamPriIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam_pri_white.svg");
		Texture2D vcamTraIcon = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/vcam_tra_white.svg");

		Texture2D vcam2d = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/{nameof(vcam2d)}.png");
		Texture2D cam_2d_confinement = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/{nameof(cam_2d_confinement)}.png");
		Texture2D cam_2d_framing = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/{nameof(cam_2d_framing)}.png");
		Texture2D cam_2d_tween = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/{nameof(cam_2d_tween)}.png");
		Texture2D cam_2d_priority = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/{nameof(cam_2d_priority)}.png");

		// Autoload Singleton
		this.AddAutoloadSingleton(nameof(GDirectorServer), $"res://addons/{nameof(GDirector)}/src/{nameof(GDirectorServer)}.cs");

		// Custom Nodes
		this.AddCustomType(nameof(VirtualCamera2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2D)}.cs"), vcam2d);
		this.AddCustomType(nameof(VirtualCamera3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3D)}.cs"), vcamIcon);

		// 2D Camera Components
		this.AddCustomType(nameof(VirtualCamera2DComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2DComponent)}.cs"), cam_bg);
		this.AddCustomType(nameof(ConfinementComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2DComponents)}/{nameof(ConfinementComponent)}.cs"), cam_2d_confinement);
		this.AddCustomType(nameof(VirtualCamera2DComponents.FramingComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2DComponents)}/{nameof(VirtualCamera2DComponents.FramingComponent)}.cs"), cam_2d_framing);
		this.AddCustomType(nameof(ObjectDetectionComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2DComponents)}/{nameof(ObjectDetectionComponent)}.cs"), cam_2d_priority);
		this.AddCustomType(nameof(VirtualCamera2DComponents.TweenComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2DComponents)}/{nameof(VirtualCamera2DComponents.TweenComponent)}.cs"), cam_2d_tween);

		// 3D Camera Components
		this.AddCustomType(nameof(FollowComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(FollowComponent)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(VirtualCamera3DComponents.FramingComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(VirtualCamera3DComponents.FramingComponent)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(FramingPriorityComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(FramingPriorityComponent)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(LineOfSightPriorityComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(LineOfSightPriorityComponent)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(LookAtTargetComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(LookAtTargetComponent)}.cs"), vcamRotIcon);
		this.AddCustomType(nameof(MimicMovementComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(MimicMovementComponent)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(MimicRotationComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(MimicRotationComponent)}.cs"), vcamRotIcon);
		this.AddCustomType(nameof(OrbitComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(OrbitComponent)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(ProximityPriorityComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(ProximityPriorityComponent)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(VirtualCamera3DComponents.TweenComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(VirtualCamera3DComponents.TweenComponent)}.cs"), vcamTraIcon);

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
		this.RemoveCustomType(nameof(ConfinementComponent));
		this.RemoveCustomType(nameof(ObjectDetectionComponent));
		this.RemoveCustomType(nameof(VirtualCamera2DComponents.FramingComponent));
		this.RemoveCustomType(nameof(VirtualCamera2DComponents.TweenComponent));

		// 3D Camera Components
		this.RemoveCustomType(nameof(FollowComponent));
		this.RemoveCustomType(nameof(VirtualCamera3DComponents.FramingComponent));
		this.RemoveCustomType(nameof(FramingPriorityComponent));
		this.RemoveCustomType(nameof(LineOfSightPriorityComponent));
		this.RemoveCustomType(nameof(LookAtTargetComponent));
		this.RemoveCustomType(nameof(MimicMovementComponent));
		this.RemoveCustomType(nameof(MimicRotationComponent));
		this.RemoveCustomType(nameof(OrbitComponent));
		this.RemoveCustomType(nameof(ProximityPriorityComponent));
		this.RemoveCustomType(nameof(VirtualCamera3DComponents.TweenComponent));
	}

	// TODO Alternatively, maybe override _Handles(), _MakeVisible(), and/or _Edit() to implement camera preview
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
