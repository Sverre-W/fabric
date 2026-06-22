using System.Net;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;
using AccessControl.Unipass.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace AccessControl.Unipass.Tests;

/*
public class AccessTickingEndpoint : UnipassTestBase
{
    [Fact]
    public async Task Should_Add_AccessTicking_Endpoint()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get &&
                    r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/AccessTicking")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") })
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();

        // Act
        await api.GetEntitiesAsync<AccessTicking>();

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri!.ToString().Contains("AccessTicking")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [Fact]
    public async Task Should_Add_AccessTicking_Search_Visitor_Id_Filter()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get &&
                    r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/AccessTicking")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") })
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();
        var id = 210;
        var filter = new AccessTickingFilter().WithVisitorId(id);

        // Act
        await api.GetEntitiesAsync(filter);

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri!.ToString().Contains("AccessTicking") &&
                    r.RequestUri!.ToString().Contains("filter") &&
                    r.RequestUri!.ToString().Contains($"Person eq {id}")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }
}
*/
