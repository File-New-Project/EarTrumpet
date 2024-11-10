namespace Windows.Win32.Foundation;

public readonly partial struct HRESULT
{
    // Missing constants https://github.com/microsoft/win32metadata/issues/1775
    public static readonly HRESULT AUDCLNT_E_DEVICE_INVALIDATED = (HRESULT)unchecked((int)0x88890004);
    public static readonly HRESULT AUDCLNT_S_NO_SINGLE_PROCESS = (HRESULT)unchecked(0x889000d);

    // Missing constants https://github.com/microsoft/CsWin32/issues/1100
    public static readonly HRESULT ERROR_NOT_FOUND = (HRESULT)unchecked((int)0x80070490);
    public static readonly HRESULT S_OK_CONST = (HRESULT)unchecked(0);
    public static readonly HRESULT S_FALSE_CONST = (HRESULT)unchecked(1);
}