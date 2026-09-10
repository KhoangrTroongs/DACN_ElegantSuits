using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Products.Commands.CreateProduct;
using ElegantSuits.Application.Features.Products.Contracts;
using ElegantSuits.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Moq;
using Xunit;

namespace ElegantSuits.Application.Tests.Features.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductWriteRepository> _writeRepoMock;
    private readonly Mock<ICategoryReadRepository> _categoryRepoMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _writeRepoMock = new Mock<IProductWriteRepository>();
        _categoryRepoMock = new Mock<ICategoryReadRepository>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateProductCommandHandler(
            _writeRepoMock.Object,
            _categoryRepoMock.Object,
            _fileStorageMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCategoryExists_CreatesProductSuccessfully()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Elegant Navy Suit",
            Description = "Tailored slim fit navy suit",
            Price = 2500000,
            Quantity = 15,
            CategoryId = 1
        };

        _categoryRepoMock.Setup(c => c.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _categoryRepoMock.Setup(c => c.GetNameByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Suits");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateProductCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Elegant Navy Suit");
        result.Price.Should().Be(2500000);
        result.CategoryId.Should().Be(1);
        result.CategoryName.Should().Be("Suits");

        _writeRepoMock.Verify(w => w.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Test Suit",
            Price = 1000000,
            CategoryId = 999
        };

        _categoryRepoMock.Setup(c => c.ExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateProductCommand(request);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Danh mục được chọn không tồn tại*");

        _writeRepoMock.Verify(w => w.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
