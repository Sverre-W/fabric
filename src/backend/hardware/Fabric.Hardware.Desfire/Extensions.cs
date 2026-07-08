using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Protocol;

namespace Fabric.Hardware.Desfire;

public static class Extensions
{
    public static IDesfireResponse<T> AsResponse<T>(this DesfireResponseFrame frame, Func<byte[], T> ifSuccess)
        where T : class
    {
        return !frame.StatusCode.IsSuccess()
            ? DesfireResponse.Create<T>(frame.StatusCode, null)
            : DesfireResponse.Create(frame.StatusCode, ifSuccess(frame.Data));
    }

    public static IDesfireResponse<T> AsResponse<T>(this DesfireResponseFrame frame, Func<byte[], T> ifSuccess, T defaultValue)
        where T : class
    {
        return !frame.StatusCode.IsSuccess()
            ? DesfireResponse.Create(frame.StatusCode, defaultValue)
            : DesfireResponse.Create(frame.StatusCode, ifSuccess(frame.Data));
    }

    public static IDesfireResponse AsNoData(this DesfireResponseFrame frame)
    {
        return DesfireResponse.Create(frame.StatusCode);
    }
}
