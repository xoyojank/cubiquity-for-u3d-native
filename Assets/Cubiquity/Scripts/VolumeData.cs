using UnityEngine;

using System;
using System.IO;
using System.Collections;

using Cubiquity.Impl;

namespace Cubiquity
{
	/// Base class representing the actual 3D grid of voxel values
	/**
	 * This class primarily serves as a light-weight wrapper around the \ref secVoxelDatabase "voxel databases" which are used by the %Cubiquity engine,
	 * allowing them to be treated as Unity3D assets. The voxel databases themselves are typically stored in the 'Streaming Assets' or 'Temporary Cache'
	 * folder (depending on the usage scenario), with the actual path being specified by the 'fullPathToVoxelDatabase' property. The VolumeData and it's
	 * subclasses then forward requests such as finding the dimensions of the voxel data or getting/setting the values of the individual voxels.
	 * 
	 * An instance of VolumeData can be created from an existing voxel database, or it can create an empty voxel database on demand. The class also
	 * abstracts the properties and functionality which are common to all types of volume data regardless of the type of the underlying voxel. Note that
	 * users should not interact with the VolumeData directly but should instead work with one of the derived classes.
	 * 
	 * \sa TerrainVolumeData, ColoredCubesVolumeData
	 */
	[System.Serializable]
	public abstract class VolumeData : ScriptableObject
	{
		private enum VoxelDatabasePaths { Streaming, Temporary, Root };
		
		/// Gets the dimensions of the VolumeData.
		/**
		 * %Cubiquity voxel databases (and by extension the VolumeData) have a fixed size which is specified on creation. You should not attempt to access
		 * and location outside of this range.
		 * 
		 * \return The dimensions of the volume. The values are inclusive, so you can safely access the voxel at the positions given by Region.lowerCorner
		 * and Region.upperCorner.
		 */
	    public Region enclosingRegion
	    {
	        get
			{			
				// The initialization can fail (bad filename, database locked, etc), so the volume handle could still be null.
				Region result = new Region(0, 0, 0, 0, 0, 0);
				if(volumeHandle != null)
				{
					CubiquityDLL.GetEnclosingRegion(volumeHandle.Value,
						out result.lowerCorner.x, out result.lowerCorner.y, out result.lowerCorner.z,
						out result.upperCorner.x, out result.upperCorner.y, out result.upperCorner.z);
				}				
				return result;
			}
	    }
		
		[SerializeField]
		private VoxelDatabasePaths basePath;
		
		[SerializeField]
		private string relativePathToVoxelDatabase;
		
