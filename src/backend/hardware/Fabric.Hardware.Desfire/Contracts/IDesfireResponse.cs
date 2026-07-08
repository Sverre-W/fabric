using Fabric.Hardware.Desfire.Protocol;

namespace Fabric.Hardware.Desfire.Contracts;

public interface IDesfireResponse
{
    DesfireStatusCode StatusCode { get; }

    public bool IsSuccess => StatusCode.IsSuccess();
}

public interface IDesfireResponse<out T> : IDesfireResponse
{
    T? Data { get; }
}
