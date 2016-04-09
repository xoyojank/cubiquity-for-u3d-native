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

#include "Import.h"

#include "Exceptions.h"
#include "ImportHeightmap.h"
#include "ImportImageSlices.h"
#include "ImportMagicaVoxel.h"
#include "ImportVXL.h"

#include "CubiquityC.h"

#include <iostream>

using namespace ez;
using namespace std;

void importVDB(ezOptionParser& options)
{
	LOG(INFO) << "Importing voxel database...";

	// Check the filename here, so we can ensure it doesn't already exist.
	string outputFilename;	
	if (options.isSet("-coloredcubes"))
		options.get("-coloredcubes")->getString(outputFilename);
	if (options.isSet("-terrain"))
		options.get("-terrain")->getString(outputFilename);
	
	// If the output file already exists then we need to delete
	// it before we can use the filename for the new volume.
	if (remove(outputFilename.c_str()) == 0)
	{
		LOG(INFO) << "Deleted previous file called \"" << outputFilename << "\"";
	}

	if (options.isSet("-heightmap"))
	{
		importHeightmap(options);
	}
	else if (options.isSet("-imageslices"))
	{
		importImageSlices(options);
	}
	else if (options.isSet("-magicavoxel"))
	{
		importMagicaVoxel(options);
	}
	else if (options.isSet("-vxl"))
	{
		importVxl(options);
	}
	else
	{
		throwException(OptionsError("No valid import format specified."));
	}
}
