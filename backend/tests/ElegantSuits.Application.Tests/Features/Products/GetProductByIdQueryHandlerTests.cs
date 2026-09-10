using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Products.Contracts;
using ElegantSuits.Application.Features.Products.Queries.GetProductById;
using FluentAssertions;
using Moq;
using Xunit;

namespace ElegantSuits.Application.Tests.Features.Products;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IProductReadRepository> _readRepoMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _readRepoMock = new Mock<IProductReadRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _handler = new GetProductByIdQueryHandler(_readRepoMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WhenProductExists_ReturnsProductResponse()
    {
        // Arrange
        var productId = 10;
        var expectedProduct = new ProductResponse
        {
            Id = productId,
            Name = "Veston Luxury",
            Price = 1500000,
            CategoryId = 1,
            CategoryName = "Veston"
        };

        _currentUserMock.Setup(u => u.IsInRole("Administrator")).Returns(false);
        _readRepoMock.Setup(r => r.GetProductByIdAsync(productId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Veston Luxury");
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsNull()
    {
        // Arrange
        var productId = 999;
        _currentUserMock.Setup(u => u.IsInRole("Administrator")).Returns(false);
        _readRepoMock.Setup(r => r.GetProductByIdAsync(productId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductResponse?)null);

        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
