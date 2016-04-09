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

#include "ImportVXL.h"

#include "Exceptions.h"

#include "CubiquityC.h"

#include <iostream>

using namespace std;

void importVxl(ez::ezOptionParser& options)
{
	LOG(INFO) << "Importing from VXL...";

	string vxlFilename;
	options.get("-vxl")->getString(vxlFilename);
	string pathToVoxelDatabase;
	options.get("-coloredcubes")->getString(pathToVoxelDatabase);

	cout << "Importing vxl from '" << vxlFilename << "' and into '" << pathToVoxelDatabase << "'";

	FILE* inputFile = fopen(vxlFilename.c_str(), "rb");
	throwExceptionIf(inputFile == NULL, FileSystemError("Failed to open input file."));

	// Determine input file's size.
	fseek(inputFile, 0, SEEK_END);
	long fileSize = ftell(inputFile);
	fseek(inputFile, 0, SEEK_SET);

	uint8_t* data = new uint8_t[fileSize];
	long bytesRead = fread(data, sizeof(uint8_t), fileSize, inputFile);
	fclose(inputFile);
	throwExceptionIf(fileSize != bytesRead, FileSystemError("Failed to read input file."));

	uint32_t volumeHandle = 1000000; // Better if handles were ints so they could be set invalid?
	VALIDATE_CALL(cuNewEmptyColoredCubesVolume(0, 0, 0, 511, 63, 511, pathToVoxelDatabase.c_str(), 32, &volumeHandle))

	uint8_t N, S, E, A, K, Z, M, colorI, zz, runlength, j, red, green, blue;

	int p;

	int i = 0;
	int x = 0;
	int y = 0;
	int columnI = 0;
	int mapSize = 512;
	int columnCount = mapSize * mapSize;
	while (columnI < columnCount)
	{
		// Bounds check before accessing data[...], useful incase we don't actually have a valid VXL file (e.g. in dry-run mode)
		if((i + 3) >= fileSize)
		{
			throwException(ParseError("Error parsing VXL file."));
		}

		// i = span start byte
		N = data[i];
		S = data[i + 1];
		E = data[i + 2];
		A = data[i + 3];
		K = E - S + 1;
		if (N == 0)
		{
			Z = 0;
			M = 64;
		} else
		{
			Z = (N-1) - K;

			// Bounds check before accessing data[...], useful incase we don't actually have a valid VXL file (e.g. in dry-run mode)
			if((i + N * 4 + 3) >= fileSize)
			{
				throwException(ParseError("Error parsing VXL file."));
			}

			// A of the next span
			M = data[i + N * 4 + 3];
		}
		colorI = 0;
		for (p = 0; p < 2; p++)
		{
			// BEWARE: COORDINATE SYSTEM TRANSFORMATIONS MAY BE NEEDED
			// Get top run of colors
			if (p == 0)
			{
				zz = S;
				runlength = K;
			} else
			{
				// Get bottom run of colors
				zz = M - Z;
				runlength = Z;
			}
			for (j = 0; j < runlength; j++)
			{
				// Bounds check before accessing data[...], useful incase we don't actually have a valid VXL file (e.g. in dry-run mode)
				if((i + 6 + colorI * 4) >= fileSize)
				{
					throwException(ParseError("Error parsing VXL file."));
				}

				red = data[i + 6 + colorI * 4];
				green = data[i + 5 + colorI * 4];
				blue = data[i + 4 + colorI * 4];
				// Do something with these colors
				//makeVoxelColorful(x, y, zz, red, green, blue);
				CuColor color = cuMakeColor(red, green, blue, 255);
				VALIDATE_CALL(cuSetVoxel(volumeHandle, x, 63 - zz, y, &color))

				zz++;
				colorI++;
			}
		}
		// Now deal with solid non-surface voxels
		// No color data is provided for non-surface voxels
		zz = E + 1;
		runlength = M - Z - zz;
		for (j = 0; j < runlength; j++)
		{
			//makeVoxelSolid(x, y, zz);
			CuColor color = cuMakeColor(127, 127, 127, 255);
			VALIDATE_CALL(cuSetVoxel(volumeHandle, x, 63 - zz, y, &color))

			zz++;
		}
		if (N == 0)
		{
			columnI++;
			x++;
			if (x >= mapSize)
			{
				x = 0;
				y++;
			}
			i += 4*(1 + K);
		}
		else
		{
			i += N * 4;
		}
	}

	VALIDATE_CALL(cuAcceptOverrideChunks(volumeHandle));
	VALIDATE_CALL(cuDeleteVolume(volumeHandle));
}