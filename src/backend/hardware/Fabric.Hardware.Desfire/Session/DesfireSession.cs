using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Protocol;
using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.Desfire.Session;

public interface IDesfireSession
{
    public byte KeyId { get; }

    public KeyType KeyType { get; }

    public Task<DesfireResponseFrame> SendCommand(DesfireCommandFrame command, CancellationToken ct = default);
}

public abstract class DesfireSession : IDesfireSession
{
    private static readonly bool TraceApdu = string.Equals(
        Environment.GetEnvironmentVariable("SMARTACCESS_TRACE_APDU"),
        "1",
        StringComparison.OrdinalIgnoreCase
    );

    private readonly ILogger _logger;
    private readonly IRfidEncoder _cardEncoder;

    protected DesfireSession(ILogger logger, IRfidEncoder cardEncoder)
    {
        _logger = logger;
        _cardEncoder = cardEncoder;
    }

    public abstract KeyType KeyType { get; }

    public async Task<DesfireResponseFrame> SendCommand(DesfireCommandFrame command, CancellationToken ct = default)
    {
        byte[] fullCommand = command.CommunicationMode switch
        {
            CommunicationMode.Plain => PreProcessPlain(command),
            CommunicationMode.Cmac => PreProcessCmaced(command),
            CommunicationMode.Enciphered => PreProcessEncrypt(command),
            _ => throw new ArgumentException(nameof(command.CommunicationMode)),
        };

        if (TraceApdu || _logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("DESFIRE >> {Command}", Convert.ToHexString(fullCommand));
        }

        byte[] response = await _cardEncoder.Send(fullCommand, ct);

        if (response.Length == 0)
        {
            throw new InvalidOperationException("Empty response received from DESFire encoder");
        }

        if (TraceApdu || _logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("DESFIRE << {Response}", Convert.ToHexString(response));
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Full response received from Send: {response}", Convert.ToHexString(response));
        }

        DesfireStatusCode statusCodeCode = (DesfireStatusCode)response[0];

        //There are certain command that do not need postprocessing (for example change key will return a result nothing else since the authentication is lost)
        byte[] data = response.Length == 1 ? response : PostProcess(command.ResponseCommunicationMode, response, command.ExpectedLength);
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("statusCodeCode: {code}, data: {data}, Data will be: {data1}",
                statusCodeCode, Convert.ToHexString(data), Convert.ToHexString(data.AsSpan(1)));
        }

        return new DesfireResponseFrame
        {
            Data = data[1..], //Skip Status Code from response data
            StatusCode = statusCodeCode,
        };
    }

    public byte KeyId { get; protected init; }
    protected abstract byte[] PreProcessEncrypt(DesfireCommandFrame command);

    protected abstract byte[] PreProcessCmaced(DesfireCommandFrame command);

    protected abstract byte[] PreProcessPlain(DesfireCommandFrame command);

    protected abstract byte[] PostProcess(CommunicationMode mode, byte[] data, int? length = 0);
}
