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

#include "ImportMagicaVoxel.h"

#include "Exceptions.h"

#include "Dependencies/MagicaVoxelModel.h"

#include "CubiquityC.h"

#include <iostream>

using namespace std;

// Code from here: https://voxel.codeplex.com/wikipage?title=Sample%20Codes
const unsigned int mv_default_palette[ 256 ] = {
    0x00000000, 0xffffffff, 0xffccffff, 0xff99ffff, 0xff66ffff, 0xff33ffff, 0xff00ffff, 0xffffccff, 0xffccccff, 0xff99ccff, 0xff66ccff, 0xff33ccff, 0xff00ccff, 0xffff99ff, 0xffcc99ff, 0xff9999ff,
    0xff6699ff, 0xff3399ff, 0xff0099ff, 0xffff66ff, 0xffcc66ff, 0xff9966ff, 0xff6666ff, 0xff3366ff, 0xff0066ff, 0xffff33ff, 0xffcc33ff, 0xff9933ff, 0xff6633ff, 0xff3333ff, 0xff0033ff, 0xffff00ff,
    0xffcc00ff, 0xff9900ff, 0xff6600ff, 0xff3300ff, 0xff0000ff, 0xffffffcc, 0xffccffcc, 0xff99ffcc, 0xff66ffcc, 0xff33ffcc, 0xff00ffcc, 0xffffcccc, 0xffcccccc, 0xff99cccc, 0xff66cccc, 0xff33cccc,
    0xff00cccc, 0xffff99cc, 0xffcc99cc, 0xff9999cc, 0xff6699cc, 0xff3399cc, 0xff0099cc, 0xffff66cc, 0xffcc66cc, 0xff9966cc, 0xff6666cc, 0xff3366cc, 0xff0066cc, 0xffff33cc, 0xffcc33cc, 0xff9933cc,
    0xff6633cc, 0xff3333cc, 0xff0033cc, 0xffff00cc, 0xffcc00cc, 0xff9900cc, 0xff6600cc, 0xff3300cc, 0xff0000cc, 0xffffff99, 0xffccff99, 0xff99ff99, 0xff66ff99, 0xff33ff99, 0xff00ff99, 0xffffcc99,
    0xffcccc99, 0xff99cc99, 0xff66cc99, 0xff33cc99, 0xff00cc99, 0xffff9999, 0xffcc9999, 0xff999999, 0xff669999, 0xff339999, 0xff009999, 0xffff6699, 0xffcc6699, 0xff996699, 0xff666699, 0xff336699,
    0xff006699, 0xffff3399, 0xffcc3399, 0xff993399, 0xff663399, 0xff333399, 0xff003399, 0xffff0099, 0xffcc0099, 0xff990099, 0xff660099, 0xff330099, 0xff000099, 0xffffff66, 0xffccff66, 0xff99ff66,
    0xff66ff66, 0xff33ff66, 0xff00ff66, 0xffffcc66, 0xffcccc66, 0xff99cc66, 0xff66cc66, 0xff33cc66, 0xff00cc66, 0xffff9966, 0xffcc9966, 0xff999966, 0xff669966, 0xff339966, 0xff009966, 0xffff6666,
    0xffcc6666, 0xff996666, 0xff666666, 0xff336666, 0xff006666, 0xffff3366, 0xffcc3366, 0xff993366, 0xff663366, 0xff333366, 0xff003366, 0xffff0066, 0xffcc0066, 0xff990066, 0xff660066, 0xff330066,
    0xff000066, 0xffffff33, 0xffccff33, 0xff99ff33, 0xff66ff33, 0xff33ff33, 0xff00ff33, 0xffffcc33, 0xffcccc33, 0xff99cc33, 0xff66cc33, 0xff33cc33, 0xff00cc33, 0xffff9933, 0xffcc9933, 0xff999933,
    0xff669933, 0xff339933, 0xff009933, 0xffff6633, 0xffcc6633, 0xff996633, 0xff666633, 0xff336633, 0xff006633, 0xffff3333, 0xffcc3333, 0xff993333, 0xff663333, 0xff333333, 0xff003333, 0xffff0033,
    0xffcc0033, 0xff990033, 0xff660033, 0xff330033, 0xff000033, 0xffffff00, 0xffccff00, 0xff99ff00, 0xff66ff00, 0xff33ff00, 0xff00ff00, 0xffffcc00, 0xffcccc00, 0xff99cc00, 0xff66cc00, 0xff33cc00,
    0xff00cc00, 0xffff9900, 0xffcc9900, 0xff999900, 0xff669900, 0xff339900, 0xff009900, 0xffff6600, 0xffcc6600, 0xff996600, 0xff666600, 0xff336600, 0xff006600, 0xffff3300, 0xffcc3300, 0xff993300,
    0xff663300, 0xff333300, 0xff003300, 0xffff0000, 0xffcc0000, 0xff990000, 0xff660000, 0xff330000, 0xff0000ee, 0xff0000dd, 0xff0000bb, 0xff0000aa, 0xff000088, 0xff000077, 0xff000055, 0xff000044,
    0xff000022, 0xff000011, 0xff00ee00, 0xff00dd00, 0xff00bb00, 0xff00aa00, 0xff008800, 0xff007700, 0xff005500, 0xff004400, 0xff002200, 0xff001100, 0xffee0000, 0xffdd0000, 0xffbb0000, 0xffaa0000,
    0xff880000, 0xff770000, 0xff550000, 0xff440000, 0xff220000, 0xff110000, 0xffeeeeee, 0xffdddddd, 0xffbbbbbb, 0xffaaaaaa, 0xff888888, 0xff777777, 0xff555555, 0xff444444, 0xff222222, 0xff111111
};

