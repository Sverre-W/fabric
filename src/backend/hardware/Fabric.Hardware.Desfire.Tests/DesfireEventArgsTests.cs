using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

//
// namespace Fabric.Hardware.Desfire.Tests;
//
// public class DesfireEventArgsTests
// {
//     [Fact]
//     public void Carries_card_uid_when_available()
//     {
//         Mock<IRfidEncoder> encoderMock = new();
//         DesfireReader reader = new(NullLogger.Instance, encoderMock.Object, disposeCardReader: false);
//
//         DesfireEventArgs args = new(
//             0,
//             new GetCardUidOperation(),
//             new DesfireResponse(DesfireStatusCode.Success),
//             reader,
//             "04A1B2C3D4"
//         );
//
//         args.CardUid.ShouldBe("04A1B2C3D4");
//         args.Operation.ShouldBeOfType<GetCardUidOperation>();
//         args.Reader.ShouldBe(reader);
//     }
// }
