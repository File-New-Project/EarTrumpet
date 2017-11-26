

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Sat Nov 25 18:22:15 2017
 */
/* Compiler settings for IEndpointNotificationHandler.idl:
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

#ifndef __IEndpointNotificationHandler_h__
#define __IEndpointNotificationHandler_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IEndpointNotificationHandler_FWD_DEFINED__
#define __IEndpointNotificationHandler_FWD_DEFINED__
typedef interface IEndpointNotificationHandler IEndpointNotificationHandler;

#endif 	/* __IEndpointNotificationHandler_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"
#include "IEndpointNotificationCallback.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __IEndpointNotificationHandler_INTERFACE_DEFINED__
#define __IEndpointNotificationHandler_INTERFACE_DEFINED__

/* interface IEndpointNotificationHandler */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_IEndpointNotificationHandler;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("AAFE7CFC-E840-4C59-B055-9F1D2D01C68F")
    IEndpointNotificationHandler : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE RegisterVolumeChangeHandler( 
            IEndpointNotificationCallback *__MIDL__IEndpointNotificationHandler0000) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IEndpointNotificationHandlerVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IEndpointNotificationHandler * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IEndpointNotificationHandler * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IEndpointNotificationHandler * This);
        
        HRESULT ( STDMETHODCALLTYPE *RegisterVolumeChangeHandler )( 
            IEndpointNotificationHandler * This,
            IEndpointNotificationCallback *__MIDL__IEndpointNotificationHandler0000);
        
        END_INTERFACE
    } IEndpointNotificationHandlerVtbl;

    interface IEndpointNotificationHandler
    {
        CONST_VTBL struct IEndpointNotificationHandlerVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IEndpointNotificationHandler_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IEndpointNotificationHandler_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IEndpointNotificationHandler_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IEndpointNotificationHandler_RegisterVolumeChangeHandler(This,__MIDL__IEndpointNotificationHandler0000)	\
    ( (This)->lpVtbl -> RegisterVolumeChangeHandler(This,__MIDL__IEndpointNotificationHandler0000) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IEndpointNotificationHandler_INTERFACE_DEFINED__ */


/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


