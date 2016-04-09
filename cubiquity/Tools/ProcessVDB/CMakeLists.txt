################################################################################
# The MIT License (MIT)
#
# Copyright (c) 2016 David Williams and Matthew Williams
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.
################################################################################

project(ProcessVDB)

include_directories(${CubiquityCore_SOURCE_DIR} ${CubiquityC_SOURCE_DIR})

add_executable (ProcessVDB Export.cpp ExportImageSlices.cpp HeaderOnlyLibs.cpp Import.cpp ImportHeightmap.cpp ImportImageSlices.cpp ImportMagicaVoxel.cpp ImportVXL.cpp main.cpp) 

# Set LINK_FLAGS
if(CMAKE_CXX_COMPILER_ID MATCHES "CLANG")
  set_target_properties(ProcessVDB PROPERTIES LINK_FLAGS "-arch i386 -arch x86_64 -static-libstdc++")
elseif(CMAKE_CXX_COMPILER_ID MATCHES "GNU")
  set_target_properties(ProcessVDB PROPERTIES LINK_FLAGS "-static-libstdc++")
endif()

# Set target_link_libraries
if(CMAKE_CXX_COMPILER_ID MATCHES "Clang")
  # Note - It's ugly that we need pthread here. It happens because the include-chain causes 'sqlite3.h' to be included from the application.
  # Ideally 'CubiqutyC.h' should be changed so that it includes nothing but standard C libraries, so we only distribute that one header?
  target_link_libraries (ProcessVDB CubiquityC pthread)
elseif(CMAKE_CXX_COMPILER_ID MATCHES "GNU")
  # Note - It's ugly that we need pthread here. It happens because the include-chain causes 'sqlite3.h' to be included from the application.
  # Ideally 'CubiqutyC.h' should be changed so that it includes nothing but standard C libraries, so we only distribute that one header?
  target_link_libraries (ProcessVDB CubiquityC pthread)
elseif(CMAKE_CXX_COMPILER_ID MATCHES "MSVC")
  target_link_libraries (ProcessVDB CubiquityC)
endif()

# Organise the Visual Studio folders.
SET_PROPERTY(TARGET ProcessVDB PROPERTY FOLDER "Tools")
