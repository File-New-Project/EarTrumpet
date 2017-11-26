

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Sat Nov 25 18:22:13 2017
 */
/* Compiler settings for IEarTrumpetVolumeCallback.idl:
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

#ifndef __IEarTrumpetVolumeCallback_h__
#define __IEarTrumpetVolumeCallback_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IEarTrumpetVolumeCallback_FWD_DEFINED__
#define __IEarTrumpetVolumeCallback_FWD_DEFINED__
typedef interface IEarTrumpetVolumeCallback IEarTrumpetVolumeCallback;

#endif 	/* __IEarTrumpetVolumeCallback_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __IEarTrumpetVolumeCallback_INTERFACE_DEFINED__
#define __IEarTrumpetVolumeCallback_INTERFACE_DEFINED__

/* interface IEarTrumpetVolumeCallback */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_IEarTrumpetVolumeCallback;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("68CDB936-6903-48E5-BB36-7EF434F28B61")
    IEarTrumpetVolumeCallback : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE OnVolumeChanged( 
            float volume) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IEarTrumpetVolumeCallbackVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IEarTrumpetVolumeCallback * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IEarTrumpetVolumeCallback * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IEarTrumpetVolumeCallback * This);
        
        HRESULT ( STDMETHODCALLTYPE *OnVolumeChanged )( 
            IEarTrumpetVolumeCallback * This,
            float volume);
        
        END_INTERFACE
    } IEarTrumpetVolumeCallbackVtbl;

    interface IEarTrumpetVolumeCallback
    {
        CONST_VTBL struct IEarTrumpetVolumeCallbackVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IEarTrumpetVolumeCallback_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IEarTrumpetVolumeCallback_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IEarTrumpetVolumeCallback_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IEarTrumpetVolumeCallback_OnVolumeChanged(This,volume)	\
    ( (This)->lpVtbl -> OnVolumeChanged(This,volume) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IEarTrumpetVolumeCallback_INTERFACE_DEFINED__ */


/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


