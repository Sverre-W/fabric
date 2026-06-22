using System.Net;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;
using AccessControl.Unipass.Enums;
using AccessControl.Unipass.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace AccessControl.Unipass.Tests;

public class AssignedAccessRuleEndpoint : UnipassTestBase
{
    [Fact]
    public async Task Should_Add_AssignedAccessRule_And_Visitor_Id_Endpoint()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get
                    && r.RequestUri!.ToString()
                        .Contains("/IDtech/IdtAPIService/api/PersonAccessRules")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }
            )
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();
        var id = 210;

        // Act
        await api.GetAssignedAccessRules(id);

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri!.ToString().Contains("PersonAccessRules")
                    && r.RequestUri!.ToString().Contains($"filter=Person eq {id}")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    /*[Fact]
    public async Task Should_Send_AssignedAccessRule_Insert_Operation()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post
                    && r.RequestUri!.ToString()
                        .Contains("/IDtech/IdtAPIService/api/PersonAccessRules")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }
            )
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();

        // Act
        await api.OperationEntitiesAsync(
            UnipassOperation.Insert,
            [
                new AssignedAccessRule
                {
                    Id = 1,
                    Person = 120,
                    Site = 1,
                    Rule = 2,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    StartTime = 0,
                    EndTime = 0,
                },
            ]
        );

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(static r =>
                    r.RequestUri!.ToString().Contains("PersonAccessRules")
                    && r.Method == HttpMethod.Post
                    && r.Content != null
                    && r.Content.ReadAsStringAsync().Result.Contains("insert")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [Fact]
    public async Task Should_Send_AssignedAccessRule_Update_Operation()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post
                    && r.RequestUri!.ToString()
                        .Contains("/IDtech/IdtAPIService/api/PersonAccessRules")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }
            )
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();

        // Act
        await api.OperationEntitiesAsync(
            UnipassOperation.Update,
            [
                new AssignedAccessRule
                {
                    Id = 1,
                    Person = 120,
                    Site = 1,
                    Rule = 2,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    StartTime = 0,
                    EndTime = 0,
                },
            ]
        );

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(static r =>
                    r.RequestUri!.ToString().Contains("PersonAccessRules")
                    && r.Method == HttpMethod.Post
                    && r.Content != null
                    && r.Content.ReadAsStringAsync().Result.Contains("update")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [Fact]
    public async Task Should_Send_AssignedAccessRule_Delete_Operation()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post
                    && r.RequestUri!.ToString()
                        .Contains("/IDtech/IdtAPIService/api/PersonAccessRules")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }
            )
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();

        // Act
        await api.OperationEntitiesAsync(
            UnipassOperation.Delete,
            [new AssignedAccessRule { Id = 1 }]
        );

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(static r =>
                    r.RequestUri!.ToString().Contains("PersonAccessRules")
                    && r.Method == HttpMethod.Post
                    && r.Content != null
                    && r.Content.ReadAsStringAsync().Result.Contains("delete")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }*/
}
