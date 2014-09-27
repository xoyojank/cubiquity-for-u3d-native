﻿using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections;
using System.Text;

namespace Cubiquity
{
	namespace Impl
	{
		public class OctreeNode : MonoBehaviour
		{
			[System.NonSerialized]
			public uint meshLastSyncronised;
			[System.NonSerialized]
			public uint lastSyncronisedWithVolumeRenderer;
			[System.NonSerialized]
			public uint lastSyncronisedWithVolumeCollider;
			[System.NonSerialized]
			public Vector3 lowerCorner;
			[System.NonSerialized]
			public GameObject[,,] children;
			
			[System.NonSerialized]
			public uint nodeHandle;

			public static GameObject CreateOctreeNode(uint nodeHandle, GameObject parentGameObject)
			{			
				int xPos, yPos, zPos;
				//Debug.Log("Getting position for node handle = " + nodeHandle);
				CubiquityDLL.GetNodePosition(nodeHandle, out xPos, out yPos, out zPos);
				
				StringBuilder name = new StringBuilder("(" + xPos + ", " + yPos + ", " + zPos + ")");
				
				GameObject newGameObject = new GameObject(name.ToString ());
				newGameObject.AddComponent<OctreeNode>();
				
				OctreeNode octreeNode = newGameObject.GetComponent<OctreeNode>();
				octreeNode.lowerCorner = new Vector3(xPos, yPos, zPos);
				octreeNode.nodeHandle = nodeHandle;
				
				if(parentGameObject)
				{
					newGameObject.layer = parentGameObject.layer;
						
					newGameObject.transform.parent = parentGameObject.transform;
					newGameObject.transform.localPosition = new Vector3();
					newGameObject.transform.localRotation = new Quaternion();
					newGameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					
					OctreeNode parentOctreeNode = parentGameObject.GetComponent<OctreeNode>();
					
					if(parentOctreeNode != null)
					{
						Vector3 parentLowerCorner = parentOctreeNode.lowerCorner;
						newGameObject.transform.localPosition = octreeNode.lowerCorner - parentLowerCorner;
					}
					else
					{
						newGameObject.transform.localPosition = octreeNode.lowerCorner;
					}
				}
				else
				{
					newGameObject.transform.localPosition = octreeNode.lowerCorner;
				}
				
				newGameObject.hideFlags = HideFlags.HideInHierarchy;
				
				return newGameObject;
			}
			
