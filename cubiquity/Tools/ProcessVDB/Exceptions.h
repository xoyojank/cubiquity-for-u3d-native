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

#ifndef CUBIQUITYTOOLS_EXCEPTIONS_H_
#define CUBIQUITYTOOLS_EXCEPTIONS_H_

#include "HeaderOnlyLibs.h" // For logging

#include "CubiquityC.h"

#include <stdexcept>

// Thrown if there is a problem parsing command line options
class OptionsError : public std::runtime_error
{
public:
	OptionsError(const std::string& what_arg)
		:runtime_error(what_arg)
	{
	}
};

// Thrown if there is a problem parsing a file format
class ParseError : public std::runtime_error
{
public:
	ParseError(const std::string& what_arg)
		:runtime_error(what_arg)
	{
	}
};

// Thrown if there is a problem parsing a file format
class FileSystemError : public std::runtime_error
{
public:
	FileSystemError(const std::string& what_arg)
		:runtime_error(what_arg)
	{
	}
};

// Thrown if an error code is returned by Cubiquity.
class CubiquityError : public std::runtime_error
{
public:
	CubiquityError(int32_t errorCode, const std::string& errorMessage)
		:runtime_error(errorMessage)
		,mErrorCode(errorCode)
	{
	}

	int32_t getErrorCode(void)
	{
		return mErrorCode;
	}

	std::string getErrorCodeAsString(void)
	{
		return cuGetErrorCodeAsString(mErrorCode);
	}

	std::string getErrorMessage(void)
	{
		return what();
	}

private:
	int32_t mErrorCode;
};

// We use this utility function for throwing exceptions
// because it lets us log the message at the same time.
template<typename ExceptionType>
void throwException(ExceptionType e)
{
	LOG(ERROR) << e.what();
	throw e;
}

// This version is useful for testing error conditions with minimal
// lines of code, and throwing an exception if the condition fails.
template<typename ExceptionType>
void throwExceptionIf(bool condition, ExceptionType e)
{
	if (condition)
	{
		throwException(e);
	}
}

// I don't think it matters whether or not calls to this macro have
// a terminating ';' or not. Extra semicolons should be harmless.
#define VALIDATE_CALL(functionCall) \
{ \
	int32_t result = functionCall; \
	if (result != CU_OK) \
	{ \
		throwException(CubiquityError(result, cuGetLastErrorMessage())); \
	} \
}

#endif //CUBIQUITYTOOLS_EXCEPTIONS_H_