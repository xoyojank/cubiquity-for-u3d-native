using System;
using System.Runtime.InteropServices;
using System.Text;

using UnityEngine;

public class CubiquityDLL
{
	// This static constructor is supposed to make sure that the Cubiquity.dll is in the right place before the DllImport is done.
	// It doesn't seem to work, because in Standalone builds the message below is printed after the exception about the .dll not
	// being found. We need to look into this further.
	static CubiquityDLL()
	{
		//Debug.LogError("In CubiquityDLL()");
		Installation.ValidateAndFix();
	}
	
	private static void Validate(int returnCode)
	{
		if(returnCode < 0)
		{
			throw new CubiquityException("An exception occured inside Cubiquity. Please see the log file for details");
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////
	// Volume functions
	////////////////////////////////////////////////////////////////////////////////
	[DllImport ("CubiquityC")]
	private static extern int cuNewColouredCubesVolume(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ, StringBuilder datasetName, uint baseNodeSize, out uint result);
	public static uint NewColoredCubesVolume(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ, string datasetName, uint baseNodeSize)
	{
		uint result;
		Validate(cuNewColouredCubesVolume(lowerX, lowerY, lowerZ, upperX, upperY, upperZ, new StringBuilder(datasetName), baseNodeSize, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuNewColouredCubesVolumeFromVolDat(StringBuilder voldatFolder, StringBuilder datasetName, uint baseNodeSize, out uint result);	
	public static uint NewColoredCubesVolumeFromVolDat(string voldatFolder, string datasetName, uint baseNodeSize)
	{
		uint result;
		Validate(cuNewColouredCubesVolumeFromVolDat(new StringBuilder(voldatFolder), new StringBuilder(datasetName), baseNodeSize, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuNewColouredCubesVolumeFromHeightmap(StringBuilder heightmapFileName, StringBuilder colormapFileName, StringBuilder datasetName, uint baseNodeSize, out uint result);	
	public static uint NewColoredCubesVolumeFromHeightmap(string heightmapFileName, string colormapFileName, string datasetName, uint baseNodeSize)
	{
		uint result;
		Validate(cuNewColouredCubesVolumeFromHeightmap(new StringBuilder(heightmapFileName), new StringBuilder(colormapFileName), new StringBuilder(datasetName), baseNodeSize, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuUpdateVolume(uint volumeHandle);
	public static void UpdateVolume(uint volumeHandle)
	{
		Validate(cuUpdateVolume(volumeHandle));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetEnclosingRegion(uint volumeHandle, out int lowerX, out int lowerY, out int lowerZ, out int upperX, out int upperY, out int upperZ);	
	public static void GetEnclosingRegion(uint volumeHandle, out int lowerX, out int lowerY, out int lowerZ, out int upperX, out int upperY, out int upperZ)
	{		
		Validate(cuGetEnclosingRegion(volumeHandle, out lowerX, out lowerY, out lowerZ, out upperX, out upperY, out upperZ));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetVoxel(uint volumeHandle, int x, int y, int z, out byte red, out byte green, out byte blue, out byte alpha);	
	public static void GetVoxel(uint volumeHandle, int x, int y, int z, out byte red, out byte green, out byte blue, out byte alpha)
	{		
		Validate(cuGetVoxel(volumeHandle, x, y, z, out red, out green, out blue, out alpha));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuSetVoxel(uint volumeHandle, int x, int y, int z, byte red, byte green, byte blue, byte alpha);
	public static void SetVoxel(uint volumeHandle, int x, int y, int z, byte red, byte green, byte blue, byte alpha)
	{
		Validate(cuSetVoxel(volumeHandle, x, y, z, red, green, blue, alpha));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuDeleteColouredCubesVolume(uint volumeHandle);
	public static void DeleteColoredCubesVolume(uint volumeHandle)
	{
		Validate(cuDeleteColouredCubesVolume(volumeHandle));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuAcceptOverrideBlocks(uint volumeHandle);
	public static void AcceptOverrideBlocks(uint volumeHandle)
	{
		Validate(cuAcceptOverrideBlocks(volumeHandle));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuDiscardOverrideBlocks(uint volumeHandle);
	public static void DiscardOverrideBlocks(uint volumeHandle)
	{
		Validate(cuDiscardOverrideBlocks(volumeHandle));
	}
	
	//--------------------------------------------------------------------------------
	
	[DllImport ("CubiquityC")]
	private static extern int cuNewTerrainVolume(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ, StringBuilder datasetName, uint baseNodeSize, uint createFloor, uint floorDepth, out uint result);
	public static uint NewTerrainVolume(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ, string datasetName, uint baseNodeSize, uint createFloor, uint floorDepth)
	{
		uint result;
		Validate(cuNewTerrainVolume(lowerX, lowerY, lowerZ, upperX, upperY, upperZ, new StringBuilder(datasetName), baseNodeSize, createFloor, floorDepth, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuUpdateVolumeMC(uint volumeHandle);
	public static void UpdateVolumeMC(uint volumeHandle)
	{
		Validate(cuUpdateVolumeMC(volumeHandle));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetEnclosingRegionMC(uint volumeHandle, out int lowerX, out int lowerY, out int lowerZ, out int upperX, out int upperY, out int upperZ);	
	public static void GetEnclosingRegionMC(uint volumeHandle, out int lowerX, out int lowerY, out int lowerZ, out int upperX, out int upperY, out int upperZ)
	{		
		Validate(cuGetEnclosingRegionMC(volumeHandle, out lowerX, out lowerY, out lowerZ, out upperX, out upperY, out upperZ));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetVoxelMC(uint volumeHandle, int x, int y, int z, uint index, out byte value);	
	public static void GetVoxelMC(uint volumeHandle, int x, int y, int z, uint index, out byte value)
	{		
		Validate(cuGetVoxelMC(volumeHandle, x, y, z, index, out value));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuSetVoxelMC(uint volumeHandle, int x, int y, int z, uint index, byte value);
	public static void SetVoxelMC(uint volumeHandle, int x, int y, int z, uint index, byte value)
	{
		Validate(cuSetVoxelMC(volumeHandle, x, y, z, index, value));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuDeleteTerrainVolume(uint volumeHandle);
	public static void DeleteTerrainVolume(uint volumeHandle)
	{
		Validate(cuDeleteTerrainVolume(volumeHandle));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuAcceptOverrideBlocksMC(uint volumeHandle);
	public static void AcceptOverrideBlocksMC(uint volumeHandle)
	{
		Validate(cuAcceptOverrideBlocksMC(volumeHandle));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuDiscardOverrideBlocksMC(uint volumeHandle);
	public static void DiscardOverrideBlocksMC(uint volumeHandle)
	{
		Validate(cuDiscardOverrideBlocksMC(volumeHandle));
	}
	
	////////////////////////////////////////////////////////////////////////////////
	// Octree functions
	////////////////////////////////////////////////////////////////////////////////
	[DllImport ("CubiquityC")]
	private static extern int cuHasRootOctreeNode(uint volumeHandle, out uint result);
	public static uint HasRootOctreeNode(uint volumeHandle)
	{
		uint result;
		Validate(cuHasRootOctreeNode(volumeHandle, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetRootOctreeNode(uint volumeHandle, out uint result);
	public static uint GetRootOctreeNode(uint volumeHandle)
	{
		uint result;
		Validate(cuGetRootOctreeNode(volumeHandle, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuHasChildNode(uint nodeHandle, uint childX, uint childY, uint childZ, out uint result);
	public static uint HasChildNode(uint nodeHandle, uint childX, uint childY, uint childZ)
	{
		uint result;
		Validate(cuHasChildNode(nodeHandle, childX, childY, childZ, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetChildNode(uint nodeHandle, uint childX, uint childY, uint childZ, out uint result);
	public static uint GetChildNode(uint nodeHandle, uint childX, uint childY, uint childZ)
	{
		uint result;
		Validate(cuGetChildNode(nodeHandle, childX, childY, childZ, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuNodeHasMesh(uint nodeHandle, out uint result);
	public static uint NodeHasMesh(uint nodeHandle)
	{
		uint result;
		Validate(cuNodeHasMesh(nodeHandle, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetNodePosition(uint nodeHandle, out int x, out int y, out int z);
	public static void GetNodePosition(uint nodeHandle, out int x, out int y, out int z)
	{
		Validate(cuGetNodePosition(nodeHandle, out x, out y, out z));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetMeshLastUpdated(uint nodeHandle, out uint result);
	public static uint GetMeshLastUpdated(uint nodeHandle)
	{
		uint result;
		Validate(cuGetMeshLastUpdated(nodeHandle, out result));
		return result;
	}
	
	//----------------------------------------------------------------------
	
	[DllImport ("CubiquityC")]
	private static extern int cuHasRootOctreeNodeMC(uint volumeHandle, out uint result);
	public static uint HasRootOctreeNodeMC(uint volumeHandle)
	{
		uint result;
		Validate(cuHasRootOctreeNodeMC(volumeHandle, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetRootOctreeNodeMC(uint volumeHandle, out uint result);
	public static uint GetRootOctreeNodeMC(uint volumeHandle)
	{
		uint result;
		Validate(cuGetRootOctreeNodeMC(volumeHandle, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuHasChildNodeMC(uint nodeHandle, uint childX, uint childY, uint childZ, out uint result);
	public static uint HasChildNodeMC(uint nodeHandle, uint childX, uint childY, uint childZ)
	{
		uint result;
		Validate(cuHasChildNodeMC(nodeHandle, childX, childY, childZ, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetChildNodeMC(uint nodeHandle, uint childX, uint childY, uint childZ, out uint result);
	public static uint GetChildNodeMC(uint nodeHandle, uint childX, uint childY, uint childZ)
	{
		uint result;
		Validate(cuGetChildNodeMC(nodeHandle, childX, childY, childZ, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuNodeHasMeshMC(uint nodeHandle, out uint result);
	public static uint NodeHasMeshMC(uint nodeHandle)
	{
		uint result;
		Validate(cuNodeHasMeshMC(nodeHandle, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetNodePositionMC(uint nodeHandle, out int x, out int y, out int z);
	public static void GetNodePositionMC(uint nodeHandle, out int x, out int y, out int z)
	{
		Validate(cuGetNodePositionMC(nodeHandle, out x, out y, out z));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetMeshLastUpdatedMC(uint nodeHandle, out uint result);
	public static uint GetMeshLastUpdatedMC(uint nodeHandle)
	{
		uint result;
		Validate(cuGetMeshLastUpdatedMC(nodeHandle, out result));
		return result;
	}
	
	////////////////////////////////////////////////////////////////////////////////
	// Mesh functions
	////////////////////////////////////////////////////////////////////////////////
	[DllImport ("CubiquityC")]
	private static extern int cuGetNoOfIndices(uint octreeNodeHandle, out uint result);
	[DllImport ("CubiquityC")]
	private static extern int cuGetIndices(uint octreeNodeHandle, out int[] result);
	public static int[] GetIndices(uint octreeNodeHandle)
	{
		uint noOfIndices;
		Validate(cuGetNoOfIndices(octreeNodeHandle, out noOfIndices));
		
		int[] result = new int[noOfIndices];
		Validate(cuGetIndices(octreeNodeHandle, out result));
		
		return result;
	}
		
	[DllImport ("CubiquityC")]
	private static extern int cuGetNoOfVertices(uint octreeNodeHandle, out uint result);
	[DllImport ("CubiquityC")]
	private static extern int cuGetVertices(uint octreeNodeHandle, out CubiquityVertex[] result);
	public static CubiquityVertex[] GetVertices(uint octreeNodeHandle)
	{
		// Based on http://stackoverflow.com/a/1318929
		uint noOfVertices;
		Validate(cuGetNoOfVertices(octreeNodeHandle, out noOfVertices));
		
		CubiquityVertex[] result = new CubiquityVertex[noOfVertices];
		Validate(cuGetVertices(octreeNodeHandle, out result));
		
		return result;
	}
	
	//--------------------------------------------------------------------------------
	
	[DllImport ("CubiquityC")]
	private static extern int cuGetNoOfIndicesMC(uint octreeNodeHandle, out uint result);
	[DllImport ("CubiquityC")]
	private static extern int cuGetIndicesMC(uint octreeNodeHandle, out int[] result);
	public static int[] GetIndicesMC(uint octreeNodeHandle)
	{
		uint noOfIndices;
		Validate(cuGetNoOfIndicesMC(octreeNodeHandle, out noOfIndices));
		
		int[] result = new int[noOfIndices];
		Validate(cuGetIndicesMC(octreeNodeHandle, out result));
		
		return result;
	}
		
	[DllImport ("CubiquityC")]
	private static extern int cuGetNoOfVerticesMC(uint octreeNodeHandle, out uint result);
	[DllImport ("CubiquityC")]
	private static extern int cuGetVerticesMC(uint octreeNodeHandle, out CubiquitySmoothVertex[] result);
	public static CubiquitySmoothVertex[] GetVerticesMC(uint octreeNodeHandle)
	{
		// Based on http://stackoverflow.com/a/1318929
		uint noOfVertices;
		Validate(cuGetNoOfVerticesMC(octreeNodeHandle, out noOfVertices));
		
		CubiquitySmoothVertex[] result = new CubiquitySmoothVertex[noOfVertices];
		Validate(cuGetVerticesMC(octreeNodeHandle, out result));
		
		return result;
	}
	
	////////////////////////////////////////////////////////////////////////////////
	// Clock functions
	////////////////////////////////////////////////////////////////////////////////
	[DllImport ("CubiquityC")]
	private static extern int cuGetCurrentTime(out uint result);
	public static uint GetCurrentTime()
	{
		uint result;
		Validate(cuGetCurrentTime(out result));
		return result;
	}
	
	////////////////////////////////////////////////////////////////////////////////
	// Raycasting functions
	////////////////////////////////////////////////////////////////////////////////
	[DllImport ("CubiquityC")]
	private static extern int cuPickFirstSolidVoxel(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out int resultX, out int resultY, out int resultZ, out uint result);
	public static uint PickFirstSolidVoxel(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out int resultX, out int resultY, out int resultZ)
	{
		uint result;
		Validate(cuPickFirstSolidVoxel(volumeHandle, rayStartX, rayStartY, rayStartZ, rayDirX, rayDirY, rayDirZ, out resultX, out resultY, out resultZ, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuPickLastEmptyVoxel(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out int resultX, out int resultY, out int resultZ, out uint result);
	public static uint PickLastEmptyVoxel(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out int resultX, out int resultY, out int resultZ)
	{
		uint result;
		Validate(cuPickLastEmptyVoxel(volumeHandle, rayStartX, rayStartY, rayStartZ, rayDirX, rayDirY, rayDirZ, out resultX, out resultY, out resultZ, out result));
		return result;
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuPickTerrainSurface(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out float resultX, out float resultY, out float resultZ, out uint result);
	public static uint PickTerrainSurface(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out float resultX, out float resultY, out float resultZ)
	{
		uint result;
		Validate(cuPickTerrainSurface(volumeHandle, rayStartX, rayStartY, rayStartZ, rayDirX, rayDirY, rayDirZ, out resultX, out resultY, out resultZ, out result));
		return result;
	}
	
	////////////////////////////////////////////////////////////////////////////////
	// Editing functions
	////////////////////////////////////////////////////////////////////////////////
	
	[DllImport ("CubiquityC")]
	private static extern int cuSculptTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount);
	public static void SculptTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount)
	{
		Validate(cuSculptTerrainVolume(volumeHandle, centerX, centerY, centerZ, brushInnerRadius, brushOuterRadius, amount));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuBlurTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount);
	public static void BlurTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount)
	{
		Validate(cuBlurTerrainVolume(volumeHandle, centerX, centerY, centerZ, brushInnerRadius, brushOuterRadius, amount));
	}
	
	[DllImport ("CubiquityC")]
	private static extern int cuPaintTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount, uint materialIndex);
	public static void PaintTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount, uint materialIndex)
	{
		Validate(cuPaintTerrainVolume(volumeHandle, centerX, centerY, centerZ, brushInnerRadius, brushOuterRadius, amount, materialIndex));
	}
	
	////////////////////////////////////////////////////////////////////////////////
	// Volume generation functions
	////////////////////////////////////////////////////////////////////////////////
	[DllImport ("CubiquityC")]
	private static extern int cuGenerateFloor(uint volumeHandle, int lowerLayerHeight, uint lowerLayerMaterial, int upperLayerHeight, uint upperLayerMaterial);
	public static void GenerateFloor(uint volumeHandle, int lowerLayerHeight, uint lowerLayerMaterial, int upperLayerHeight, uint upperLayerMaterial)
	{
		Validate(cuGenerateFloor(volumeHandle, lowerLayerHeight, lowerLayerMaterial, upperLayerHeight, upperLayerMaterial));
	}
}
