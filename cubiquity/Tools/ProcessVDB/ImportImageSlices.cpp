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

#include "ImportImageSlices.h"

#include "Exceptions.h"
#include "HeaderOnlyLibs.h"

#include "CubiquityC.h"

#include <iomanip>
#include <iostream>
#include <cstdint>
#include <sstream>
#include <vector>

using namespace std;

std::vector<std::string> findImagesInFolder(std::string folder)
{
	if((folder.back() != '/') && (folder.back() != '\\'))
	{
		folder.append("/");
	}

	// Slightly hacky way to initialise vector: http://stackoverflow.com/a/8906577
	std::string extsArray[] = { "png", "PNG", "jpg", "JPG", "jpeg", "JPEG", "bmp", "BMP" };
	std::vector<std::string> extensions(std::begin(extsArray), std::end(extsArray));

	// Identify all relevant images
	uint32_t image = 0;
	bool foundImage = false;
	std::vector<std::string> imageFilenames;
	do
	{
		foundImage = false;

		// Note: The number of leading zeros may not be the same for all files. e.g. 009.png and 010.png.
		// The total number of chaachters may also be different - e.g. 1.png and 324.png.
		for(uint32_t leadingZeros = 0; (leadingZeros < 10) && (foundImage == false) ; leadingZeros++)
		{
			std::stringstream filenameBase;
			filenameBase << std::setfill('0') << std::setw(leadingZeros) << image;

			//for(std::string ext : extensions)
			for(auto extIter = extensions.begin(); extIter != extensions.end(); extIter++)
			{
				std::string filename = folder + filenameBase.str() + "." + *(extIter);

				// Check whether the file exists
				FILE* fp;
				fp = fopen(filename.c_str(), "rb");
				if(fp)
				{
					imageFilenames.push_back(filename);
					foundImage = true;
					fclose(fp);
					break;
				}
			}
		}

		image++;
	} while(foundImage);

	return imageFilenames;
}

void importImageSlices(ez::ezOptionParser& options)
{
	LOG(INFO) << "Importing from image slices...";

	// We know the -imageslices flag was set or we wouldn't be
	// in this function. Must check the -coloredcubes flag though.
	throwExceptionIf(!options.get("-coloredcubes"), OptionsError("-coloredcubes flag not found"));

	string folder;
	options.get("-imageslices")->getString(folder);
	string pathToVoxelDatabase;
	options.get("-coloredcubes")->getString(pathToVoxelDatabase);

	LOG(INFO) << "Importing images from '" << folder << "' and into '" << pathToVoxelDatabase << "'";

	std::vector<std::string> imageFilenames = findImagesInFolder(folder);

	// Make sure at least one image was found.
	uint32_t sliceCount = imageFilenames.size();
	LOG(INFO) << "Found " << sliceCount << " images for import" << endl;
	throwExceptionIf(sliceCount == 0, FileSystemError("No images found in provided folder"));	

	// Open the first image to determine the width and height
	int volumeWidth = 0, volumeHeight = 0, noOfChannels;
	unsigned char *sliceData = stbi_load((*(imageFilenames.begin())).c_str(), &volumeWidth, &volumeHeight, &noOfChannels, 0);
	throwExceptionIf(sliceData == NULL, FileSystemError("Failed to open first image"));

	//Close it straight away - we only wanted to find the dimensions.
	stbi_image_free(sliceData);

	// Create the volume. When importing we treat 'y' as up because most game engines and
	// physics engines expect this. This means we need to swap the 'y' and 'slice' indices.
	uint32_t volumeHandle;
	VALIDATE_CALL(cuNewEmptyColoredCubesVolume(0, 0, 0, volumeWidth - 1, sliceCount - 1, volumeHeight - 1, pathToVoxelDatabase.c_str(), 32, &volumeHandle))

	// Now iterate over each slice and import the data.
	for(int slice = 0; slice < sliceCount; slice++)
	{
		LOG(INFO) << "Importing image " << slice << endl;
		int imageWidth = 0, imageHeight = 0, imageChannels;
		unsigned char *sliceData = stbi_load(imageFilenames[slice].c_str(), &imageWidth, &imageHeight, &imageChannels, 0);

		if((imageWidth != volumeWidth) || (imageHeight != volumeHeight))
		{
			throwException(ParseError("All images must have the same dimensions!"));
		}

		// When building a colored cubes volume we need an alpha channel in the images
		// or we do not know what to write into the alpha channel of the volume.
		throwExceptionIf(imageChannels != 4, ParseError("Image must be in RGBA format (four channels)"));

		// Now iterate over each pixel.
		for(int x = 0; x < imageWidth; x++)
		{
			for(int y = 0; y < imageHeight; y++)
			{
				unsigned char *pixelData = sliceData + (y * imageWidth + x) * imageChannels;

				CuColor color = cuMakeColor(*(pixelData + 0), *(pixelData + 1), *(pixelData + 2), *(pixelData + 3));

				// When importing we treat 'y' as up because most game engines and physics
				// engines expect this. This means we need to swap the 'y' and 'slice' indices.
				VALIDATE_CALL(cuSetVoxel(volumeHandle, x, slice, y, &color))
			}
		}

		stbi_image_free(sliceData);
	}

	//volume->markAsModified(volume->getEnclosingRegion(), UpdatePriorities::Background);

	VALIDATE_CALL(cuAcceptOverrideChunks(volumeHandle));
	VALIDATE_CALL(cuDeleteVolume(volumeHandle));
}
