

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Sat Nov 25 18:22:11 2017
 */
/* Compiler settings for IControlChangeHandler.idl:
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

#ifndef __IControlChangeHandler_h__
#define __IControlChangeHandler_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IControlChangeHandler_FWD_DEFINED__
#define __IControlChangeHandler_FWD_DEFINED__
typedef interface IControlChangeHandler IControlChangeHandler;

#endif 	/* __IControlChangeHandler_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"
#include "IControlChangeCallback.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __IControlChangeHandler_INTERFACE_DEFINED__
#define __IControlChangeHandler_INTERFACE_DEFINED__

/* interface IControlChangeHandler */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_IControlChangeHandler;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("265E0961-8265-46F0-A062-173B8CB3CDC6")
    IControlChangeHandler : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE RegisterVolumeChangedCallback( 
            IControlChangeCallback *callback) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IControlChangeHandlerVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IControlChangeHandler * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IControlChangeHandler * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IControlChangeHandler * This);
        
        HRESULT ( STDMETHODCALLTYPE *RegisterVolumeChangedCallback )( 
            IControlChangeHandler * This,
            IControlChangeCallback *callback);
        
        END_INTERFACE
    } IControlChangeHandlerVtbl;

    interface IControlChangeHandler
    {
        CONST_VTBL struct IControlChangeHandlerVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IControlChangeHandler_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IControlChangeHandler_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IControlChangeHandler_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IControlChangeHandler_RegisterVolumeChangedCallback(This,callback)	\
    ( (This)->lpVtbl -> RegisterVolumeChangedCallback(This,callback) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IControlChangeHandler_INTERFACE_DEFINED__ */


/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


