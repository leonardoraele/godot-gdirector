# GDirector (WIP)

A Godot addon that provides a complete solution for operating 3D cameras with dynamic behavior, for both cutscenes and
gameplay. Inspired by Unity's Cinemachine package.

<!--
GDirector adds highly configurable pre-implemented camera behavior nodes that control the camera's position and rotation
individually, allowing you to compose procedural camera shots that are updated every frame at runtime. Because the
camera shots are procedually calculated at runtime, they remain updated even if different elements of the scene changes,
either as a result of player input or when elements are changed by you.

GDirector also has the ability interpolate between pre-configured virtual cameras, allowing you to create very
complex camera shots.

You can also configure various conditions that the virtual cameras can check to calculate their priority and let
GDirector automatically select the best camera shot at runtime, creating camera behavior that reacts to the player
automatically based on the parameters configured by you.

GDirector is designed to be expandable, so that you can create custom behaviors for your virtual cameras that are not
pre-implemented by the many GDirector pre-implemented camera behavior nodes.
-->

## Installation

> ⚠ GDirector is still a work in progress. It is not feature complete yet and is likely to contain bugs.
If you find any, please create an issue in GitHub.

In your project's `addons` folder, run:

	git submodule add <this_repository_clone_uri> GDirector

## Feature Roadmap

#### Camera Position Nodes
- [X] FollowPosition
- [ ] FramingConstraint <!-- Add option to select wether the controller should change the camera's local XY position, local Z position, a set global plane, or FOV to accomodate the framed target; or a combination of the three in set weights -->
- [X] OrbitalMovement
- [ ] FollowAlongPath <!-- Tries to follow a target node while moving the camera only within a Path3D. -->
- [ ] AreaConstraint <!-- Forces the camera to remain inside an Area3D -->
- [ ] FreeRoam
- [ ] Noise <!-- I think two 1D perlim noises applied to each axis should do -->
- [ ] Bounce
- [ ] ObstacleAvoidance <!-- Perform raycasts at angles and pivot the camera around obstacles to avoid breaking LOS—pivot toward the obstacle's normal -->
- [ ] ClipPrevention <!-- Prevents the camera from clipping through walls or other obstacles. The proposed solution is to add an invisible spherical rigidbody that follow the camera. Every physics frame, it moves toward the camera as much as possible, then sets the camera position to it's position. It should also slide alongside the wall to prevent hard snaps while the camera moves behind the wall. -->

#### Camera Rotation Nodes
- [X] LookAtTarget
- [ ] LookAtGroup <!-- Create option to look at dynamic groups (node groups) -->
- [X] MimicRotation
- [ ] FreeLook

#### Prioritization Nodes <!-- Each priority controller should have a blend mode (either Add, Sub, or Override) -->
- [X] FramingPriority
- [X] ProximityPriority <!-- Controls priority by multiplying the distance between two targets by a set factor and base values -->
- [X] LineOfSightPriority
- [ ] NodeCountPriority <!-- Controls priority by counting the number of nodes on the scene with a specific group -->
- [ ] VelocityBasedPriority <!-- This controller compares the camera's position between this and the previous frame to set priority -->
- [ ] PlayerInputPriority <!-- Adds priority when the player enters camera-control inputs; and reduce priority later after a delay -->

#### Other
- [X] CameraTransition
- [ ] BlendedVirtualCamera <!-- A VCam whose position and rotation are derived from other cameras and a slider control -->
- [ ] CameraSettings
