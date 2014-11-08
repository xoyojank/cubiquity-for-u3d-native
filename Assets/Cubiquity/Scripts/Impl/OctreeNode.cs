using UnityEngine;

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
            public uint structureLastSynced;
			[System.NonSerialized]
            public uint propertiesLastSynced;
            [System.NonSerialized]
            public uint meshLastSynced;
			[System.NonSerialized]
            public uint nodeAndChildrenLastSynced;
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
                // Get node position from Cubiquity
                CuOctreeNode cuOctreeNode = CubiquityDLL.GetOctreeNode(nodeHandle);
                int xPos = cuOctreeNode.posX, yPos = cuOctreeNode.posY, zPos = cuOctreeNode.posZ;
				
                // Build a corresponding game object
				StringBuilder name = new StringBuilder("OctreeNode (" + xPos + ", " + yPos + ", " + zPos + ")");				
				GameObject newGameObject = new GameObject(name.ToString ());
                newGameObject.hideFlags = HideFlags.HideInHierarchy;

                // Use parent properties as appropriate
                newGameObject.transform.parent = parentGameObject.transform;
                newGameObject.layer = parentGameObject.layer;

                // It seems that setting the parent does not cause the object to move as Unity adjusts 
                // the child transform to compensate (this can be seen when moving objects between parents 
                // in the hierarchy view). Reset the local transform as shown here: http://goo.gl/k5n7M7
                newGameObject.transform.localRotation = Quaternion.identity;
                newGameObject.transform.localPosition = Vector3.zero;                
                newGameObject.transform.localScale = Vector3.one;

                // Attach an OctreeNode component
                OctreeNode octreeNode = newGameObject.AddComponent<OctreeNode>();
                octreeNode.lowerCorner = new Vector3(xPos, yPos, zPos);
                octreeNode.nodeHandle = nodeHandle;
					
                // Does the parent game object have an octree node attached?
				OctreeNode parentOctreeNode = parentGameObject.GetComponent<OctreeNode>();					
				if(parentOctreeNode)
				{
                    // Cubiquity gives us absolute positions for the Octree nodes, but for a hierarchy of
                    // GameObjects we need relative positions. Obtain these by subtracting parent position.
                    newGameObject.transform.localPosition = octreeNode.lowerCorner - parentOctreeNode.lowerCorner;
				}
				else
				{
                    // If not then the parent must be the Volume GameObject and the one we are creating
                    // must be the root of the Octree. In this case we can use the position directly.
					newGameObject.transform.localPosition = octreeNode.lowerCorner;
				}
				
				return newGameObject;
			}

            public static int syncNodeMeshes(int availableNodeSyncs, GameObject nodeGameObject, GameObject voxelTerrainGameObject)
			{
				int nodeSyncsPerformed = 0;				
				if(availableNodeSyncs <= 0)
				{
					return nodeSyncsPerformed;
				}

                OctreeNode octreeNode = nodeGameObject.GetComponent<OctreeNode>();
                //uint meshOrChildMeshLastUpdated = CubiquityDLL.GetMeshOrChildMeshLastUpdated(octreeNode.nodeHandle);

                CuOctreeNode cuOctreeNode = CubiquityDLL.GetOctreeNode(octreeNode.nodeHandle);

                //if ((octreeNode.meshAndChildMeshesLastSyncronised < cuOctreeNode.meshLastChangedRecursive) || (octreeNode.propertiesLastSyncronised < cuOctreeNode.propertiesLastChangedRecursive))
                //if (cuOctreeNode.nodeOrChildrenLastChanged > octreeNode.nodeAndChildrenLastSynced)
                {
                    //uint meshLastUpdated = CubiquityDLL.GetMeshLastUpdated(octreeNode.nodeHandle);
                    if (cuOctreeNode.meshLastChanged > octreeNode.meshLastSynced)
                    {
                        if (cuOctreeNode.hasMesh == 1)
                        {
                            // Set up the rendering mesh											
                            VolumeRenderer volumeRenderer = voxelTerrainGameObject.GetComponent<VolumeRenderer>();
                            if (volumeRenderer != null)
                            {
                                Mesh renderingMesh = null;
                                if (voxelTerrainGameObject.GetComponent<Volume>().GetType() == typeof(TerrainVolume))
                                {
                                    renderingMesh = MeshConversion.BuildMeshFromNodeHandleForTerrainVolume(octreeNode.nodeHandle, false);
                                }
                                else if (voxelTerrainGameObject.GetComponent<Volume>().GetType() == typeof(ColoredCubesVolume))
                                {
                                    renderingMesh = MeshConversion.BuildMeshFromNodeHandleForColoredCubesVolume(octreeNode.nodeHandle, false);
                                }

                                MeshFilter meshFilter = nodeGameObject.GetOrAddComponent<MeshFilter>() as MeshFilter;
                                MeshRenderer meshRenderer = nodeGameObject.GetOrAddComponent<MeshRenderer>() as MeshRenderer;

                                if (meshFilter.sharedMesh != null)
                                {
                                    DestroyImmediate(meshFilter.sharedMesh);
                                }

                                meshFilter.sharedMesh = renderingMesh;

                                meshRenderer.sharedMaterial = volumeRenderer.material;
                            }

                            // Set up the collision mesh
                            VolumeCollider volumeCollider = voxelTerrainGameObject.GetComponent<VolumeCollider>();
                            if ((volumeCollider != null) && (Application.isPlaying))
                            {
                                Mesh collisionMesh = volumeCollider.BuildMeshFromNodeHandle(octreeNode.nodeHandle);
                                MeshCollider meshCollider = nodeGameObject.GetOrAddComponent<MeshCollider>() as MeshCollider;
                                meshCollider.sharedMesh = collisionMesh;
                            }
                        }
                        // If there is no mesh in Cubiquity then we make sure there isn't on in Unity.
                        else
                        {
                            MeshCollider meshCollider = nodeGameObject.GetComponent<MeshCollider>() as MeshCollider;
                            if (meshCollider)
                            {
                                DestroyImmediate(meshCollider);
                            }

                            MeshRenderer meshRenderer = nodeGameObject.GetComponent<MeshRenderer>() as MeshRenderer;
                            if (meshRenderer)
                            {
                                DestroyImmediate(meshRenderer);
                            }

                            MeshFilter meshFilter = nodeGameObject.GetComponent<MeshFilter>() as MeshFilter;
                            if (meshFilter)
                            {
                                DestroyImmediate(meshFilter);
                            }
                        }

                        octreeNode.meshLastSynced = CubiquityDLL.GetCurrentTime(); // Could perhaps just use the meshLastUpdated time here?
                        availableNodeSyncs--;
                        nodeSyncsPerformed++;
                    }

                    if (cuOctreeNode.structureLastChanged > octreeNode.structureLastSynced)
                    {
                        uint[, ,] childHandleArray = new uint[2, 2, 2];
                        childHandleArray[0, 0, 0] = cuOctreeNode.childHandle000;
                        childHandleArray[0, 0, 1] = cuOctreeNode.childHandle001;
                        childHandleArray[0, 1, 0] = cuOctreeNode.childHandle010;
                        childHandleArray[0, 1, 1] = cuOctreeNode.childHandle011;
                        childHandleArray[1, 0, 0] = cuOctreeNode.childHandle100;
                        childHandleArray[1, 0, 1] = cuOctreeNode.childHandle101;
                        childHandleArray[1, 1, 0] = cuOctreeNode.childHandle110;
                        childHandleArray[1, 1, 1] = cuOctreeNode.childHandle111;

                        //Now syncronise any children
                        for (uint z = 0; z < 2; z++)
                        {
                            for (uint y = 0; y < 2; y++)
                            {
                                for (uint x = 0; x < 2; x++)
                                {
                                    if (childHandleArray[x, y, z] != 0xFFFFFFFF)
                                    {
                                        uint childNodeHandle = childHandleArray[x, y, z];

                                        if (octreeNode.GetChild(x, y, z) == null)
                                        {
                                            octreeNode.SetChild(x, y, z, OctreeNode.CreateOctreeNode(childNodeHandle, nodeGameObject));
                                        }
                                    }
                                    else
                                    {
                                        if (octreeNode.GetChild(x, y, z))
                                        {
                                            Utility.DestroyImmediateWithChildren(octreeNode.GetChild(x, y, z));
                                            octreeNode.SetChild(x, y, z, null);
                                        }
                                    }
                                }
                            }
                        }

                        octreeNode.structureLastSynced = CubiquityDLL.GetCurrentTime();
                    }

                    if (cuOctreeNode.propertiesLastChanged > octreeNode.propertiesLastSynced)
            {
                VolumeRenderer vr = voxelTerrainGameObject.GetComponent<VolumeRenderer>();
                        MeshRenderer mr = nodeGameObject.GetComponent<MeshRenderer>();
                if (vr != null && mr != null)
                {
                    mr.enabled = vr.enabled && (cuOctreeNode.renderThisNode != 0);

                            if (octreeNode.lastSyncronisedWithVolumeRenderer < vr.lastModified)
                    {
                        mr.receiveShadows = vr.receiveShadows;
                        mr.castShadows = vr.castShadows;
#if UNITY_EDITOR
                        EditorUtility.SetSelectedWireframeHidden(mr, !vr.showWireframe);
#endif
                                octreeNode.lastSyncronisedWithVolumeRenderer = Clock.timestamp;
                    }
                }

                VolumeCollider vc = voxelTerrainGameObject.GetComponent<VolumeCollider>();
                        MeshCollider mc = nodeGameObject.GetComponent<MeshCollider>();
                if (vc != null && mc != null)
                {
                    if (mc.enabled != vc.enabled) // Not sure we really need this check?
                    {
                        mc.enabled = vc.enabled;
                    }

                            if (octreeNode.lastSyncronisedWithVolumeCollider < vc.lastModified)
                    {
                        // Actual syncronization to be filled in in the future when we have something to syncronize.
                                octreeNode.lastSyncronisedWithVolumeCollider = Clock.timestamp;
                            }
                    }
                }

                //Now syncronise any children
                for (uint z = 0; z < 2; z++)
                {
                    for (uint y = 0; y < 2; y++)
                    {
                        for (uint x = 0; x < 2; x++)
                        {
                                if (octreeNode.GetChild(x, y, z) != null)
                            {
                                    int syncs = OctreeNode.syncNodeMeshes(availableNodeSyncs, octreeNode.GetChild(x, y, z), voxelTerrainGameObject);
                                    availableNodeSyncs -= syncs;
                                    nodeSyncsPerformed += syncs;
                            }
                        }
                    }
                }

                    /*if(nodeSyncsPerformed == 0)
                    {
                        octreeNode.meshAndChildMeshesLastSyncronised = CubiquityDLL.GetCurrentTime();
                    }*/
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
		}
	}
}
