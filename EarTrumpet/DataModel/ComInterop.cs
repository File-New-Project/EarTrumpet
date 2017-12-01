namespace EarTrumpet.DataModel
{

    enum CLSCTX : int
    {
        CLSCTX_INPROC_SERVER = 0x1,
    }

    class ComInterop
    {
        public static bool Succeeded(int hr)
        {
            return hr == 0 || hr == 1;
        }

        public static bool Failed(int hr)
        {
            return !Succeeded(hr);
        }
    }
}
