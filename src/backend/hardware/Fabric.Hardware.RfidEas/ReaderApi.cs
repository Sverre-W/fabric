using System.Runtime.InteropServices;

namespace Fabric.Hardware.RfidEas;

internal static class ReaderApi
{
    private const string PcProxDll = "pcProxAPI.dll";

    static ReaderApi()
    {
        var myPath = new Uri(typeof(ReaderApi).Assembly.Location).LocalPath;
        var myFolder = Path.GetDirectoryName(myPath);

        var subPath =
            Environment.Is64BitProcess
                ? "\\imports64\\"
                : "\\imports32\\";

        LoadLibrary(myFolder + subPath + PcProxDll);
    }


    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibrary(string dllToLoad);

    [DllImport(PcProxDll)]
    public static extern ushort usbConnect();

    [DllImport(PcProxDll)]
    public static extern short GetActDev();

    [DllImport(PcProxDll)]
    public static extern ushort BeepNow(byte iCount, ushort longBeep);

    [DllImport(PcProxDll)]
    public static extern IntPtr getPartNumberString();

    [DllImport(PcProxDll)]
    public static extern int GetLUID();

    [DllImport(PcProxDll)]
    public static extern short GetDevCnt();

    [DllImport(PcProxDll)]
    public static extern IntPtr GetVidPidVendorName();

    [DllImport(PcProxDll)]
    public static extern ushort SetActDev(short iNdx);

    [DllImport(PcProxDll)]
    public static extern ushort GetLibVersion(ref short major, ref short minor, ref short ver);

    [DllImport(PcProxDll)]
    public static extern ushort GetActiveID32(IntPtr result1, short buffSize);

    [DllImport(PcProxDll)]
    public static extern ushort SetDevTypeSrch(short iSrchType);
}
