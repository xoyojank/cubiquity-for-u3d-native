using UnityEngine;
using System.Collections;

using Cubiquity;

public class VoxelForgeToCubiquity
{
    public static GameObject convert(GameObject voxelForgePrefab)
    {
        // Create a volume the same size as the VoxelForge volume
        int width = 64;
        int height = 64;
        int depth = 64;

        ColoredCubesVolumeData cubiquityVolumeData = VolumeData.CreateEmptyVolumeData<ColoredCubesVolumeData>(new Region(0, 0, 0, width - 1, height - 1, depth - 1));

        // Just dummy data for testing.
        Color32 red = new Color32(255, 0, 0, 255);
        Color32 green = new Color32(0, 255, 0, 255);
        Color32 blue = new Color32(0, 0, 255, 255);
        Color32 transparent = new Color32(0, 0, 0, 0);

        // Iterate over each voxel and copy the voxel color from VoxelForge to Cubiquity.
        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    switch(y % 4)
                    {
                        case 0:
                            cubiquityVolumeData.SetVoxel(x, y, z, (QuantizedColor)red);
                            break;
                        case 1:
                            cubiquityVolumeData.SetVoxel(x, y, z, (QuantizedColor)green);
                            break;
                        case 2:
                            cubiquityVolumeData.SetVoxel(x, y, z, (QuantizedColor)blue);
                            break;
                        case 3:
                            cubiquityVolumeData.SetVoxel(x, y, z, (QuantizedColor)transparent);
                            break;
                    }
                }
            }
        }

        // Create a Cubiquity volume from the provided volume data. This function also creates renderers and colliders.
        GameObject cubiquityVolume = ColoredCubesVolume.CreateGameObject(cubiquityVolumeData, true, true);

        // Should also set the transform here.

        return cubiquityVolume;
    }
}
