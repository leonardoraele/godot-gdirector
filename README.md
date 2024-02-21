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

#### Position Controllers
- [X] FollowPositionController
- [ ] FramingPositionController <!-- Add option to select wether the controller should change the camera's local XY position, local Z position, a set global plane, or FOV to accomodate the framed target; or a combination of the three in set weights -->
- [X] OrbitalPositionController
- [ ] PathTrackPositionController <!-- Moves the camera in a Path3D while trying to follow a target node without leaving the path -->
- [X] PathFollowPositionController <!-- Uses a PathFollow node to move the camera through a Path3D -->
- [ ] PathConstraintController <!-- Forces the camera to remain in a Path3D. If it leaves the Path3D (probably caused by another controller), moves it back to the track. -->
- [ ] BouncePositionController
- [ ] PlayerInputPositionController
- [ ] AreaConstraintController <!-- Forces the camera to remain inside an Area3D -->

#### Look Direction Controllers
- [X] LookAtTargetController
- [ ] LookAtGroupController <!-- Create option to look at dynamic groups (node groups) -->
- [ ] LookAtPathController <!-- Contains the look to a Path3D, looking to the closest point in the Path3D to a follow target -->
- [X] MimicRotationController
- [ ] PlayerInputLookController
- [ ] TiltController

#### Priority Controllers <!-- Each priority controller should have a blend mode (either Add, Sub, or Override) -->
- [X] FramingPriorityController
- [X] ProximityPriorityController <!-- Controls priority by multiplying the distance between two targets by a set factor and base values -->
- [X] LineOfSightPriorityController
- [ ] NodeCountPriorityController <!-- Controls priority by counting the number of nodes on the scene with a specific group -->
- [ ] PlayerInputPriorityController <!-- Adds priority when the player enters camera-control inputs; and reduce priority later after a delay -->
- [ ] VelocityBasedPriorityController <!-- This controller compares the camera's position between this and the previous frame to set priority -->

#### Transition Controllers
- [ ] TransitionController

#### Other
- [ ] PreemptiveObstacleAvoidanceController <!-- Perform raycasts at angles and pivot the camera around obstacles to avoid breaking LOS—pivot toward the obstacle's normal -->
- [ ] CollisionController <!-- Add collisiton detection to the VCam—basically, it's a Rigidbody that moves toward the VCam and, when it can't, moves the VCam toward it -->
- [ ] DynamicCameraDistanceController <!-- Pulls the camera toward closer to the player through an obstacle to prevent breaking LOS  -->
- [ ] NoiseController <!-- I think two 1D perlim noise, applied to each axis should do -->
- [ ] BlendedVirtualCamera <!-- A VCam whose position and rotation are derived from other cameras and a slider control -->
- [ ] ScreenShakeController
