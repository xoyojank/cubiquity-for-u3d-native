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

cmake_minimum_required(VERSION 2.8)

# Force MSVC to statically link against the runtime.
# http://www.cmake.org/Wiki/CMake_FAQ#Make_Override_Files
set(CMAKE_USER_MAKE_RULES_OVERRIDE
   ${CMAKE_CURRENT_SOURCE_DIR}/c_flag_overrides.cmake)
set(CMAKE_USER_MAKE_RULES_OVERRIDE_CXX
   ${CMAKE_CURRENT_SOURCE_DIR}/cxx_flag_overrides.cmake)
   
project(Cubiquity)

# For keeping things tidy in Visual Studio
SET_PROPERTY(GLOBAL PROPERTY USE_FOLDERS ON)

# To run the examples/tools from Visual Studio we need the built executables and libraries to
# be in the same folder. See http://www.cmake.org/pipermail/cmake/2006-September/011249.html
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

# We set the CXX flags here as 'global' options, and then add to them as required using set_target_properties()
# See: http://stackoverflow.com/questions/5096881/does-set-target-properties-in-cmake-override-cmake-cxx-flags
# Note: Should we be using 'CMAKE_CXX_COMPILE_FLAGS' instead of 'CMAKE_CXX_FLAGS'? See http://goo.gl/rLqUyk
if(CMAKE_CXX_COMPILER_ID MATCHES "Clang")
	# Should look into CMAKE_OSX_ARCHITECTURES for this?
	set(CMAKE_C_FLAGS "-arch i386 -arch x86_64 ${CMAKE_C_FLAGS}")
	set(CMAKE_CXX_FLAGS "-std=c++11 -stdlib=libc++ -arch i386 -arch x86_64 ${CMAKE_CXX_FLAGS}")
elseif(CMAKE_CXX_COMPILER_ID MATCHES "GNU")
	set(CMAKE_C_FLAGS "-fPIC ${CMAKE_C_FLAGS}")
	set(CMAKE_CXX_FLAGS "-fPIC -std=c++0x ${CMAKE_CXX_FLAGS}")
endif()

#Build the various projects
add_subdirectory(Core)
add_subdirectory(Examples/CubiquityCTest)
add_subdirectory(Examples/OpenGL)
add_subdirectory(Tools/ProcessVDB)
