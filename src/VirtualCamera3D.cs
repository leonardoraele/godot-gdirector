using System.Collections.Generic;
using System.Threading;
using Godot;

namespace Raele.GDirector;

[Tool]
public partial class VirtualCamera3D : Node3D, IVirtualCamera
{
	// -----------------------------------------------------------------------------------------------------------------
	// EXPORTED VARS
	// -----------------------------------------------------------------------------------------------------------------

	[ExportToolButton("Force Go Live/Release")] public Callable ToogleIsLiveOverrideToolButton
		=> Callable.From(() => this.AsInterface().ForceGoLive = !this.AsInterface().ForceGoLive);
	[Export] public double BasePriority { get; private set; } = 0;

	// -----------------------------------------------------------------------------------------------------------------
	// SIGNALS
	// -----------------------------------------------------------------------------------------------------------------

	[Signal] public delegate void PriorityChangedEventHandler(double newPriority, double oldPriority);
	[Signal] public delegate void IsLiveChangedEventHandler(bool isLive);

	// -----------------------------------------------------------------------------------------------------------------
	// FIELDS
	// -----------------------------------------------------------------------------------------------------------------

	public double Priority { get; set; }

	private CancellationTokenSource ServerRegistrationCancelSource = new();

	// -----------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// -----------------------------------------------------------------------------------------------------------------

	Vector2 IVirtualCamera.Position2D
	{
		get => new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y);
		set => this.GlobalPosition = new Vector3(value.X, value.Y, this.GlobalPosition.Z);
	}
	Vector3 IVirtualCamera.Position3D
	{
		get => this.GlobalPosition;
		set => this.GlobalPosition = value;
	}
	float IVirtualCamera.Rotation2D
	{
		get => this.GlobalRotation.Z;
		set => this.GlobalRotation = this.GlobalRotation with { Z = value };
	}
	Vector3 IVirtualCamera.Rotation3D
	{
		get => this.GlobalRotation;
		set => this.GlobalRotation = value;
	}

	// -----------------------------------------------------------------------------------------------------------------
	// OVERRIDES
	// -----------------------------------------------------------------------------------------------------------------

	public override void _EnterTree()
	{
		base._EnterTree();
		this.AsInterface()._EnterTree(this.ServerRegistrationCancelSource.Token);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		this.AsInterface()._ExitTree();
		this.ServerRegistrationCancelSource.Cancel();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		this.AsInterface()._Process();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// METHODS
	// -----------------------------------------------------------------------------------------------------------------

	public IVirtualCamera AsInterface() => this;
}
