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
            public bool renderThisNode;
			[System.NonSerialized]
			public uint lastSyncronisedWithVolumeRenderer;
			[System.NonSerialized]
			public uint lastSyncronisedWithVolumeCollider;
			[System.NonSerialized]
			public Vector3 lowerCorner;
			[System.NonSerialized]
			public GameObject[,,] children;

            [System.NonSerialized]
            public uint height;
			
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

            public static void syncNode(ref uint availableSyncOperations, GameObject nodeGameObject, GameObject voxelTerrainGameObject)
			{
                OctreeNode octreeNode = nodeGameObject.GetComponent<OctreeNode>();
                CuOctreeNode cuOctreeNode = CubiquityDLL.GetOctreeNode(octreeNode.nodeHandle);

                ////////////////////////////////////////////////////////////////////////////////
                // Has anything in this node or its children changed? If so, we may need to syncronise the node's properties, mesh and
                // structure. Each of these can be teste against a timestamp. We may also need to do this recursively on child nodes.
                ////////////////////////////////////////////////////////////////////////////////
                if (cuOctreeNode.nodeOrChildrenLastChanged > octreeNode.nodeAndChildrenLastSynced)
                {
                    bool resyncedProperties = false; // See comments where this is tested - it's a bit of a hack

                    ////////////////////////////////////////////////////////////////////////////////
                    // 1st test - Have the properties of the node changed?
                    ////////////////////////////////////////////////////////////////////////////////
                    if (cuOctreeNode.propertiesLastChanged > octreeNode.propertiesLastSynced)
                    {
                        octreeNode.renderThisNode = cuOctreeNode.renderThisNode != 0;
                        octreeNode.propertiesLastSynced = CubiquityDLL.GetCurrentTime();
                        resyncedProperties = true;
                    }

                    ////////////////////////////////////////////////////////////////////////////////
                    // 2nd test - Has the mesh changed and do we have time to syncronise it?
                    ////////////////////////////////////////////////////////////////////////////////
                    if ((cuOctreeNode.meshLastChanged > octreeNode.meshLastSynced) && (availableSyncOperations > 0))
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

                                switch(cuOctreeNode.height)
                                {
                                    case 0:
                                        meshRenderer.sharedMaterial = volumeRenderer.material;
                                        break;
                                    case 1:
                                        meshRenderer.sharedMaterial = volumeRenderer.materialLod1;
                                        break;
                                    case 2:
                                        meshRenderer.sharedMaterial = volumeRenderer.materialLod2;
                                        break;
                                    default:
                                        meshRenderer.sharedMaterial = volumeRenderer.material;
                                        break;
                                }

                                meshRenderer.enabled = volumeRenderer.enabled && octreeNode.renderThisNode;
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

                        octreeNode.meshLastSynced = CubiquityDLL.GetCurrentTime();
                        availableSyncOperations--;
                    }

                    // We want to syncronize the properties before the mesh, so that the enabled flag can be set correctly when the mesh
                    // is created. But we also want to syncronize properties after the mesh, so we can apply the correct enabled flag to
                    // existing meshes when the node's 'renderThisNode' flag has changed. Therefore we set the 'resyncedProperties' flag
                    // previously to let ourseves know that we should come back an finish the propertiy syncing here. It's a bit of a hack.
                    if (resyncedProperties)
                    {
                        VolumeRenderer volumeRenderer = voxelTerrainGameObject.GetComponent<VolumeRenderer>();
                        MeshRenderer meshRenderer = nodeGameObject.GetComponent<MeshRenderer>();
                        if (volumeRenderer != null && meshRenderer != null)
                        {
                            meshRenderer.enabled = volumeRenderer.enabled && octreeNode.renderThisNode;
                        }
                    }

                    ////////////////////////////////////////////////////////////////////////////////
                    // 3rd test - Has the structure of the octree node changed (gained or lost children)?
                    ////////////////////////////////////////////////////////////////////////////////
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

                    ////////////////////////////////////////////////////////////////////////////////
                    // The last step of syncronization is to apply it recursively to our children.
                    ////////////////////////////////////////////////////////////////////////////////
                    for (uint z = 0; z < 2; z++)
                    {
                        for (uint y = 0; y < 2; y++)
                        {
                            for (uint x = 0; x < 2; x++)
                            {
                                if (octreeNode.GetChild(x, y, z) != null && availableSyncOperations > 0)
                                {
                                    OctreeNode.syncNode(ref availableSyncOperations, octreeNode.GetChild(x, y, z), voxelTerrainGameObject);
                                }
                            }
                        }
                    }

                    if (availableSyncOperations > 0)
                    {
                        octreeNode.nodeAndChildrenLastSynced = CubiquityDLL.GetCurrentTime();
                    }
                }
			}

            public static void syncNodeProperties(GameObject nodeGameObject, GameObject voxelTerrainGameObject)
            {
                OctreeNode octreeNode = nodeGameObject.GetComponent<OctreeNode>();

                VolumeRenderer vr = voxelTerrainGameObject.GetComponent<VolumeRenderer>();
                MeshRenderer mr = nodeGameObject.GetComponent<MeshRenderer>();
                if (vr != null && mr != null)
                {
                    mr.enabled = vr.enabled && octreeNode.renderThisNode;

                    mr.receiveShadows = vr.receiveShadows;
                    mr.castShadows = vr.castShadows;
#if UNITY_EDITOR
                    EditorUtility.SetSelectedWireframeHidden(mr, !vr.showWireframe);
#endif
                }

                VolumeCollider vc = voxelTerrainGameObject.GetComponent<VolumeCollider>();
                MeshCollider mc = nodeGameObject.GetComponent<MeshCollider>();
                if (vc != null && mc != null)
                {
                    mc.enabled = vc.enabled;
                }

                foreach (Transform child in nodeGameObject.transform)
                {
                    OctreeNode.syncNodeProperties(child.gameObject, voxelTerrainGameObject);
                }

                //Now syncronise any children
                /*for (uint z = 0; z < 2; z++)
                {
                    for (uint y = 0; y < 2; y++)
                    {
                        for (uint x = 0; x < 2; x++)
                        {
                            if (nodeGameObject.GetChild(x, y, z) != null)
                            {
                                OctreeNode.syncNodeProperties(nodeGameObject.GetChild(x, y, z), voxelTerrainGameObject);
                            }
                        }
                    }
                }*/
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