			public int syncNode(int availableNodeSyncs, GameObject voxelTerrainGameObject)
			{
				int nodeSyncsPerformed = 0;
				
				if(availableNodeSyncs <= 0)
				{
					return nodeSyncsPerformed;
				}
				
				uint meshLastUpdated = CubiquityDLL.GetMeshLastUpdated(nodeHandle);		
				
				if(meshLastSyncronised < meshLastUpdated)
				{			
					if(CubiquityDLL.NodeHasMesh(nodeHandle) == 1)
					{					
						// Set up the rendering mesh											
						VolumeRenderer volumeRenderer = voxelTerrainGameObject.GetComponent<VolumeRenderer>();
						if(volumeRenderer != null)
						{						
							//Mesh renderingMesh = volumeRenderer.BuildMeshFromNodeHandle(nodeHandle);
							
							Mesh renderingMesh = null;
							if(voxelTerrainGameObject.GetComponent<Volume>().GetType() == typeof(TerrainVolume))
							{
								renderingMesh = BuildMeshFromNodeHandleForTerrainVolume(nodeHandle);
							}
							else if(voxelTerrainGameObject.GetComponent<Volume>().GetType() == typeof(ColoredCubesVolume))
							{
								renderingMesh = BuildMeshFromNodeHandleForColoredCubesVolume(nodeHandle);
							}
					
					        MeshFilter meshFilter = gameObject.GetOrAddComponent<MeshFilter>() as MeshFilter;
						    MeshRenderer meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>() as MeshRenderer;
							
							if(meshFilter.sharedMesh != null)
							{
								DestroyImmediate(meshFilter.sharedMesh);
							}
							
					        meshFilter.sharedMesh = renderingMesh;
						
							meshRenderer.sharedMaterial = volumeRenderer.material;
						}
						
						// Set up the collision mesh
						VolumeCollider volumeCollider = voxelTerrainGameObject.GetComponent<VolumeCollider>();					
						if((volumeCollider != null) && (Application.isPlaying))
						{
							Mesh collisionMesh = volumeCollider.BuildMeshFromNodeHandle(nodeHandle);
							MeshCollider meshCollider = gameObject.GetOrAddComponent<MeshCollider>() as MeshCollider;
							meshCollider.sharedMesh = collisionMesh;
						}
					}
					// If there is no mesh in Cubiquity then we make sure there isn't on in Unity.
					else
					{
						MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>() as MeshCollider;
						if(meshCollider)
						{
							DestroyImmediate(meshCollider);
						}
						
						MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>() as MeshRenderer;
						if(meshRenderer)
						{
							DestroyImmediate(meshRenderer);
						}
						
						MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>() as MeshFilter;
						if(meshFilter)
						{
							DestroyImmediate(meshFilter);
						}
					}
					
					meshLastSyncronised = CubiquityDLL.GetCurrentTime();
					availableNodeSyncs--;
					nodeSyncsPerformed++;
					
				}
				
				VolumeRenderer vr = voxelTerrainGameObject.GetComponent<VolumeRenderer>();
				MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
				if(vr != null && mr != null)
				{
					uint renderThisNode = 0;
					// Horrible hack to choose correct funtion!
					if(voxelTerrainGameObject.GetComponent<Volume>().GetType() == typeof(TerrainVolume))
					{
						renderThisNode = CubiquityDLL.RenderThisNodeMC(nodeHandle);
					}
					else if(voxelTerrainGameObject.GetComponent<Volume>().GetType() == typeof(ColoredCubesVolume))
					{
						renderThisNode = CubiquityDLL.RenderThisNode(nodeHandle);
					}

					mr.enabled = vr.enabled && (renderThisNode != 0);
					
					if(lastSyncronisedWithVolumeRenderer < vr.lastModified)
					{
						mr.receiveShadows = vr.receiveShadows;
						mr.castShadows = vr.castShadows;

						#if UNITY_EDITOR
						EditorUtility.SetSelectedWireframeHidden(mr, !vr.showWireframe);
						#endif

						lastSyncronisedWithVolumeRenderer = Clock.timestamp;
					}
				}
				
				VolumeCollider vc = voxelTerrainGameObject.GetComponent<VolumeCollider>();
				MeshCollider mc = gameObject.GetComponent<MeshCollider>();
				if(vc != null && mc != null)
				{
					if(mc.enabled != vc.enabled) // Not sure we really need this check?
					{
						mc.enabled = vc.enabled;
					}
					
					if(lastSyncronisedWithVolumeCollider < vc.lastModified)
					{
						// Actual syncronization to be filled in in the future when we have something to syncronize.
						lastSyncronisedWithVolumeCollider = Clock.timestamp;
					}
				}
				
				//Now syncronise any children
				for(uint z = 0; z < 2; z++)
				{
					for(uint y = 0; y < 2; y++)
					{
						for(uint x = 0; x < 2; x++)
						{
							if(CubiquityDLL.HasChildNode(nodeHandle, x, y, z) == 1)
							{					
							
								uint childNodeHandle = CubiquityDLL.GetChildNode(nodeHandle, x, y, z);					
								
								GameObject childGameObject = GetChild(x,y,z);
								
								if(childGameObject == null)
								{							
									childGameObject = OctreeNode.CreateOctreeNode(childNodeHandle, gameObject);
									
									SetChild(x, y, z, childGameObject);
								}
								
								//syncNode(childNodeHandle, childGameObject);
								
								OctreeNode childOctreeNode = childGameObject.GetComponent<OctreeNode>();
								int syncs = childOctreeNode.syncNode(availableNodeSyncs, voxelTerrainGameObject);
								availableNodeSyncs -= syncs;
								nodeSyncsPerformed += syncs;
							}
						}
					}
				}
				
				return nodeSyncsPerformed;
			}
			
			public GameObject GetChild(uint x, uint y, uint z)
			{
				if(children != null)
				{
					return children[x, y, z];
				}
				else
				{
					return null;
				}
			}
			
			public void SetChild(uint x, uint y, uint z, GameObject gameObject)
			{
				if(children == null)
				{
					children = new GameObject[2, 2, 2];
				}
				
				children[x, y, z] = gameObject;
			}
			
