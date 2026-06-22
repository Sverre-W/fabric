using System.Net;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;
using AccessControl.Unipass.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace AccessControl.Unipass.Tests;

public class SitesEndpoint : UnipassTestBase
{
    [Fact]
    public async Task Should_Add_Sites_Endpoint()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get
                    && r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/Sites")
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
        await api.GetSites();

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("Sites")),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [Fact]
    public async Task Should_Add_Sites_Search_Name_Filter()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get
                    && r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/Sites")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }
            )
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();
        var name = "Namur";
        var filter = new SitesFilter().WithName(name);

        // Act
        await api.GetSites(filter);

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri!.ToString().Contains("Sites")
                    && r.RequestUri!.ToString().Contains("filter")
                    && r.RequestUri!.ToString()
                        .Contains($"Name1 eq {name} or Name2 eq {name} or Name3 eq {name}")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }
}
