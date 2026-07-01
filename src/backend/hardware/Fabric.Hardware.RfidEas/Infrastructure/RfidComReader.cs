using System.IO.Ports;
using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.RfidEas.Infrastructure;

/// <summary>
///     An SmartAccess.Kiosk.Hardware.Rfid that is connected trough a serial port.
/// </summary>
public sealed class RfidComReader : IDisposable
{
    private readonly ICardDataReader _cardDataReader;
    private readonly ILogger<RfidComReader> _logger;
    private readonly SerialPort _serialPort;

    private TaskCompletionSource<string> _cardReadSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public RfidComReader(string comPort, ICardDataReader cardDataReader, ILogger<RfidComReader> logger)
    {
        _cardDataReader = cardDataReader;
        _logger = logger;
        _serialPort = new SerialPort(comPort, 9600, Parity.None) { DataBits = 8, StopBits = StopBits.One };
        _serialPort.Open();
        _serialPort.DataReceived += SerialPort_DataReceived;
    }

    public void Dispose()
    {
        _serialPort.Dispose();
        _cardReadSource.TrySetCanceled();
    }

    public Task<string> ReadCard(int readerId, CancellationToken cancellationToken)
    {
        return _cardReadSource.Task.WaitAsync(cancellationToken);
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var length = _serialPort.BytesToRead;
        if (length == 0)
            return;

        _logger.RfidSerialDataReceived(length);

        var data = new byte[length];
        _serialPort.Read(data, 0, length);

        string cardNumber = _cardDataReader.ReadData(data);
        _logger.RfidCardParsed(cardNumber);

        _cardReadSource.TrySetResult(cardNumber);
        _cardReadSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
