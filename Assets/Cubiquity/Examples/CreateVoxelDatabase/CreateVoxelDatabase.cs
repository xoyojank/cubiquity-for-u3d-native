using UnityEngine;
using System;
using System.Collections;

namespace Cubiquity
{
	public class CreateVoxelDatabase : MonoBehaviour
	{
		void Start ()
		{
			int planetRadius = 60;

			// Randomize the filename incase the file already exists
			System.Random randomIntGenerator = new System.Random();
			int randomInt = randomIntGenerator.Next();
			string saveLocation = Paths.voxelDatabases + "/planet-" + randomInt + ".vdb";

			Region volumeBounds = new Region(-planetRadius, -planetRadius, -planetRadius, planetRadius, planetRadius, planetRadius);		
			TerrainVolumeData data = VolumeData.CreateEmptyVolumeData<TerrainVolumeData>(volumeBounds, saveLocation);
			data.CommitChanges();
			
			// The numbers below control the thickness of the various layers.
			TerrainVolumeGenerator.GeneratePlanet(data, planetRadius, planetRadius - 1, planetRadius - 10, planetRadius - 35);
		}
	}
}