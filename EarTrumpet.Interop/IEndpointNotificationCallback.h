

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Sat Nov 25 18:22:14 2017
 */
/* Compiler settings for IEndpointNotificationCallback.idl:
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

#ifndef __IEndpointNotificationCallback_h__
#define __IEndpointNotificationCallback_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IEndpointNotificationCallback_FWD_DEFINED__
#define __IEndpointNotificationCallback_FWD_DEFINED__
typedef interface IEndpointNotificationCallback IEndpointNotificationCallback;

#endif 	/* __IEndpointNotificationCallback_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __IEndpointNotificationCallback_INTERFACE_DEFINED__
#define __IEndpointNotificationCallback_INTERFACE_DEFINED__

/* interface IEndpointNotificationCallback */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_IEndpointNotificationCallback;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("2B278740-717F-4D43-987D-05BA8EAC7943")
    IEndpointNotificationCallback : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE OnVolumeChanged( 
            float volume) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IEndpointNotificationCallbackVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IEndpointNotificationCallback * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IEndpointNotificationCallback * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IEndpointNotificationCallback * This);
        
        HRESULT ( STDMETHODCALLTYPE *OnVolumeChanged )( 
            IEndpointNotificationCallback * This,
            float volume);
        
        END_INTERFACE
    } IEndpointNotificationCallbackVtbl;

    interface IEndpointNotificationCallback
    {
        CONST_VTBL struct IEndpointNotificationCallbackVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IEndpointNotificationCallback_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IEndpointNotificationCallback_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IEndpointNotificationCallback_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IEndpointNotificationCallback_OnVolumeChanged(This,volume)	\
    ( (This)->lpVtbl -> OnVolumeChanged(This,volume) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IEndpointNotificationCallback_INTERFACE_DEFINED__ */


/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


