/*******************************************************************************
* The MIT License (MIT)
*
* Copyright (c) 2016 David Williams and Matthew Williams
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

#include "ImportHeightmap.h"

#include "Exceptions.h"
#include "HeaderOnlyLibs.h"

#include "CubiquityC.h"

#include <algorithm>
#include <iomanip>
#include <iostream>
#include <cstdint>
#include <sstream>
#include <vector>

using namespace std;

void importHeightmap(ez::ezOptionParser& options)
{
	LOG(INFO) << "Importing from heightmap...";

	//if (outputFormat == CU_COLORED_CUBES)
	if (options.isSet("-coloredcubes"))
	{
		return importHeightmapAsColoredCubesVolume(options);
	}
	//else if (outputFormat == CU_TERRAIN)
	else if (options.isSet("-terrain"))
	{
		return importHeightmapAsTerrainVolume(options);
	}
	else
	{
		throwException(OptionsError("No volume type specified."));
	}
}

void importHeightmapAsColoredCubesVolume(ez::ezOptionParser& options)
{
	// At this point we know the -heightmap and -coloredcubes flags were set, otherwise we wouldn't be in this function.

	// Open the heightmap
	int heightmapWidth = 0, heightmapHeight = 0, heightmapChannels;
	string heightmapFilename;
	options.get("-heightmap")->getString(heightmapFilename);
	unsigned char* heightmapData = stbi_load(heightmapFilename.c_str(), &heightmapWidth, &heightmapHeight, &heightmapChannels, 0);

	// Make sure it opened sucessfully
	throwExceptionIf(heightmapData == NULL, FileSystemError("Failed to open or understand heightmap file. Note: Our image loader is limited - if the file exists then try resaving it as a different format."));

	// Open the colormap
	int colormapWidth = 0, colormapHeight = 0, colormapChannels;
	string colormapFilename;
	throwExceptionIf(options.get("-colormap") == 0, OptionsError("No colormap specified."));
	options.get("-colormap")->getString(colormapFilename);
	unsigned char* colormapData = stbi_load(colormapFilename.c_str(), &colormapWidth, &colormapHeight, &colormapChannels, 0);
	throwExceptionIf(colormapData == NULL, FileSystemError("Failed to open or understand colormap file. Note: Our image loader is limited - if the file exists then try resaving it as a different format."));

	if ((heightmapWidth != colormapWidth) || (heightmapHeight != colormapHeight))
	{
		throwException(ParseError("Heightmap and colormap must have same dimensions"));
	}

	// Create the volume. When importing we treat 'y' as up because most game engines and
	// physics engines expect this. This means we need to swap the 'y' and 'slice' indices.
	// Ideally we should allow command line parameters to control this mapping.
	uint32_t volumeHandle;
	uint32_t volumeWidth = heightmapWidth;
	uint32_t volumeHeight = 256; // Assume we're not loading HDR images, not supported by stb_image anyway
	uint32_t volumeDepth = heightmapHeight;
	string pathToVoxelDatabase;
	options.get("-coloredcubes")->getString(pathToVoxelDatabase);
	VALIDATE_CALL(cuNewEmptyColoredCubesVolume(0, 0, 0, volumeWidth - 1, volumeHeight - 1, volumeDepth - 1, pathToVoxelDatabase.c_str(), 32, &volumeHandle))

	CuColor gray = cuMakeColor(63, 63, 63, 255);
	CuColor empty = cuMakeColor(0, 0, 0, 0);

	for (uint32_t imageY = 0; imageY < heightmapHeight; imageY++)
	{
		for (uint32_t imageX = 0; imageX < heightmapWidth; imageX++)
		{
			unsigned char* heightmapPixel = heightmapData + (imageY * heightmapWidth + imageX) * heightmapChannels;
			unsigned char* colormapPixel = colormapData + (imageY * colormapWidth + imageX) * colormapChannels;

			for (uint32_t height = 0; height < volumeHeight; height++)
			{
				CuColor colorToUse;
				if (height < *heightmapPixel)
				{
					colorToUse = gray;
				}
				else if (height == *heightmapPixel)
				{
					colorToUse = cuMakeColor(*(colormapPixel + 0), *(colormapPixel + 1), *(colormapPixel + 2), 255);
				}
				else
				{
					colorToUse = empty;
				}

				// Note: We are flipping Y here to match the Unity3D coordinate system. This no longer matches the
				// OpenGL coordinate system. This flipping shold ideally be controlled by command line switches.
				VALIDATE_CALL(cuSetVoxel(volumeHandle, imageX, height, (heightmapHeight - 1) - imageY, &colorToUse))
			}
		}
	}

	VALIDATE_CALL(cuAcceptOverrideChunks(volumeHandle));
	VALIDATE_CALL(cuDeleteVolume(volumeHandle));
}

void importHeightmapAsTerrainVolume(ez::ezOptionParser& options)
{
	// At this point we know the -heightmap and -terrain flags were set, otherwise we wouldn't be in this function.

	// Open the heightmap
	int originalWidth = 0, originalHeight = 0, originalChannels = 0; // 'Original' values are before resampling
	int originalDepth = 256; // Assume 8-bit input image.
	string heightmapFilename;
	options.get("-heightmap")->getString(heightmapFilename);
	unsigned char* originalData = stbi_load(heightmapFilename.c_str(), &originalWidth, &originalHeight, &originalChannels, 0);

	// Make sure it opened sucessfully
	throwExceptionIf(originalData == NULL, FileSystemError("Failed to open or understand heightmap file. Note: Our image loader is limited - if the file exists then try resaving it as a different format."));

	int originalPixelCount = originalWidth * originalHeight;
	float* floatOriginalData = new float[originalPixelCount];
	for (int y = 0; y < originalHeight; y++)
	{
		for (int x = 0; x < originalWidth; x++)
		{
			floatOriginalData[y * originalWidth + x] = static_cast<float>(originalData[(y * originalWidth + x) * originalChannels]) / 255.0f;
		}
	}

	float scale = 1.0f;
	if (options.get("-scale"))
	{
		options.get("-scale")->getFloat(scale);
	}

	if (scale > 0.5f)
	{
		std::cout << "Warning - We recommend using '-scale' with a value of 0.5 or less when generating a terrain volume from a heightmap." << std::endl;
	}

	int resampledWidth = static_cast<int>((originalWidth * scale) + 0.5f);
	int resampledHeight = static_cast<int>((originalHeight * scale) + 0.5f);
	int resampledDepth = static_cast<int>((originalDepth * scale) + 0.5f);
	int resampledPixelCount = resampledWidth * resampledHeight;
	float* resampledData = new float[resampledPixelCount];

	stbir_resize_float(floatOriginalData, originalWidth, originalHeight, 0,
		resampledData, resampledWidth, resampledHeight, 0, 1);


	uint32_t volumeHandle;
	uint32_t volumeWidth = resampledWidth;
	uint32_t volumeHeight = resampledDepth; // Swapping height and depth 
	uint32_t volumeDepth = resampledHeight; // so that 'y' is 'up'
	string pathToVoxelDatabase;
	options.get("-terrain")->getString(pathToVoxelDatabase);
	VALIDATE_CALL(cuNewEmptyTerrainVolume(0, 0, 0, volumeWidth - 1, volumeHeight - 1, volumeDepth - 1, pathToVoxelDatabase.c_str(), 32, &volumeHandle))

	// Don't yet know the best value for this. Setting it smaller makes the 
	// result smoother but sculpting goes a bit crazy (some kind of wraparound)?
	float scalarFieldCompressionFactor = 1024.0f;

	for (uint32_t imageY = 0; imageY < resampledHeight; imageY++)
	{
		for (uint32_t imageX = 0; imageX < resampledWidth; imageX++)
		{
			float fPixelHeight = resampledData[imageY * resampledWidth + imageX];

			for (uint32_t height = 0; height < volumeHeight; height++)
			{
				CuMaterialSet materialSet;
				materialSet.data = 0;

				float fHeight = float(height) / float(volumeHeight - 1);				

				float fDiff = fPixelHeight - fHeight;

				fDiff *= scalarFieldCompressionFactor;

				int diff = int(fDiff);
				diff += 127;

				diff = (std::min)(diff, 255);
				diff = (std::max)(diff, 0);

				materialSet.data = static_cast<uint8_t>(diff);

				if (materialSet.data > 135)
				{
					materialSet.data <<= 8;
				}

				// Note: We are flipping Y here to match the Unity3D coordinate system. This no longer matches the
				// OpenGL coordinate system. This flipping shold ideally be controlled by command line switches.
				VALIDATE_CALL(cuSetVoxel(volumeHandle, imageX, height, (resampledHeight-1) - imageY, &materialSet))
			}
		}
	}

	VALIDATE_CALL(cuAcceptOverrideChunks(volumeHandle));
	VALIDATE_CALL(cuDeleteVolume(volumeHandle));
}
