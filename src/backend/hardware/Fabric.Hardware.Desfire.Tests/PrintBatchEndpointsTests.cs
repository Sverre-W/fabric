// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Logging.Abstractions;
// using Marten;
// using Microsoft.Extensions.DependencyInjection;
// using Moq;
// using Shouldly;
// using Fabric.Hardware.Desfire.Encoding.Models;
// using Fabric.Hardware.Desfire.Encoding.Server.Endpoints;
// using Fabric.Hardware.Desfire.Encoding.Server.Services;
//
// namespace Fabric.Hardware.Desfire.Tests;
//
// public class PrintBatchEndpointsTests
// {
//     private static PrinterScheduler CreateScheduler()
//     {
//         var provider = new Mock<IServiceProvider>();
//         var factory = new Mock<IServiceScopeFactory>();
//         provider.Setup(x => x.GetService(typeof(ILogger<PrinterScheduler>))).Returns(NullLogger<PrinterScheduler>.Instance);
//         return new PrinterScheduler(provider.Object, factory.Object);
//     }
//
//     [Fact]
//     public async Task CancelPrintBatch_should_set_cancelled()
//     {
//         var batch = new PrintingBatch { Id = Guid.NewGuid(), Status = PrintingBatchStatus.Scheduled, Printer = new EntityLink { Id = Guid.NewGuid(), Name = "Printer 1" } };
//         var session = new Mock<IDocumentSession>();
//         session.Setup(x => x.LoadAsync<PrintingBatch>(batch.Id, It.IsAny<CancellationToken>())).ReturnsAsync(batch);
//         session.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
//
//         var result = await PrintBatchEndpoints.CancelPrintBatch(batch.Id, session.Object, CreateScheduler(), CancellationToken.None);
//
//         batch.Status.ShouldBe(PrintingBatchStatus.Cancelled);
//         result.ShouldNotBeNull();
//     }
//
//     [Fact]
//     public async Task HoldPrintBatch_should_set_on_hold()
//     {
//         var batch = new PrintingBatch { Id = Guid.NewGuid(), Status = PrintingBatchStatus.Scheduled, Printer = new EntityLink { Id = Guid.NewGuid(), Name = "Printer 1" } };
//         var session = new Mock<IDocumentSession>();
//         session.Setup(x => x.LoadAsync<PrintingBatch>(batch.Id, It.IsAny<CancellationToken>())).ReturnsAsync(batch);
//         session.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
//
//         var result = await PrintBatchEndpoints.HoldPrintBatch(batch.Id, session.Object, CreateScheduler(), CancellationToken.None);
//
//         batch.Status.ShouldBe(PrintingBatchStatus.OnHold);
//         result.ShouldNotBeNull();
//     }
//
//     [Fact]
//     public async Task ResumePrintBatch_should_set_scheduled()
//     {
//         var batch = new PrintingBatch { Id = Guid.NewGuid(), Status = PrintingBatchStatus.OnHold, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow, Printer = new EntityLink { Id = Guid.NewGuid(), Name = "Printer 1" } };
//         var session = new Mock<IDocumentSession>();
//         session.Setup(x => x.LoadAsync<PrintingBatch>(batch.Id, It.IsAny<CancellationToken>())).ReturnsAsync(batch);
//         session.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
//
//         var result = await PrintBatchEndpoints.ResumePrintBatch(batch.Id, session.Object, CancellationToken.None);
//
//         batch.Status.ShouldBe(PrintingBatchStatus.Scheduled);
//         batch.StartedAt.ShouldBeNull();
//         batch.CompletedAt.ShouldBeNull();
//         result.ShouldNotBeNull();
//     }
//
//     [Fact]
//     public async Task CancelPrintBatch_should_reject_terminal_batches()
//     {
//         var batch = new PrintingBatch { Id = Guid.NewGuid(), Status = PrintingBatchStatus.Completed, Printer = new EntityLink { Id = Guid.NewGuid(), Name = "Printer 1" } };
//         var session = new Mock<IDocumentSession>();
//         session.Setup(x => x.LoadAsync<PrintingBatch>(batch.Id, It.IsAny<CancellationToken>())).ReturnsAsync(batch);
//
//         var result = await PrintBatchEndpoints.CancelPrintBatch(batch.Id, session.Object, CreateScheduler(), CancellationToken.None);
//
//         result.ShouldBeOfType<Microsoft.AspNetCore.Http.HttpResults.Conflict<Microsoft.AspNetCore.Mvc.ValidationProblemDetails>>();
//         batch.Status.ShouldBe(PrintingBatchStatus.Completed);
//     }
// }
