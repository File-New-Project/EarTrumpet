#pragma once

// 870AF99C-171D-4F9E-AF0D-E63DF40C2BC9
const CLSID CLSID_PolicyConfigClient    = { 0x870AF99C, 0x171D, 0x4F9E, { 0xAF, 0x0D, 0xE6, 0x3D, 0xF4, 0x0C, 0x2B, 0xC9 } };

// CA286FC3-91FD-42C3-8E9B-CAAFA66242E3
const GUID IID_IPolicyConfig_TH1        = { 0xCA286FC3, 0x91FD, 0x42C3, { 0x8E, 0x9B, 0xCA, 0xAF, 0xA6, 0x62, 0x42, 0xE3 } };

// 6BE54BE8-A068-4875-A49D-0C2966473B11
const GUID IID_IPolicyConfig_TH2        = { 0x6BE54BE8, 0xA068, 0x4875, { 0xA4, 0x9D, 0x0C, 0x29, 0x66, 0x47, 0x3B, 0x11 } };

// F8679F50-850A-41CF-9C72-430F290290C8
const GUID IID_IPolicyConfig_RS1        = { 0xF8679F50, 0x850A, 0x41CF, { 0x9C, 0x72, 0x43, 0x0F, 0x29, 0x02, 0x90, 0xC8 } };

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