using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Services;
using Fabric.Hardware.Desfire.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Fabric.Hardware.Desfire.Tests;

public class WriteToFileOperationEncodingTests
{
    [Fact]
    public async Task Execute_should_pack_badge_number_as_seven_byte_big_endian_data()
    {
        Mock<IRfidEncoder> encoderMock = new();
        List<byte[]> sent = [];
        _ = encoderMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], CancellationToken>((bytes, _) => sent.Add([.. bytes]))
            .ReturnsAsync([0x00]);

        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, encoderMock.Object)
        {
            Session = new PlainDesfireSession(loggerMock.Object, encoderMock.Object),
        };

        ExecutionState state = new()
        {
            Variables =
            {
                ["badgeNumber"] = System.Text.Encoding.UTF8.GetBytes("47551"),
            },
        };

        WriteToFileOperation operation = new(1, CommunicationMode.Plain, "badgeNumber", "uint:7:be", 0, 48);

        _ = await operation.Execute(state, reader);

        sent.ShouldHaveSingleItem();
        Convert.ToHexString(sent[0]).ShouldContain("0000000000B9BF");
    }

    [Fact]
    public async Task Execute_should_not_use_file_offset_as_value_offset()
    {
        Mock<IRfidEncoder> encoderMock = new();
        List<byte[]> sent = [];
        _ = encoderMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], CancellationToken>((bytes, _) => sent.Add([.. bytes]))
            .ReturnsAsync([0x00]);

        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, encoderMock.Object)
        {
            Session = new PlainDesfireSession(loggerMock.Object, encoderMock.Object),
        };

        ExecutionState state = new()
        {
            Variables =
            {
                ["badgeNumber"] = System.Text.Encoding.UTF8.GetBytes("47551"),
            },
        };

        WriteToFileOperation operation = new(1, CommunicationMode.Plain, "badgeNumber", "uint:7:be", 3, 48);

        _ = await operation.Execute(state, reader);

        sent.ShouldHaveSingleItem();
        Convert.ToHexString(sent[0]).ShouldContain("0000000000B9BF");
    }

    [Fact]
    public async Task Execute_should_pack_badge_number_as_seven_byte_little_endian_data()
    {
        Mock<IRfidEncoder> encoderMock = new();
        List<byte[]> sent = [];
        _ = encoderMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], CancellationToken>((bytes, _) => sent.Add([.. bytes]))
            .ReturnsAsync([0x00]);

        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, encoderMock.Object)
        {
            Session = new PlainDesfireSession(loggerMock.Object, encoderMock.Object),
        };

        ExecutionState state = new()
        {
            Variables =
            {
                ["badgeNumber"] = System.Text.Encoding.UTF8.GetBytes("3552564"),
            },
        };

        WriteToFileOperation operation = new(1, CommunicationMode.Plain, "badgeNumber", "uint:7:le", 3, 48);

        _ = await operation.Execute(state, reader);

        sent.ShouldHaveSingleItem();
        Convert.ToHexString(sent[0]).ShouldEndWith("34353600000000");
    }
}