		/// Gets the full path to the voxel database which backs this instance.
		/**
		 * The full path to the voxel database is derived from the relative path which you can optionally specify at creation time, and the base path
		 * which depends on the way the instance was created. See CreateEmptyVolumeData<VolumeDataType>() and CreateFromVoxelDatabase<VolumeDataType>()
		 * for more information about how the base path is chosen.
		 * 
		 * This property is provided mostly for informational and debugging purposes as you are unlikely to directly make use of it.
		 * 
		 * \return The full path to the voxel database
		 */
		public string fullPathToVoxelDatabase
		{
			get
			{
				if(relativePathToVoxelDatabase.Length == 0)
				{
					throw new System.ArgumentException(@"The relative path to the voxel database should never be empty.
						Perhaps you created the VolumeData with ScriptableObject.CreateInstance(), rather than with
						CreateEmptyVolumeData() or CreateFromVoxelDatabase()?");
				}
				
				string basePathString = null;
				switch(basePath)
				{
				case VoxelDatabasePaths.Streaming:
					basePathString = Paths.voxelDatabases + '/';
					break;
				case VoxelDatabasePaths.Temporary:
					basePathString = Application.temporaryCachePath + '/';
					break;
				case VoxelDatabasePaths.Root:
					basePathString = "";
					break;
				}
				return basePathString + relativePathToVoxelDatabase;
			}
		}
		
		// If set, this identifies the volume to the Cubiquity DLL. It can
		// be tested against null to find if the volume is currently valid.
		/// \cond
		protected uint? mVolumeHandle = null;
		internal uint? volumeHandle
		{
			get
			{
				// We open the database connection the first time that any attempt is made to access the data. See the
				// comments in the 'VolumeData.OnEnable()' function for more information about why we don't do it there.
				if(mVolumeHandle == null)
				{
					InitializeExistingCubiquityVolume();
				}

				return mVolumeHandle;
			}
			set { mVolumeHandle = value; }
		}
		/// \endcond
		
		// Don't really like having this defined here. The base node size should be a rendering property rather than a
		// property of the actual volume data. Need to make this change in the underlying Cubiquity library as well though.
		/// \cond
		protected static uint DefaultBaseNodeSize = 32;
		/// \endcond

		// Used to stop too much spamming of error messages
		/// \cond
		protected bool initializeAlreadyFailed = false;
		/// \endcond

		/**
		 * It is possible for %Cubiquity voxel databse files to be created outside of the %Cubiquity for Unity3D ecosystem (see the \ref secCubiquity
		 * "user manual" if you are not clear on the difference between 'Cubiquity and 'Cubiquity for Unity3D'). For example, the %Cubiquity SDK contains
		 * importers for converting a variety of external file formats into voxel databases. This function provides a way for you to create volume data
		 * which is linked to such a user provided voxel database.
		 * 
		 * \param pathToVoxelDatabase The path to the .vdb files containing the data. You should provide one of the following:
		 *   - <b>An absolute path:</b> In this case the provided path will be used directly to reference the desired .vdb file.
		 *   - <b>A relative path:</b> If you provide a relative path then is it assumed to be relative to the streaming assets folder, 
		 *   because the contents of this folder are included in the build and can therefore be accessed in the editor, during play 
		 *   mode, and also in standalone builds.
		 */
		public static VolumeDataType CreateFromVoxelDatabase<VolumeDataType>(string pathToVoxelDatabase) where VolumeDataType : VolumeData
		{			
			VolumeDataType volumeData = ScriptableObject.CreateInstance<VolumeDataType>();
			if(Path.IsPathRooted(pathToVoxelDatabase))
			{
				volumeData.basePath = VoxelDatabasePaths.Root;
			}
			else
			{
				volumeData.basePath = VoxelDatabasePaths.Streaming;
			}
			volumeData.relativePathToVoxelDatabase = pathToVoxelDatabase;
			
			volumeData.InitializeExistingCubiquityVolume();
			
			return volumeData;
		}
		
		/**
		 * Creates an empty volume data instance with a new voxel database (.vdb) being created at the location specified by the optional path.
		 * 
		 * \param region A Region instance specifying the dimensions of the volume data. You should not later try to access voxels outside of this range.
		 * \param pathToVoxelDatabase The path where the voxel database should be created. You should provide one of the following:
		 *   - <b>A null or empty string:</b> In this case a temporary filename will be used and the .vdb will reside in a temporary folder.
		 *   You will be unable to access the data after the volume asset is destroyed. This usage is appropriate if you want to create 
		 *   a volume at run time and then fill it with data generated by your own means (e.g. procedurally).
		 *   - <b>An absolute path:</b> In this case the provided path will be used directly to reference the desired .vdb file. Keep in mind that
		 *   such a path may be specific to the system on which it was created. Therefore you are unlikely to want to use this approach in
		 *   editor code as the file will not be present when building and distrubuting your application, but you may wish to use it in
		 *   play mode from a users machine to create a volume that can be saved between play sessions.
		 *   - <b>A relative path:</b> If you provide a relative path then is it assumed to be relative to the streaming assets folder, where the
		 *   .vdb will then be created. The contents of the streaming assets folder are distributed with your application, and so this
		 *   variant is probably most appropriate if you are creating volume through code in the editor. However, be aware that you should not
		 *   use it in play mode because the streaming assets folder may be read only.
		 */
		public static VolumeDataType CreateEmptyVolumeData<VolumeDataType>(Region region, string pathToVoxelDatabase = null) where VolumeDataType : VolumeData
		{			
			VolumeDataType volumeData = ScriptableObject.CreateInstance<VolumeDataType>();

			if(String.IsNullOrEmpty(pathToVoxelDatabase))
			{
				// No path was provided, so create a temporary path and the created .vdb file cannot be used after the current session.
				string pathToCreateVoxelDatabase = Cubiquity.Impl.Utility.GenerateRandomVoxelDatabaseName();
				volumeData.basePath = VoxelDatabasePaths.Temporary;
				volumeData.relativePathToVoxelDatabase = pathToCreateVoxelDatabase;				
				volumeData.hideFlags = HideFlags.DontSave; //Don't serialize this instance as it uses a temporary file for the voxel database.
			}
			else if(Path.IsPathRooted(pathToVoxelDatabase))
			{
				// The user provided a rooted (non-relative) path and so we use the details directly.
				volumeData.basePath = VoxelDatabasePaths.Root;
				volumeData.relativePathToVoxelDatabase = pathToVoxelDatabase;
			}
			else
			{
				// The user provided a relative path, which we then assume to be relative to the streaming assets folder.
				// This should only be done in edit more (not in play mode) as stated belwo.
				if(Application.isPlaying)
				{
					Debug.LogWarning("You should not provide a relative path when creating empty volume " +
						"data in play mode, because the streaming assets folder might not have write access..");
				}

				// As the user is providing a name for the voxel database then it follows that they want to make use of it later.
				// In this case it should not be in the temp folder so we put it in streaming assets.
				volumeData.basePath = VoxelDatabasePaths.Streaming;
				volumeData.relativePathToVoxelDatabase = pathToVoxelDatabase;
			}
			
			volumeData.InitializeEmptyCubiquityVolume(region);
			
			return volumeData;
		}
		
		private void Awake()
		{
			// Make sure the Cubiquity library is installed.
			Installation.ValidateAndFix();
		}
		
		private void OnEnable()
		{	
			// We should reset this flag from time-to-time incase the user has fixed the issue. This 
			// seems like an appropriate place as the user may fix the issue aand then reload the scene.
			initializeAlreadyFailed = false;

			// It would seem intuitive to open and initialise the voxel database from this function. However, there seem to be 
			// some problems with this approach.
			//		1. OnEnable() is (sometimes?) called when simply clicking on the asset in the project window. In this scenario
			// 		we don't really want/need to connect to the database.
			//		2. OnEnable() does not seem to be called when a volume asset is dragged onto an existing volume, and this is
			// 		the main way of setting which data a volume should use.
		}
		
		private void OnDisable()
		{
			// Note: For some reason this function is not called when transitioning between edit/play mode if this scriptable 
			// object has been turned into an asset. Therefore we also call Initialize...()/Shutdown...() from the Volume class.
			ShutdownCubiquityVolume();
		}
		
		private void OnDestroy()
		{
			// If the voxel database was created in the temporary location
			// then we can be sure the user has no further use for it.
			if(basePath == VoxelDatabasePaths.Temporary)
			{
				File.Delete(fullPathToVoxelDatabase);
				
				if(File.Exists(fullPathToVoxelDatabase))
				{
					Debug.LogWarning("Failed to delete voxel database from temporary cache");
				}
			}
		}
		
		/// \cond
		protected abstract void InitializeEmptyCubiquityVolume(Region region);
		protected abstract void InitializeExistingCubiquityVolume();
		public abstract void ShutdownCubiquityVolume();
		/// \endcond
	}
}
