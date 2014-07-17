using UnityEngine;
using System.Collections;

namespace Cubiquity
{
	public class CreateVoxelDatabase : MonoBehaviour
	{
		void Start ()
		{
			int planetRadius = 60;
			string saveLocation = Paths.voxelDatabases + "/planet.vdb";
			Region volumeBounds = new Region(-planetRadius, -planetRadius, -planetRadius, planetRadius, planetRadius, planetRadius);		
			TerrainVolumeData data = VolumeData.CreateEmptyVolumeData<TerrainVolumeData>(volumeBounds, saveLocation);
			
			// The numbers below control the thickness of the various layers.
			TerrainVolumeGenerator.GeneratePlanet(data, planetRadius, planetRadius - 1, planetRadius - 10, planetRadius - 35);
		}
	}
}