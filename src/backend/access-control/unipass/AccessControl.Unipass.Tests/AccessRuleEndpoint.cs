using System.Net;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;
using AccessControl.Unipass.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace AccessControl.Unipass.Tests;

public class AccessRuleEndpoint : UnipassTestBase
{
    [Fact]
    public async Task Should_Add_AccessRule_Endpoint()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get
                    && r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/RuleCalendar")
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
        await api.GetAccessRules();

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri!.ToString().Contains("RuleCalendar")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [Fact]
    public async Task Should_Add_AccessRule_With_Id_Endpoint()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get
                    && r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/RuleCalendar")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }
            )
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();
        var id = 1;
        var filter = new AccessRuleFilter().WithId(id);

        // Act
        await api.GetAccessRules(filter);

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri!.ToString().Contains("RuleCalendar")
                    && r.RequestUri!.ToString().Contains($"Id eq {id}")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }
}