			unsafe public Mesh BuildMeshFromNodeHandleForTerrainVolume(uint nodeHandle)
			{					
				// Get the data from Cubiquity.                
                uint noOfIndices = CubiquityDLL.GetNoOfIndicesMC(nodeHandle);
                ushort* indices = CubiquityDLL.GetIndicesMC(nodeHandle);
                uint noOfVertices = CubiquityDLL.GetNoOfVerticesMC(nodeHandle);
				TerrainVertex* vertices = CubiquityDLL.GetVerticesMC(nodeHandle);

                // Cubiquity uses 16-bit index arrays to save space, and it appears Unity does the same (at least, there is
                // a limit of 65535 vertices per mesh). However, the Mesh.triangles property is of the signed 32-bit int[]
                // type rather than the unsigned 16-bit ushort[] type. Perhaps this is so they can switch to 32-bit index
                // buffers in the future? At any rate, it means we have to perform a conversion.
                int[] indicesAsInt = new int[noOfIndices];
                for (int ct = 0; ct < noOfIndices; ct++)
                {
                    indicesAsInt[ct] = *indices;
                    indices++;
                }
				
				// Create the arrays which we'll copy the data to.
                Vector3[] positions = new Vector3[noOfVertices];
                Vector3[] normals = new Vector3[noOfVertices];
                Color32[] colors32 = new Color32[noOfVertices];
                Vector2[] uv = new Vector2[noOfVertices];
                Vector2[] uv2 = new Vector2[noOfVertices];
				
                // Move the data from our Cubiquity-owned memory to managed memory. We also
                // need to decode the data as Cubiquity stores it in a compressed form.
                for (int ct = 0; ct < noOfVertices; ct++)
				{
					// Get and decode the position
                    positions[ct].Set(vertices->x, vertices->y, vertices->z);
                    positions[ct] *= (1.0f / 256.0f);
					
					// Get the materials. Some are stored in color...
                    colors32[ct].r = vertices->m0;
                    colors32[ct].g = vertices->m1;
                    colors32[ct].b = vertices->m2;
                    colors32[ct].a = vertices->m3;

                    // And some are stored in UVs.
                    uv[ct].Set(vertices->m4 / 255.0f, vertices->m5 / 255.0f);
                    uv2[ct].Set(vertices->m6 / 255.0f, vertices->m7 / 255.0f);

                    // Get and decode the normal
                    ushort ux = (ushort)((vertices->normal >> (ushort)8) & (ushort)0xFF);
                    ushort uy = (ushort)((vertices->normal) & (ushort)0xFF);

					// Convert to floats in the range [-1.0f, +1.0f].
					float ex = ux * (1.0f / 127.5f) - 1.0f;
                    float ey = uy * (1.0f / 127.5f) - 1.0f;
					
					// Reconstruct the origninal vector. This is a C++ implementation
					// of Listing 2 of http://jcgt.org/published/0003/02/01/
					float vx = ex;
					float vy = ey;
					float vz = 1.0f - Math.Abs(ex) - Math.Abs(ey);
					
					if (vz < 0.0f)
					{
						float refX = ((1.0f - Math.Abs(vy)) * (vx >= 0.0f ? +1.0f : -1.0f));
						float refY = ((1.0f - Math.Abs(vx)) * (vy >= 0.0f ? +1.0f : -1.0f));
						vx = refX;
						vy = refY;
					}
                    normals[ct].Set(vx, vy, vz);

                    // Now do the next vertes.
                    vertices++;
				}

                // Create rendering mesh
                Mesh mesh = new Mesh(); //Should pool these
                mesh.Clear(false); // When pooling, this will be required to share Mesh instancews between terrain and colored cubes.
                mesh.hideFlags = HideFlags.DontSave;
				
				// Assign vertex data to the mesh.
                mesh.vertices = positions;
                mesh.normals = normals;
                mesh.colors32 = colors32;
                mesh.uv = uv;
                mesh.uv2 = uv2;

                // Assign index data to the meshes.
                mesh.triangles = indicesAsInt;

                return mesh;
			}
			
			public Mesh BuildMeshFromNodeHandleForColoredCubesVolume(uint nodeHandle)
			{
				// At some point I should read this: http://forum.unity3d.com/threads/5687-C-plugin-pass-arrays-from-C
				
				Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f); // Required for the CubicVertex decoding process.
					
				// Create rendering and possible collision meshes.
				Mesh renderingMesh = new Mesh();		
				renderingMesh.hideFlags = HideFlags.DontSave;
	
				// Get the data from Cubiquity.
				int[] indices = CubiquityDLL.GetIndices(nodeHandle);		
				ColoredCubesVertex[] cubiquityVertices = CubiquityDLL.GetVertices(nodeHandle);			
				
				// Create the arrays which we'll copy the data to.
		        Vector3[] renderingVertices = new Vector3[cubiquityVertices.Length];	
				Color32[] renderingColors = new Color32[cubiquityVertices.Length];
				
				for(int ct = 0; ct < cubiquityVertices.Length; ct++)
				{
					// Get the vertex data from Cubiquity.
					Vector3 position = new Vector3(cubiquityVertices[ct].x, cubiquityVertices[ct].y, cubiquityVertices[ct].z);
					position -= offset; // Part of the CubicVertex decoding process.
					QuantizedColor color = cubiquityVertices[ct].color;
						
					// Copy it to the arrays.
					renderingVertices[ct] = position;
					renderingColors[ct] = (Color32)color;
				}
				
				// Assign vertex data to the meshes.
				renderingMesh.vertices = renderingVertices; 
				renderingMesh.colors32 = renderingColors;
				renderingMesh.triangles = indices;
				
				// FIXME - Get proper bounds
				renderingMesh.bounds.SetMinMax(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(32.0f, 32.0f, 32.0f));
				
				return renderingMesh;
			}
		}
	}
}
