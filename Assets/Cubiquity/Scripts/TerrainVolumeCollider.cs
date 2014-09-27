using UnityEngine;
using System.Collections;

using Cubiquity.Impl;

namespace Cubiquity
{
	[ExecuteInEditMode]
	/// Causes the terrain volume to have a collision mesh and allows it to participate in collisions.
	/**
	 * See the base VolumeCollider class for further details.
	 */
	public class TerrainVolumeCollider : VolumeCollider
	{
		unsafe public override Mesh BuildMeshFromNodeHandle(uint nodeHandle)
		{
			Mesh collisionMesh = new Mesh();
			collisionMesh.hideFlags = HideFlags.DontSave;

			// Get the data from Cubiquity.
            // Cubiquity uses 16-bit index arrays to save space, and it appears Unity does the same (at least, there is
            // a limit of 65535 vertices per mesh). However, the Mesh.triangles property is of the signed 32-bit int[]
            // type rather than the unsigned 16-bit ushort[] type. Perhaps this is so they can switch to 32-bit index
            // buffers in the future? At any rate, it means we have to perform a conversion.
            uint noOfIndices = CubiquityDLL.GetNoOfIndicesMC(nodeHandle);
            ushort* result = CubiquityDLL.GetIndicesMC(nodeHandle);
            int[] indices = new int[noOfIndices];
            for (int ct = 0; ct < noOfIndices; ct++)
            {
                indices[ct] = *result;
                result++;
            }

			TerrainVertex[] cubiquityVertices = CubiquityDLL.GetVerticesMC(nodeHandle);			
			
			// Create the arrays which we'll copy the data to.	
			Vector3[] vertices = new Vector3[cubiquityVertices.Length];
			
			for(int ct = 0; ct < cubiquityVertices.Length; ct++)
			{
				// Get the vertex data from Cubiquity.
				vertices[ct] = new Vector3(cubiquityVertices[ct].x, cubiquityVertices[ct].y, cubiquityVertices[ct].z);
				vertices[ct] *= (1.0f / 256.0f);
			}
			
			// Assign vertex data to the meshes.
			collisionMesh.vertices = vertices;
			collisionMesh.triangles = indices;
			
			return collisionMesh;
		}
	}
}
