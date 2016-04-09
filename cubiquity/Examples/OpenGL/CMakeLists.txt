# CMake entry point
cmake_minimum_required (VERSION 2.6)
project (Tutorials)

find_package(OpenGL REQUIRED)


if( CMAKE_BINARY_DIR STREQUAL CMAKE_SOURCE_DIR )
    message( FATAL_ERROR "Please select another Build Directory ! (and give it a clever name, like bin_Visual2012_64bits/)" )
endif()
if( CMAKE_SOURCE_DIR MATCHES " " )
	message( "Your Source Directory contains spaces. If you experience problems when compiling, this can be the cause." )
endif()
if( CMAKE_BINARY_DIR MATCHES " " )
	message( "Your Build Directory contains spaces. If you experience problems when compiling, this can be the cause." )
endif()



# Compile external dependencies 
add_subdirectory (external)

# On Visual 2005 and above, this module can set the debug working directory
#list(APPEND CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/external/rpavlik-cmake-modules-1c73e35")
#include(CreateLaunchers)
#include(MSVCMultipleProcessCompile) # /MP

if(INCLUDE_DISTRIB)
	add_subdirectory(distrib)
endif(INCLUDE_DISTRIB)



include_directories(
	${CubiquityCore_SOURCE_DIR} ${CubiquityC_SOURCE_DIR}
	external/glfw-3.0.3/include/GLFW/
	external/glm-0.9.4.0/
	external/glew-1.9.0/include/
	.
)

set(ALL_LIBS
	${OPENGL_LIBRARY}
	GLFW_303
	GLEW_190
)

add_definitions(
	-DTW_STATIC
	-DTW_NO_LIB_PRAGMA
	-DTW_NO_DIRECT3D
	-DGLEW_STATIC
	-D_CRT_SECURE_NO_WARNINGS
)

# Tutorial 6
add_executable(OpenGLExample
	main.cpp
	shader.cpp
	shader.hpp
	controls.cpp
	controls.hpp
	texture.cpp
	texture.hpp
	
	ColoredCubesVertexShader.glsl
	ColoredCubesFragmentShader.glsl
	TerrainVertexShader.glsl
	TerrainFragmentShader.glsl
)
target_link_libraries(OpenGLExample
	CubiquityC
	${ALL_LIBS}
)
# Xcode and Visual working directories
#set_target_properties(OpenGLExample PROPERTIES XCODE_ATTRIBUTE_CONFIGURATION_BUILD_DIR "${CMAKE_CURRENT_SOURCE_DIR}/")
#create_target_launcher(OpenGLExample WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/")

#SOURCE_GROUP(common REGULAR_EXPRESSION ".*/common/.*" )
SOURCE_GROUP(shaders REGULAR_EXPRESSION ".*/.*shader$" )

if(WIN32) # Perhaps this shouldn't be Windows only?
	# We want to be able to run the examples from within Visual Studio, which means that the shaders/textures need to be in the
	# 'working directory'. Unfortunately we can't set this working directory from CMake (http://goo.gl/Ne5uec), so we copy the
	# resources to the default working directory (which is where the project file is). Perhaps there is a better way to find this?
	file(COPY ColoredCubesVertexShader.glsl DESTINATION ${CMAKE_BINARY_DIR}/Examples/OpenGL)
	file(COPY ColoredCubesFragmentShader.glsl DESTINATION ${CMAKE_BINARY_DIR}/Examples/OpenGL)
	file(COPY TerrainVertexShader.glsl DESTINATION ${CMAKE_BINARY_DIR}/Examples/OpenGL)
	file(COPY TerrainFragmentShader.glsl DESTINATION ${CMAKE_BINARY_DIR}/Examples/OpenGL)
endif()

#if (NOT ${CMAKE_GENERATOR} MATCHES "Xcode" )
#add_custom_command(
#   TARGET OpenGLExample POST_BUILD
#   COMMAND ${CMAKE_COMMAND} -E copy "${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_CFG_INTDIR}/OpenGLExample${CMAKE_EXECUTABLE_SUFFIX}" "${CMAKE_CURRENT_SOURCE_DIR}/"
#)
#elseif (${CMAKE_GENERATOR} MATCHES "Xcode" )

#endif (NOT ${CMAKE_GENERATOR} MATCHES "Xcode" )

if(CMAKE_CXX_COMPILER_ID MATCHES "MSVC")
  # Organise the Visual Studio folders.
  SET_PROPERTY(TARGET OpenGLExample PROPERTY FOLDER "Examples")
  SET_PROPERTY(TARGET GLEW_190 PROPERTY FOLDER "Examples/external")
  SET_PROPERTY(TARGET GLFW_303 PROPERTY FOLDER "Examples/external")
  SET_PROPERTY(TARGET docs PROPERTY FOLDER "Examples/external")
endif()
