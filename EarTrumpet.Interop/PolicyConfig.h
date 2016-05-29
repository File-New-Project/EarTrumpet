#pragma once

interface DECLSPEC_UUID("00000000-0000-0000-C000-000000000046") IPolicyConfig;
class DECLSPEC_UUID("870af99c-171d-4f9e-af0d-e63df40c2bc9") CPolicyConfigClient;

interface IPolicyConfig : public IUnknown
{
public:

	virtual HRESULT STDMETHODCALLTYPE Unused1();
	virtual HRESULT STDMETHODCALLTYPE Unused2();
	virtual HRESULT STDMETHODCALLTYPE Unused3();
	virtual HRESULT STDMETHODCALLTYPE Unused4();
	virtual HRESULT STDMETHODCALLTYPE Unused5();
	virtual HRESULT STDMETHODCALLTYPE Unused6();
	virtual HRESULT STDMETHODCALLTYPE Unused7();
	virtual HRESULT STDMETHODCALLTYPE Unused8();
	virtual HRESULT STDMETHODCALLTYPE Unused9();
	virtual HRESULT STDMETHODCALLTYPE Unused10();

	virtual HRESULT STDMETHODCALLTYPE SetDefaultEndpoint(
		PCWSTR wszDeviceId,
		ERole eRole
	);

	// ...
};