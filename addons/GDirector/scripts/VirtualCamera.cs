using System.Collections.Generic;
using Godot;
using Raele.GDirector.VirtualCameraControllers;

namespace Raele.GDirector;

public partial class VirtualCamera : Node3D
{
    [Export] public int Priority = 0;
    [Export] public bool ResetPositionOnTransitionStart;

    [Signal] public delegate void TransitionStartEventHandler();
    [Signal] public delegate void TransitionFinishEventHandler();

    public Dictionary<VirtualCamera, TransitionController> TransitionControllers = new();
    public TransitionController? DefaultTransitionController;

    public override void _EnterTree()
    {
        base._EnterTree();
		GDirectorServer.Register(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
		GDirectorServer.Unregister(this);
    }
}
