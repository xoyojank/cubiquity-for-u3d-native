using UnityEngine;
using System.Collections;

using Cubiquity.Impl;

namespace Cubiquity
{
	[ExecuteInEditMode]
	/// Causes the colored cubes volume to have a collision mesh and allows it to participate in collisions.
	/**
	 * See the base VolumeCollider class for further details.
	 */
	public class ColoredCubesVolumeCollider : VolumeCollider
	{
        public override Mesh BuildMeshFromNodeHandle(uint nodeHandle)
        {
            return MeshConversion.BuildMeshFromNodeHandleForColoredCubesVolume(nodeHandle, true);
        }
	}
}
