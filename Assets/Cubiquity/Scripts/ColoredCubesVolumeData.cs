﻿using UnityEngine;

using System;
using System.IO;
using System.Collections;

using Cubiquity;
using Cubiquity.Impl;

namespace Cubiquity
{
	/// An inplementation of VolumeData which stores a QuantizedColor for each voxel.
	/**
	 * This class provides the actual 3D grid of color values which are used by the ColoredCubesVolume. You can use the provided interface to directly
	 * manipulate the volume by getting or setting the color of each voxel.
	 */
	[System.Serializable]
	public sealed class ColoredCubesVolumeData : VolumeData
	{
		/** 
		 * \copydoc CreateFromVoxelDatabase<VolumeDataType>(string)
		 */
		public static ColoredCubesVolumeData CreateFromVoxelDatabase(string relativePathToVoxelDatabase)
		{
			return CreateFromVoxelDatabase<ColoredCubesVolumeData>(relativePathToVoxelDatabase);
		}
		
		/** 
		 * \copydoc CreateEmptyVolumeData<VolumeDataType>(Region)
		 */
		public static ColoredCubesVolumeData CreateEmptyVolumeData(Region region)
		{
			return CreateEmptyVolumeData<ColoredCubesVolumeData>(region);
		}
		
		/** 
		 * \copydoc CreateEmptyVolumeData<VolumeDataType>(Region, string)
		 */
		public static ColoredCubesVolumeData CreateEmptyVolumeData(Region region, string relativePathToVoxelDatabase)
		{
			return CreateEmptyVolumeData<ColoredCubesVolumeData>(region, relativePathToVoxelDatabase);
		}
		
		/// Gets the color of the specified position.
		/**
		 * \param x The 'x' position of the voxel to get.
		 * \param y The 'y' position of the voxel to get.
		 * \param z The 'z' position of the voxel to get.
		 * \return The color of the voxel.
		 */
		public QuantizedColor GetVoxel(int x, int y, int z)
		{
			QuantizedColor result;
			if(volumeHandle.HasValue)
			{
				CubiquityDLL.GetVoxel(volumeHandle.Value, x, y, z, out result);
			}
			else
			{
				//Should maybe throw instead.
				result = new QuantizedColor();
			}
			return result;
		}
		
		/// Sets the color of the specified position.
		/**
		 * \param x The 'x' position of the voxel to set.
		 * \param y The 'y' position of the voxel to set.
		 * \param z The 'z' position of the voxel to set.
		 * \param quantizedColor The color the voxel should be set to.
		 */
		public void SetVoxel(int x, int y, int z, QuantizedColor quantizedColor)
		{
			if(volumeHandle.HasValue)
			{
				if(x >= enclosingRegion.lowerCorner.x && y >= enclosingRegion.lowerCorner.y && z >= enclosingRegion.lowerCorner.z
					&& x <= enclosingRegion.upperCorner.x && y <= enclosingRegion.upperCorner.y && z <= enclosingRegion.upperCorner.z)
				{						
					CubiquityDLL.SetVoxel(volumeHandle.Value, x, y, z, quantizedColor);
				}
			}
		}
		
		/// \cond
		protected override void InitializeEmptyCubiquityVolume(Region region)
		{				
			// This function might get called multiple times. E.g the user might call it striaght after crating the volume (so
			// they can add some initial data to the volume) and it might then get called again by OnEnable(). Handle this safely.
			if(volumeHandle == null)
			{
				// Create an empty region of the desired size.
				volumeHandle = CubiquityDLL.NewEmptyColoredCubesVolume(region.lowerCorner.x, region.lowerCorner.y, region.lowerCorner.z,
					region.upperCorner.x, region.upperCorner.y, region.upperCorner.z, fullPathToVoxelDatabase, DefaultBaseNodeSize);
			}
		}
		/// \endcond

		/// \cond
		protected override void InitializeExistingCubiquityVolume()
		{				
			// This function might get called multiple times. E.g the user might call it striaght after crating the volume (so
			// they can add some initial data to the volume) and it might then get called again by OnEnable(). Handle this safely.
			if(volumeHandle == null)
			{
				// Create an empty region of the desired size.
				volumeHandle = CubiquityDLL.NewColoredCubesVolumeFromVDB(fullPathToVoxelDatabase, DefaultBaseNodeSize);
			}
		}
		/// \endcond
		
		/// \cond
		protected override void ShutdownCubiquityVolume()
		{
			if(volumeHandle.HasValue)
			{
				// We only save if we are in editor mode, not if we are playing.
				bool saveChanges = !Application.isPlaying;
				
				if(saveChanges)
				{
					CubiquityDLL.AcceptOverrideBlocks(volumeHandle.Value);
				}
				CubiquityDLL.DiscardOverrideBlocks(volumeHandle.Value);
				
				CubiquityDLL.DeleteColoredCubesVolume(volumeHandle.Value);
				volumeHandle = null;
			}
		}
		/// \endcond
	}
}