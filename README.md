# GDirector

![GDirector Logo](./icons/logo.png)

GDirector is an addon for Godot that provides a complete solution for operating 3D cameras with dynamic behavior,
for both cutscenes and gameplay.
Inspired by Unity's Cinemachine package.

> ⚠ GDirector is still a work in progress. It is not feature complete yet and is likely to contain bugs.
If you find any, please create an issue in GitHub.

- [Showcase](#showcase)
- [Basic Concepts](#basic-concepts)
	- [Virtual Cameras & Camera Transitions](#virtual-cameras--camera-transitions)
	- [Behavior Composition With Nodes](#behavior-composition-with-nodes)
	- [Procedural Camera Behavior](#procedural-camera-behavior)
- [Getting Started](#getting-started)
	- [Installation](#installation)
	- [How To Use](#how-to-use)
- [API Documentation](#documentation)
- [Feature Roadmap](#feature-roadmap)
- [License](#license)
- [Contribution](#contribution)
- [Development](#development)

## Showcase

> TODO

## Basic Concepts

### Virtual Cameras & Camera Transitions

When using GDirector, you will have only a single `Camera3D` node in your scene.
GDirector controls this camera and performs smooth transitions between shots.
(or camera cuts)

You create these shots with GDirector's node, `VirtualCamera3D`.
Virtual cameras differ from regular `Camera3D` nodes in that they don't render anything to the screen.
Instead, they simple provide the information necessary to tell GDirector how the camera should be positioned and rotated.

If you have used Unity's Cinemachine package, this concept should feel very familiar.

When building your scene, you should add multiple `VirtualCamera3D` nodes.
Think of it as a movie set with multiple cameras,
each one dedicated to capture a shot from a different angle.
During gameplay, you can **activate** the virtual camera that is most appropriate for the moment,
and GDirector will control the actual `Camera3D` node.
It will either by performing cuts directly to the position and rotation of the new active virtual camera,
or it will smoothing transition between the previous virtual camera to the next.
(depending on the settings of the virtual cameras)

Only one virtual camera is **active** at any given time.

Thi system allows you to create complex camera shots with ease.

### Behavior Composition With Nodes
By default, virtual cameras as static—it won't move and won't rotate.
GDirector provides several nodes that you can attach to virtual cameras to customize their behavior.
These are called Virtual Camera Controllers.

Each controller node adds a different behavior to the camera.
Some of them control the camera's position,
others control its rotation,
others control when the camera gains priority over others, and so on.

For example, the `FollowPosition` controller makes the camera follow a node of your choice,
from a distance you can configure, and an optional damping.
The `LookAtTarget` rotates the camera every frame so that it keeps pointing to a given position.

You can mix and match different controller nodes to configure the behavior of the camera,
allowing you to easily program complex camera behavior just by attaching nodes to the camera and configuring their
settings in the inspector.

### Procedural Shot Composition

Controller nodes determine the behavior of the camera at runtime,
allowing you to compose procedural camera shots that are updated every frame.

Because the camera shots are procedually calculated at runtime,
they remain updated even if different elements of the scene changes.
This is ideal for gameplay, specially when you have dynamic elements,
like moving characters or objects, that your camera has to keep track of.

It's also useful even for less dynamic, more predictable moments, like scripted cutscenes.
In these cases, the procedural nature of the camera means you don't need to adjust camera positions or scripts manually
whenever other elements of the scene change as the game evolves.

### Procedural Camera Prioritization

Like it's inspiration (Cinemachine),
GDirector uses a priority system to procedurally determine which virtual camera should be active at each point in time.

Using pre-implemented priority controllers,
you can configure various conditions that the virtual cameras can check to dynamically calculate their priority.
For example, the `LineOfSightPriority` node adds priority to the camera when a given target
(e.g. the player character) is in the camera view,
and the `ProximityPriority` adds priority to the camera when the target is in a certain distance from it.
You can also write your own scripts to add priority to the camera based on logic that is unique to your game.

GDirector will reevaluate the active virtual camera whenever there is a priority change,
and automatically select the best camera shot.

With this system, GDirector is capable of responding to dynamic game events in real time,
creating camera behavior that reacts to the player automatically based on the parameters configured by you.

### Plays Great With `AnimationPlayer` and `PathFollow3D` Nodes

GDirector works great when combined with the `AnimationPlayer` node and/or the `PathFollow3D` node.

When using an `AnimationPlayer` to build a cutscene,
you can manually activate different virtual cameras,
(overriding the automatic priority system in favor of precise control of the camera shots)
while allowing virtual camera controllers to control the camera transition from one virtual camera to another.

You can also attach a virtual camera to a `PathFollow3D` and use the `AnimationPlayer` to animate it's progress along a `Path3D`.

### Easy To Expand

GDirector provides many pre-implemented virtual camera controller nodes,
but it's also designed to be expandable, so that you can write your own scripts
to control the virtual cameras.

You can even combine your script with existing pre-implemented virtual camera controllers provided by GDirector.

<!--
The secret is that the `VirtualCamera3D` node already has all the necessary logic to interact with GDirector.

That means the virtual camera controllers are just simple `Node`-based scripts
with logic that control their parent `VirtualCamera3D` in someone.

Because of this, you don't need to do anything special to implement your own virtual camera controllers.
You just have to create a script that retrieves a reference to the parent on `_read`,
and controls it's position or rotation.

You can even combine your script with other existing virtual camera controllers,
as long as you pay attention not to control properties of the camera that the other virtual camera controller is already
controlling.
For example, if you change the camera's `position` in your script and you add a virtual camera controller that also
changes this property, the last node to `_process` will override the change made by the previous one.
-->

## Getting Started

### Installation

GDirector was not added to Godot's asset library yet.
Until then, here's how you can use it:

1. Download this repository as a .zip file.

1. Extract to any folder you want.

1. Copy the contents of the `addons` folder to your project's `addons` folder.
	If your project doesn't have an `addons` folder, creature one.

1. If you have never created a C# script in the project you are importing
	GDirector into, then create a new C# script by right-clicking any directory
	in your project and selecting "Create New > Script...". Select C# as the
	script's language, leave the name as is, confirm the creation,
	then delete it.

	This step is necessary because GDirector was written in C#, but Godot will
	only creature the configuration files necessary to compile C# scripts in
	your project when you create a C# script for the first time in that project.

1. Click on the "Build" button on the top right corner of the Godot editor, next
	to the "Run Project" button.

1. Enable GDirector in your project settings.
	1. Open "Project > Project Settings..." (in the application's menu at the top)
	1. Go to the "Plugins" tab
	1. Check the box next to GDirector.

### How To Use

GDirector will automatically detect the active camera in a scene by calling `SceneTree.root.get_camera_3d()`,
and then take over it.

> **Note:** If you want to manually choose the `Camera3D` node GDirector should control,
add the `GDirectorAnchor` to that camera.

To create a new virtual camera, simply add the `VirtualCamera3D` node to your scene.
You can have any number of these as you want.
<!--
Performance-wise, the virtual camera nodes themselves are very computationally cheap,
however, the virtual camera controllers have different costs, and some of them can be expensive.
-->

By default, virtual cameras are static—they won't move or rotate.
Unless you want the camera to be static, add a Position Controller node and a Rotation Controller node to it.

By default, when transitioning from one virtual camera to the next,
GDirector will always look for a `CameraTransition` node in the next virtual camera.
If there is no `CameraTransition` node, a camera cut is performed,
otherwise the interpolation settings in the node are used to perform the transition.
If you want smooth transition to the newly created camera, add a `CameraTransition` node to it.

If you want GDirector to automatically prioritize this camera during gameplay,
add PriorityController nodes to it.

If you want the different camera settings that only apply to this virtual camera, add a `CameraSettings` node to it.

## API Documentation

<!--
Access the link below to go to the docs.

https://leonardoraele.github.io/godot-gdirector
-->

> TODO

## Feature Roadmap

See [Roadmap](./docs/roadmap.md).

## License

GDirector is licensed under **Creative Commons Attribution 4.0 International** (CC BY 4.0).

In simple terms, this means you have maximum freedom to do what you want with it,
as long as you provide appropriate credits to the original author in your work,
including a URI or hyperlink to this repository and a copyright notice.

This text is only a short summary of the license.
For the full license text, read the [`LICENSE.txt`](/LICENSE.txt) file.

## Contribution

**Bug reports** and **feature requests** are very welcome.
If you have any, plase create a new issue in GitHub.

Unfortunately, Pull Requests won't be accepted.

<!--
## Development

This section is only useful for people intending to fork this repository.
If you don't know what this means, you can ignore this section.

### Documentation

The static html documentation website is generated using [docfx](https://github.com/dotnet/docfx).

To generate it, follow the steps:
(run the commands in the project's root directory)

1. Update docfx. You only need to do this once.
```
dotnet tool update -g docfx
```
1. Then, to generate the docs, run:
```
docfx
```

Alternativelly, to generate the docs and serve the doc website locally, run:
(this useful for seeting the results locally before commiting changes)
```
docfx --serve
```
-->