int ReadInt( FILE *fp )
{
    int v = 0;
    fread( &v, 4, 1, fp );
    return v;
}

// This function has been derived from the official Magica Voxel import
// code: https://voxel.codeplex.com/wikipage?title=Sample%20Codes
bool isMagicaVoxel(const std::string& filename)
{
    FILE *fp = fopen(filename.c_str(), "rb");
	if (!fp)
	{
        return false;
    }

	const int MV_VERSION = 150;
        
    const int ID_VOX  = MV_ID( 'V', 'O', 'X', ' ' );
    const int ID_MAIN = MV_ID( 'M', 'A', 'I', 'N' );
    const int ID_SIZE = MV_ID( 'S', 'I', 'Z', 'E' );
    const int ID_XYZI = MV_ID( 'X', 'Y', 'Z', 'I' );
    const int ID_RGBA = MV_ID( 'R', 'G', 'B', 'A' );
       
    // magic number
	int magic;
	fread(&magic, 4, 1, fp);
    if ( magic != ID_VOX )
	{
        // Magic number does not match
        return false;
    }
        
    // version
    int version;
	fread(&version, 4, 1, fp);
    if (version != MV_VERSION)
	{
        // Version does not match
        return false;
    }

	fclose(fp);

	return true;
}

void importMagicaVoxel(ez::ezOptionParser& options)
{
	LOG(INFO) << "Importing from Magica Voxel...";

	// We know the -magicavoxel flag was set or we wouldn't be
	// in this function. Must check the -coloredcubes flag though.
	throwExceptionIf(!options.get("-coloredcubes"), OptionsError("-coloredcubes flag not found"));

	string filename;
	options.get("-magicavoxel")->getString(filename);
	string pathToVoxelDatabase;
	options.get("-coloredcubes")->getString(pathToVoxelDatabase);

	LOG(INFO) << "Importing MagicaVoxel from '" << filename << "' and into '" << pathToVoxelDatabase << "'";

	MV_Model model;
	bool result = model.LoadModel(filename.c_str());
	throwExceptionIf(!result, ParseError("Failed to load Magica Voxel file."));

	uint32_t volumeHandle;
	// NOTE: Y/Z axis swapped to better match Magica to Cubiquity.
	VALIDATE_CALL(cuNewEmptyColoredCubesVolume(0, 0, 0, model.sizex - 1, model.sizez - 1, model.sizey - 1, pathToVoxelDatabase.c_str(), 32, &volumeHandle))

	for(int i = 0; i < model.numVoxels; i++)
	{
		MV_RGBA rgba;
		if(model.isCustomPalette)
		{
			rgba = model.palette[model.voxels[i].colorIndex];
		}
		else
		{
			unsigned int color = mv_default_palette[model.voxels[i].colorIndex];
			rgba.a = (color >> 24) & 0xFF;
			rgba.b = (color >> 16) & 0xFF;
			rgba.g = (color >> 8) & 0xFF;
			rgba.r = (color >> 0) & 0xFF;
		}

		CuColor color = cuMakeColor(rgba.r, rgba.g, rgba.b, rgba.a);

		// NOTE: Y/Z axis swapped to better match Magica to Cubiquity.
		VALIDATE_CALL(cuSetVoxel(volumeHandle, model.voxels[i].x, model.voxels[i].z, model.voxels[i].y, &color))
	}

	VALIDATE_CALL(cuAcceptOverrideChunks(volumeHandle));
	VALIDATE_CALL(cuDeleteVolume(volumeHandle));
}
