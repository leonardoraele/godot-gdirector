#if TOOLS
using Godot;
using Raele.GDirector.Editor;
using Raele.GDirector.VirtualCamera2DComponents;
using Raele.GDirector.VirtualCamera3DComponents;

namespace Raele.GDirector;

[Tool]
public partial class GDirectorPlugin : EditorPlugin
{
	private VCam2DAreaConstraintComponent? AreaEditTarget = null;
	private AreaConstraintEditor AreaEditor = new();
	public override bool _Handles(GodotObject @object) => @object is VCam2DAreaConstraintComponent;
	public override void _Edit(GodotObject @object)
	{
		base._Edit(@object);
		this.AreaEditor.EditTarget = this.AreaEditTarget = @object as VCam2DAreaConstraintComponent;
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
		Texture2D cam_2d_area_constraint = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/{nameof(cam_2d_area_constraint)}.png");
		Texture2D cam_2d_framing = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/{nameof(cam_2d_framing)}.png");
		Texture2D cam_2d_tween = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/{nameof(cam_2d_tween)}.png");
		Texture2D cam_2d_priority = GD.Load<Texture2D>($"res://addons/{nameof(GDirector)}/icons/{nameof(cam_2d_priority)}.png");

		// Autoload Singleton
		this.AddAutoloadSingleton(nameof(GDirectorServer), $"res://addons/{nameof(GDirector)}/src/{nameof(GDirectorServer)}.cs");

		// Custom Nodes
		this.AddCustomType(nameof(VirtualCamera2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2D)}.cs"), vcam2d);
		this.AddCustomType(nameof(VirtualCamera3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3D)}.cs"), vcamIcon);

		// 2D Camera Components
		this.AddCustomType(nameof(VCam2DAreaConstraintComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2DComponents)}/{nameof(VCam2DAreaConstraintComponent)}.cs"), cam_2d_area_constraint);
		this.AddCustomType(nameof(VCam2DFramingComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2DComponents)}/{nameof(VCam2DFramingComponent)}.cs"), cam_2d_framing);
		this.AddCustomType(nameof(VCam2DObjectDetectionComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2DComponents)}/{nameof(VCam2DObjectDetectionComponent)}.cs"), cam_2d_priority);
		this.AddCustomType(nameof(VCam2DTweenComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera2DComponents)}/{nameof(VCam2DTweenComponent)}.cs"), cam_2d_tween);

		// 3D Camera Components
		this.AddCustomType(nameof(VCam3DFollowComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(VCam3DFollowComponent)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(VCam3DFramingComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(VCam3DFramingComponent)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(VCam3DFramingPriorityComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(VCam3DFramingPriorityComponent)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(VCam3DLineOfSightPriorityComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(VCam3DLineOfSightPriorityComponent)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(VCam3DLookAtTargetComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(VCam3DLookAtTargetComponent)}.cs"), vcamRotIcon);
		this.AddCustomType(nameof(VCam3DMimicMovementComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(VCam3DMimicMovementComponent)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(VCam3DMimicRotationComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(VCam3DMimicRotationComponent)}.cs"), vcamRotIcon);
		this.AddCustomType(nameof(VCam3DOrbitComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(VCam3DOrbitComponent)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(VCam3DProximityPriorityComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(VCam3DProximityPriorityComponent)}.cs"), vcamPriIcon);
		this.AddCustomType(nameof(VCam3DTweenComponent), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(GDirector)}/src/{nameof(VirtualCamera3DComponents)}/{nameof(VCam3DTweenComponent)}.cs"), vcamTraIcon);

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
		this.RemoveCustomType(nameof(VCam2DAreaConstraintComponent));
		this.RemoveCustomType(nameof(VCam2DObjectDetectionComponent));
		this.RemoveCustomType(nameof(VCam2DFramingComponent));
		this.RemoveCustomType(nameof(VCam2DTweenComponent));

		// 3D Camera Components
		this.RemoveCustomType(nameof(VCam3DFollowComponent));
		this.RemoveCustomType(nameof(VCam3DFramingComponent));
		this.RemoveCustomType(nameof(VCam3DFramingPriorityComponent));
		this.RemoveCustomType(nameof(VCam3DLineOfSightPriorityComponent));
		this.RemoveCustomType(nameof(VCam3DLookAtTargetComponent));
		this.RemoveCustomType(nameof(VCam3DMimicMovementComponent));
		this.RemoveCustomType(nameof(VCam3DMimicRotationComponent));
		this.RemoveCustomType(nameof(VCam3DOrbitComponent));
		this.RemoveCustomType(nameof(VCam3DProximityPriorityComponent));
		this.RemoveCustomType(nameof(VCam3DTweenComponent));
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
