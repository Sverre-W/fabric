using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Protocol;

namespace Fabric.Hardware.Desfire.Models;

public class DesfireResponse : IDesfireResponse
{
    public DesfireResponse(DesfireStatusCode statusCode)
    {
        StatusCode = statusCode;
    }

    public DesfireStatusCode StatusCode { get; }

    public static DesfireResponse Create(DesfireStatusCode statusCode)
    {
        return new DesfireResponse(statusCode);
    }

    public static DesfireResponse<T> Create<T>(DesfireStatusCode statusCode, T? data)
    {
        return new DesfireResponse<T>(statusCode, data);
    }

    public override string ToString()
    {
        return $"[{StatusCode}]";
    }
}

public class DesfireResponse<T> : IDesfireResponse<T>
{
    public DesfireResponse(DesfireStatusCode statusCode, T? data)
    {
        if (statusCode == DesfireStatusCode.Success && data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        StatusCode = statusCode;
        Data = data;
    }

    public DesfireStatusCode StatusCode { get; }
    public T? Data { get; }

    public override string ToString()
    {
        return Data == null ? $"[{StatusCode}]" : $"[{StatusCode}] ({Data})";
    }
}
