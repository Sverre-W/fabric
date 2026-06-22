using System.Net;
using AccessControl.Unipass.ChangeSets;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;
using AccessControl.Unipass.Enums;
using AccessControl.Unipass.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

namespace AccessControl.Unipass.Tests;

public class VisitorsEndpointTests : UnipassTestBase
{
    [Fact]
    public async Task Should_Add_Default_Visitor_Filter()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get
                    && r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/Persons")
                    && r.RequestUri.Query.Contains("AccessType")
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
        await api.GetPersons(new PersonFilter().WithAccessType(UnipassPersonType.Visitor));

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("Persons")),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [Fact]
    public async Task Should_Add_Visitor_Cards_Filter()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get
                    && r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/Persons")
                    && r.RequestUri.Query.Contains("AccessType")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }
            )
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();

        var filter = new PersonFilter().WithCards().WithAccessType(UnipassPersonType.Visitor);

        // Act
        await api.GetPersons(filter);

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri!.ToString().Contains("Persons")
                    && r.RequestUri.ToString().Contains("AccessType eq 1")
                    && r.RequestUri.ToString()
                        .Contains(
                            "Badge1 ne 0 or Badge2 ne 0 or Badge3 ne 0 or Badge4 ne 0 or Badge5 ne 0"
                        )
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [Fact]
    public async Task Should_Add_Visitor_Id_And_Cards_Filter()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get
                    && r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/Persons")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }
            )
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();

        int id = 123123;
        var filter = new PersonFilter()
            .WithId(id)
            .WithCards()
            .WithAccessType(UnipassPersonType.Visitor);

        // Act
        await api.GetPersons(filter);

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri!.ToString().Contains("Persons")
                    && r.RequestUri.ToString().Contains("AccessType eq 1")
                    && r.RequestUri.ToString().Contains($"Person eq {id}")
                    && r.RequestUri.ToString()
                        .Contains(
                            $"Badge1 ne 0 or Badge2 ne 0 or Badge3 ne 0 or Badge4 ne 0 or Badge5 ne 0"
                        )
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [Fact]
    public async Task Should_Send_Visitor_Insert_Operation()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post
                    && r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/Persons")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }
            )
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();

        int id = 123123;

        // Act

        await api.ApplyChangeSet(
            PersonChangeSet
                .Create(id)
                .FirstName("Firstname")
                .LastName("Lastname")
                .Language(0)
                .PersonType(UnipassPersonType.Visitor)
                .Sex(UnipassSex.Female)
        );

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(static r =>
                    r.RequestUri!.ToString().Contains("Persons")
                    && r.Method == HttpMethod.Post
                    && r.Content != null
                    && r.Content.ReadAsStringAsync().Result.Contains("Insert")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [Fact]
    public async Task Should_Send_Visitor_Update_Operation()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post
                    && r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/Persons")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }
            )
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();

        int id = 123123;

        // Act

        await api.ApplyChangeSet(PersonChangeSet.Update(id).FirstName("Firstname"));

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri!.ToString().Contains("Persons")
                    && r.Method == HttpMethod.Post
                    && r.Content != null
                    && r.Content.ReadAsStringAsync().Result.Contains("Update")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [Fact]
    public async Task Should_Send_Visitor_Delete_Operation()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post
                    && r.RequestUri!.ToString().Contains("/IDtech/IdtAPIService/api/Persons")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }
            )
            .Verifiable();

        var sp = CreateServiceProvider(handlerMock.Object);
        var api = sp.GetRequiredService<IUnipassApi>();

        int id = 123123;

        // Act
        await api.ApplyChangeSet(PersonChangeSet.Delete(id));

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.RequestUri!.ToString().Contains("Persons")
                    && r.Method == HttpMethod.Post
                    && r.Content != null
                    && r.Content.ReadAsStringAsync().Result.Contains("Delete")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }
}
