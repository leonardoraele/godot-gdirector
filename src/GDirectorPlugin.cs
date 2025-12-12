#if TOOLS
using System;
using Godot;
using Raele.GDirector.VirtualCamera2DComponents;
using Raele.GDirector.VirtualCamera3DComponents;

namespace Raele.GDirector;

[Tool]
public partial class GDirectorPlugin : EditorPlugin
{
	private int CurrentActiveControlPointIndex = -1;
	private AreaConstraintComponent2D? areaEditTarget = null;
	public override bool _Handles(GodotObject @object) => @object is AreaConstraintComponent2D;
	public override void _Edit(GodotObject @object)
	{
		base._Edit(@object);
		this.areaEditTarget = @object as AreaConstraintComponent2D;
	}
	public override void _ForwardCanvasDrawOverViewport(Control viewportControl)
	{
		base._ForwardCanvasDrawOverViewport(viewportControl);
		this.areaEditTarget?.DrawOverViewport(viewportControl);
	}
	public override bool _ForwardCanvasGuiInput(InputEvent @event)
	{
		if (this.areaEditTarget == null)
		{
			return base._ForwardCanvasGuiInput(@event);
		}
		Vector2[] controlPoints = this.areaEditTarget.ControlPoints;
		float minControlDistanceSquared = 900f;
		Vector2 mousePos = this.areaEditTarget.GetGlobalMousePosition();
		if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
		{
			this.CurrentActiveControlPointIndex = -1;
			if (mouseButton.Pressed)
			{
				for (int i = 0; i < controlPoints.Length; i++)
				{
					if (mousePos.DistanceSquaredTo(controlPoints[i]) <= minControlDistanceSquared)
					{
						this.CurrentActiveControlPointIndex = i;
						return true;
					}
				}
			}
		}
		else if (@event is InputEventMouseMotion mouseMotion)
		{
			if (this.CurrentActiveControlPointIndex != -1)
			{
				switch (this.CurrentActiveControlPointIndex)
				{
					case 0:
						this.areaEditTarget.Region.Position += mouseMotion.Relative;
						this.areaEditTarget.Region.Size -= mouseMotion.Relative;
						break;
					case 1:
						this.areaEditTarget.Region.Position += new Vector2(0, mouseMotion.Relative.Y);
						this.areaEditTarget.Region.Size += new Vector2(mouseMotion.Relative.X, -mouseMotion.Relative.Y);
						break;
					case 2:
						this.areaEditTarget.Region.Size += mouseMotion.Relative;
						break;
					case 3:
						this.areaEditTarget.Region.Position += new Vector2(mouseMotion.Relative.X, 0);
						this.areaEditTarget.Region.Size += new Vector2(-mouseMotion.Relative.X, mouseMotion.Relative.Y);
						break;
				}
				return true;
			}
			if (
				mousePos.DistanceSquaredTo(controlPoints[0]) <= minControlDistanceSquared
				|| mousePos.DistanceSquaredTo(controlPoints[2]) <= minControlDistanceSquared
			)
			{
				Input.SetDefaultCursorShape(Input.CursorShape.Bdiagsize);
			}
			else if (
				mousePos.DistanceSquaredTo(controlPoints[1]) <= minControlDistanceSquared
				|| mousePos.DistanceSquaredTo(controlPoints[3]) <= minControlDistanceSquared
			)
			{
				Input.SetDefaultCursorShape(Input.CursorShape.Fdiagsize);
			}
			else
			{
				Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
			}
		}
		return false;
	}

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
		this.AddCustomType(nameof(AreaConstraintComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera2DComponents)}/{nameof(AreaConstraintComponent2D)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(AreaPriorityComponent), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera2DComponents)}/{nameof(AreaPriorityComponent)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(FramingComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera2DComponents)}/{nameof(FramingComponent2D)}.cs"), vcamPosIcon);
		this.AddCustomType(nameof(TweenComponent2D), nameof(Node2D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera2DComponents)}/{nameof(VirtualCamera2DComponents.TweenComponent2D)}.cs"), vcamTraIcon);

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
		this.AddCustomType(nameof(TweenComponent3D), nameof(Node3D), GD.Load<Script>($"res://addons/{nameof(Raele.GDirector)}/src/{nameof(GDirector.VirtualCamera3DComponents)}/{nameof(VirtualCamera3DComponents.TweenComponent3D)}.cs"), vcamTraIcon);

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
		this.RemoveCustomType(nameof(AreaConstraintComponent2D));
		this.RemoveCustomType(nameof(AreaPriorityComponent));
		this.RemoveCustomType(nameof(FramingComponent2D));
		this.RemoveCustomType(nameof(TweenComponent2D));

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
		this.RemoveCustomType(nameof(TweenComponent3D));
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
