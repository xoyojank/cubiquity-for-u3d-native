Cubiquity voxel engine
======================
.. attention ::
	This system is not currently under active development, but a replacement system ('Cubiquity 2') is currently in the research phase. For more details please see this blog post: http://www.volumesoffun.com/reflections-on-cubiquity-and-finding-the-path-forward/

Cubiquity is a voxel engine written in C++ and released under the terms of the `MIT license <https://www.tldrlegal.com/l/mit>`_. It was created to provide a higher-level wrapper around our `PolyVox <https://bitbucket.org/volumesoffun/polyvox>`_ library and to simplify integration into existing game engine such as `Unity <https://unity3d.com/>`_ and `Unreal <https://www.unrealengine.com>`_. It began life as a commercial project and was sold on the Unity Asset Store, but limited time and real-world commitments have led to us releasing the whole system for free.

It is not expected that you will make use of this voxel engine directly, but instead use one of the existing integration layers for Unity or Unreal:

Unity integration
-----------------
* Source Code: https://bitbucket.org/volumesoffun/cubiquity-for-unity3d
* Support thread: http://forum.unity3d.com/threads/cubiquity-a-fast-and-powerful-voxel-plugin-for-unity3d.184599/

Unreal integration
------------------
* Source code: https://github.com/volumesoffun/cubiquity-for-unreal-engine
* Support thread: https://forums.unrealengine.com/showthread.php?29873-Cubiquity-for-UE4-Voxel-Terrain-Plugin

This repository is mainly for users who want to recompile the library for new platforms, or for advanced users who want to fix bugs or extend the capabilities of the system.