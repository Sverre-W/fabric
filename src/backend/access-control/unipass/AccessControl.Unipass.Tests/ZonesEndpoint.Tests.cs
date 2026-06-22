using System.Net;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;
using AccessControl.Unipass.Enums;
using AccessControl.Unipass.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace AccessControl.Unipass.Tests;

/*
public class ZonesEndpoint : UnipassTestBase
{
    [Fact]
    public async Task Should_Add_Zone_Endpoint()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get
                    && r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/Zone")
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
        await api.GetEntitiesAsync<UnipassZone>();

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("Zone")),
                ItExpr.IsAny<CancellationToken>()
            );
    }
}
*/
