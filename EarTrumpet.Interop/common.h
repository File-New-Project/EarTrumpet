#pragma once
#include <windows.h>

//
// Silence buffer overrun warnings for SDK content
// https://wpdev.uservoice.com/forums/110705/suggestions/20550229
//

#pragma warning(push)
#pragma warning(disable: 6386)

#include <atlbase.h>
#include <atlstr.h>
#include <atlcom.h>
#include <strsafe.h>

#pragma warning(pop)

#include <string>
#include <memory>
#include <vector>
#include <array>
#include <map>
#include <thread>
#include <functional>

#define FAST_FAIL(x) { HRESULT __hr = x; if (FAILED(__hr)) { return __hr; } }
#define FAST_FAIL_BUFFER(x) { long __rc = x; if (__rc != ERROR_INSUFFICIENT_BUFFER) { return HRESULT_FROM_WIN32(__rc); } }
#define FAST_FAIL_WIN32(x) { long __rc = x; if (__rc < 0) { return HRESULT_FROM_WIN32(__rc); } }
#define FAST_FAIL_HANDLE(x) { if(x == INVALID_HANDLE_VALUE) { return HRESULT_FROM_WIN32(GetLastError()); }}