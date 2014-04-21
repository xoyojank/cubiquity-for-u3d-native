﻿using UnityEngine;
using System.Collections;

using Cubiquity.Impl;

namespace Cubiquity
{
	public abstract class VolumeCollider : MonoBehaviour
	{
		public abstract Mesh BuildMeshFromNodeHandle(uint nodeHandle);
		
		public uint lastModified = Clock.timestamp;
		
		// Dummy start method rqured for the 'enabled' checkbox to show up in the inspector.
		void Start() { }
	}
}
