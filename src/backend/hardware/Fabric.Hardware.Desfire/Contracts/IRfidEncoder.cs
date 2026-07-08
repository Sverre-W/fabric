namespace Fabric.Hardware.Desfire.Contracts;

/// <summary>
///     Represents an encoder the can take a native desfire command and send it to the reader
/// </summary>
public interface IRfidEncoder : IDisposable
{
    /// <summary>
    ///     The command to be sent
    /// </summary>
    /// <param name="data">The command</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The response data</returns>
    public Task<byte[]> Send(byte[] data, CancellationToken cancellationToken = default);
}
