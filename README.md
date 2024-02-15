# GDirector (WIP)

A Godot addon for configuring procedural behavior for 3D cameras, heavily inspired by Unity's Cinemachine, made with
both cutscenes and gameplay in mind.

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

> ⚠ GDirector is a work in progress. It is not feature complete yet and is likely to contain bugs.
If you find any, please create an issue in GitHub.

In your project's `addons` folder, run:

	git submodule add <this_repository_clone_uri> GDirector

## Feature Roadmap

#### Position Controllers
- (X) FollowPositionController
- (!) FramingPositionController <!-- Select wether the controller should change the camera's XY position, Z position, or FOV to accomodate the framed target; or a combination of the three in set weights -->
- (X) OrbitalPositionController
- (X) PathTrackPositionController
- ( ) BouncePositionController
- ( ) PlayerInputPositionController

#### Look Direction Controllers
- (X) LookAtTargetController
- (!) LookAtGroupController <!-- Create option to look at dynamic groups (node groups) -->
- (X) MimicRotationController
- (!) PlayerInputLookController
- ( ) TiltController

#### Priority Controllers <!-- Each priority controller should have a blend mode (either Add, Sub, or Override) -->
- ( ) DistanceBasedPriorityController <!-- Controls priority by multiplying the distance between two targets by a set factor and base values -->
- ( ) NodeCountPriorityController <!-- Controls priority by counting the number of nodes on the scene with a specific group -->
- ( ) PlayerInputPriorityController <!-- Adds priority when the player enters camera-control inputs; and reduce priority later after a delay -->
- ( ) VelocityBasedPriorityController <!-- This controller compares the camera's position between this and the previous frame to set priority -->

#### Transition Controllers
- (!) TransitionController

#### Other
- ( ) PreemptiveObstacleAvoidanceController <!-- Perform raycasts at angles and pivot the camera around obstacles to avoid breaking LOS—pivot toward the obstacle's normal -->
- ( ) CollisionController <!-- Add collisiton detection to the VCam—basically, it's a Rigidbody that moves toward the VCam and, when it can't, moves the VCam toward it -->
- ( ) DynamicCameraDistanceController <!-- Pulls the camera toward closer to the player through an obstacle to prevent breaking LOS  -->
- ( ) NoiseController <!-- I think two 1D perlim noise, applied to each axis should do -->
- ( ) BlendedVirtualCamera <!-- A VCam whose position and rotation are derived from other cameras and a slider control -->
- ( ) ScreenShakeController
