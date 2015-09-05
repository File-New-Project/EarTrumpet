#pragma once

#include <windows.h>

#include <atlbase.h>
#include <atlstr.h>
#include <strsafe.h>

#include <string>
#include <memory>
#include <vector>
#include <array>
#include <map>

#define FAST_FAIL(x) { HRESULT __hr = x; if (FAILED(__hr)) { return __hr; } }
#define FAST_FAIL_BUFFER(x) { long __rc = x; if (__rc != ERROR_INSUFFICIENT_BUFFER) { return HRESULT_FROM_WIN32(__rc); } }
#define FAST_FAIL_WIN32(x) { long __rc = x; if (__rc < 0) { return HRESULT_FROM_WIN32(__rc); } }
#define FAST_FAIL_HANDLE(x) { if(x == INVALID_HANDLE_VALUE) { return HRESULT_FROM_WIN32(GetLastError()); }}