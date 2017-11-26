

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Sat Nov 25 18:22:10 2017
 */
/* Compiler settings for IControlChangeCallback.idl:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=X86 8.00.0603 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __IControlChangeCallback_h__
#define __IControlChangeCallback_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IControlChangeCallback_FWD_DEFINED__
#define __IControlChangeCallback_FWD_DEFINED__
typedef interface IControlChangeCallback IControlChangeCallback;

#endif 	/* __IControlChangeCallback_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __IControlChangeCallback_INTERFACE_DEFINED__
#define __IControlChangeCallback_INTERFACE_DEFINED__

/* interface IControlChangeCallback */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_IControlChangeCallback;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("0AC96DA4-337E-4975-ACD7-082E0B85B3C6")
    IControlChangeCallback : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE OnVolumeChanged( 
            LPCWSTR deviceId,
            float volume) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IControlChangeCallbackVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IControlChangeCallback * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IControlChangeCallback * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IControlChangeCallback * This);
        
        HRESULT ( STDMETHODCALLTYPE *OnVolumeChanged )( 
            IControlChangeCallback * This,
            LPCWSTR deviceId,
            float volume);
        
        END_INTERFACE
    } IControlChangeCallbackVtbl;

    interface IControlChangeCallback
    {
        CONST_VTBL struct IControlChangeCallbackVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IControlChangeCallback_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IControlChangeCallback_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IControlChangeCallback_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IControlChangeCallback_OnVolumeChanged(This,deviceId,volume)	\
    ( (This)->lpVtbl -> OnVolumeChanged(This,deviceId,volume) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IControlChangeCallback_INTERFACE_DEFINED__ */


/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


