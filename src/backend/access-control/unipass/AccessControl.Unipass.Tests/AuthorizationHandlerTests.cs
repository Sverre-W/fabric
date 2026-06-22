using System.Net;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace AccessControl.Unipass.Tests;

public class AuthorizationHandlerTests : UnipassTestBase
{
    [Fact]
    public async Task Should_Add_ApiKey_Header()
    {
        // Arrange
        var apiKey = "test-api-key";
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Headers.Authorization != null
                    && (
                        r.Headers.Authorization.Parameter == apiKey
                        || r.Headers.GetValues("Authorization").Contains(apiKey)
                    )
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }
            )
            .Verifiable();

        var provider = CreateServiceProvider(handlerMock.Object, apiKey, "https://localhost");
        var client = provider.GetRequiredService<IUnipassApi>();

        // Act
        await client.GetPersons(new PersonFilter(), CancellationToken.None);

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Headers.Authorization != null
                    && (
                        r.Headers.Authorization.Parameter == apiKey
                        || r.Headers.GetValues("Authorization").Contains(apiKey)
                    )
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }
}
