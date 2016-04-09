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

#include "main.h"

#include "Exceptions.h"
#include "Export.h"
#include "HeaderOnlyLibs.h"
#include "Import.h"

#include "CubiquityC.h"

#include <cstdio>
#include <iostream>
#include <string>

using namespace ez;
using namespace std;

// Sample command lines
// -import -heightmap C:\code\cubiquity\Data\Heightmaps\height.png -terrain C:\code\cubiquity\Data\exported_volume.vdb -scale 0.25
// -import -imageslices C:\code\cubiquity\Data\ImageSlices\VoxeliensTerrain -coloredcubes C:\code\cubiquity\Data\exported_volume.vdb
// -import -magicavoxel C:\code\cubiquity\Data\MagicaVoxel\scene_store3.vox -coloredcubes C:\code\cubiquity\Data\exported_volume.vdb
// -import -vxl C:\code\cubiquity\Data\VXL\RealisticBridge.vxl -coloredcubes C:\code\cubiquity\Data\exported_volume.vdb
//
// -export -coloredcubes C:\code\cubiquity\Data\exported_volume.vdb -imageslices C:\code\cubiquity\Data\ImageSlices\ExportedVolume

int main(int argc, const char* argv[])
{
	START_EASYLOGGINGPP(argc, argv);

	try
	{
		// Configure our logging library.
		el::Configurations defaultConf;
		defaultConf.setToDefault();
		defaultConf.set(el::Level::Global, el::ConfigurationType::Format, "[%datetime{%H:%m:%s}, %level]: %msg");
		defaultConf.set(el::Level::Global, el::ConfigurationType::ToFile, "false"); // See also ELPP_NO_DEFAULT_LOG_FILE
		el::Loggers::reconfigureLogger("default", defaultConf);

		// We have to declare here all options which we might later want to check for. Unfortunaltly ezOptionParser does not support 'increamental',
		// parsing which would let us for example reparse the command line looking for an input format once we had establised that we wanted to import.
		// Format for adding an option is:
		// 
		//   options.add(Default, Required?, Number of args expected, Delimiter if expecting multiple args, Help description, Flag token, Flag token);

		ezOptionParser options;

		// Help/usage
		options.add("", 0, 0, 0, "Display usage instructions.", "-h", "-help", "--help", "--usage");

		// Mode of operation
		options.add("", 0, 0, 0, "Import volume data.", "-import", "--import");
		options.add("", 0, 0, 0, "Export volume data.", "-export", "--export");

		// Recognised data formats
		options.add("", 0, 1, 0, "A grayscale image representing a heightmap.", "-heightmap", "--heightmap");
		options.add("", 0, 1, 0, "A color image corresponding to a heightmap.", "-colormap", "--colormap");
		options.add("", 0, 1, 0, "A folder containing a series of images representing slices through the volume.", "-imageslices", "--imageslices");
		options.add("", 0, 1, 0, "The format used by the MagicaVoxel modelling application.", "-magicavoxel", "--magicavoxel");
		options.add("", 0, 1, 0, "The format used by the game 'Build and Shoot', and possibly other games built on the 'Voxlap' engine.", "-vxl", "--vxl");

		// Volume formats
		options.add("", 0, 1, 0, "A volume consisting of colored cubes.", "-coloredcubes", "--coloredcubes");
		options.add("", 0, 1, 0, "A volume representing a terrain with each voxel being a 'MaterialSet'.", "-terrain", "--terrain");

		// Other parameters.
		options.add("1.0", 0, 1, 0, "Scaling factor.", "-scale", "--scale");

		options.parse(argc, argv);
		if (options.isSet("-h"))
		{
			// We don't use exOptionParser's built-in functionality for generating help text because our options are
			// too complex. Also, we have see it crashing on VS 2013 (http://sourceforge.net/p/ezoptionparser/bugs/3/).
			// Therefore we just print a simple error mesage telling the user to consult the manual for instructions.
			std::cout << std::endl;
			std::cout << "ProcessVDB is a tool for performing various operations on voxel databases" << std::endl;
			std::cout << "Please see the Cubiquity user manual for options and examples." << std::endl;
			std::cout << std::endl;
			return EXIT_SUCCESS;
		}
		else if (options.isSet("--import"))
		{
			importVDB(options);
		}
		else if (options.isSet("--export"))
		{
			exportVDB(options);
		}
		else
		{
			throwException(OptionsError("No mode of operation has been set."));
		}
	}
	catch (CubiquityError& e)
	{
		LOG(ERROR) << "An error occured inside the Cubiquity library:";
		LOG(ERROR) << "\tError code: " << e.getErrorCode() << " (" << e.getErrorCodeAsString() << ")";
		LOG(ERROR) << "\tError message: " << e.getErrorMessage();
		return EXIT_FAILURE;
	}
	catch (OptionsError& e)
	{
		LOG(ERROR) << "There is a problem with the provided program options:";
		LOG(ERROR) << "\t" << e.what();
		LOG(ERROR) << "Please see the user manual for information on how to format the command line.";
		return EXIT_FAILURE;
	}
	catch (const std::exception& e)
	{
		LOG(ERROR) << "Unhandled exception:";
		LOG(ERROR) << "\t" << e.what();
		return EXIT_FAILURE;
	}
	catch (...)
	{
		LOG(ERROR) << "Unhandled exception caught by catch-all handler.";
		return EXIT_FAILURE;
	}

	LOG(INFO) << "Exiting normally.";
	return EXIT_SUCCESS;
}
