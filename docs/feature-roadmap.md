
### Position Controllers

- [X] **FollowPosition**

	Makes the camera follow a target node.

- [ ] FramingConstraint

	Add option to select wether the controller should change the camera's local XY position, local Z position, a set global plane, or FOV to accomodate the framed target; or a combination of the three in set weights -->

- [X] OrbitalMovement
- [ ] FollowAlongPath <!-- Tries to follow a target node while moving the camera only within a Path3D. -->
- [ ] AreaConstraint <!-- Forces the camera to remain inside an Area3D -->
- [ ] FreeRoam
- [ ] Noise <!-- I think two 1D perlim noises applied to each axis should do -->
- [ ] Bounce
- [ ] ObstacleAvoidance <!-- Perform raycasts at angles and pivot the camera around obstacles to avoid breaking LOS—pivot toward the obstacle's normal -->
- [ ] ClipPrevention <!-- Prevents the camera from clipping through walls or other obstacles. The proposed solution is to add an invisible spherical rigidbody that follow the camera. Every physics frame, it moves toward the camera as much as possible, then sets the camera position to it's position. It should also slide alongside the wall to prevent hard snaps while the camera moves behind the wall. -->

#### Camera Position Nodes

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
- [ ] GDirectorAnchor <!-- Use this node to tell GDirector which camera it should control. -->
