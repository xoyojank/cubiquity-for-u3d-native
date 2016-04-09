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

#ifndef CUBIQUITYTOOLS_HEADERONLYLIBS_H_
#define CUBIQUITYTOOLS_HEADERONLYLIBS_H_

// Note: Including this file is pretty bad for compile tiimes!
// Maybe consider splittig it up if that becaomes a problem.

// We make use of several header-only external libraries in this tool, and it seems they
// can be sensitive to the exact include order. We also sometimes need to define certain
// constants exactly once per program. We take tare of this in this HeaderOnlyLibs.h/cpp

#include "Dependencies/ezOptionParser.hpp" // Needs to go before 'easylogging++' od compile errors result.

#define ELPP_NO_DEFAULT_LOG_FILE
#include "Dependencies/easylogging++.h"

#include "Dependencies/stb_image.h"
#include "Dependencies/stb_image_resize.h"
#include "Dependencies/stb_image_write.h"

#endif //CUBIQUITYTOOLS_HEADERONLYLIBS_H_