#pragma once

namespace EarTrumpet
{
    namespace Interop
    {
        MIDL_INTERFACE("130A2F65-2BE7-4309-9A58-A9052FF2B61C")
        IMrtResourceManager : public IUnknown
        {
        public:
			STDMETHOD(Initialize());
			STDMETHOD(InitializeForCurrentApplication)() = 0;
			STDMETHOD(InitializeForPackage)(LPWSTR) = 0;
			STDMETHOD(InitializeForFile)(/* ... */) = 0;
			STDMETHOD(GetMainResourceMap)(GUID const &, void **) = 0;
            // ...
        };

        class DECLSPEC_UUID("DBCE7E40-7345-439D-B12C-114A11819A09") MrtResourceManager;

        MIDL_INTERFACE("6E21E72B-B9B0-42AE-A686-983CF784EDCD")
        IResourceMap : public IUnknown
        {
        public:
			STDMETHOD(GetUri)(/* ... */) = 0;
			STDMETHOD(GetSubtree)(/* ... */) = 0;
			STDMETHOD(GetString)(/* ... */) = 0;
			STDMETHOD(GetStringForContext)(/* ... */) = 0;
			STDMETHOD(GetFilePath)(LPWSTR, LPWSTR*) = 0;
            // ...
        };
    }
}