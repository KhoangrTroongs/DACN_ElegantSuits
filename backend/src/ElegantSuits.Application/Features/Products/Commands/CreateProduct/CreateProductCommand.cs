using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Products.Contracts;
using ElegantSuits.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    CreateProductRequest Request,
    Stream? ImageStream = null,
    string? ImageFileName = null,
    Stream? Model3DStream = null,
    string? Model3DFileName = null) : IRequest<ProductResponse>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Tên sản phẩm không được để trống")
            .MaximumLength(200).WithMessage("Tên sản phẩm không được vượt quá 200 ký tự");

        RuleFor(x => x.Request.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Giá sản phẩm phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.Request.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Số lượng phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.Request.CategoryId)
            .GreaterThan(0).WithMessage("Danh mục không hợp lệ");
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponse>
{
    private readonly IProductWriteRepository _writeRepository;
    private readonly ICategoryReadRepository _categoryReadRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        IProductWriteRepository writeRepository,
        ICategoryReadRepository categoryReadRepository,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork)
    {
        _writeRepository = writeRepository;
        _categoryReadRepository = categoryReadRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductResponse> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        var categoryExists = await _categoryReadRepository.ExistsAsync(req.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new ValidationException("Danh mục được chọn không tồn tại.");
        }

        string imageUrl = "";
        if (command.ImageStream != null && !string.IsNullOrEmpty(command.ImageFileName))
        {
            imageUrl = await _fileStorage.SaveFileAsync(command.ImageStream, command.ImageFileName, "images/products", cancellationToken);
        }

        string? model3DUrl = req.Model3DUrl;
        if (command.Model3DStream != null && !string.IsNullOrEmpty(command.Model3DFileName))
        {
            model3DUrl = await _fileStorage.SaveFileAsync(command.Model3DStream, command.Model3DFileName, "models/products", cancellationToken);
        }

        var product = new Product
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            Quantity = req.Quantity,
            CategoryId = req.CategoryId,
            IsHidden = req.IsHidden,
            Model3DUrl = model3DUrl,
            ProfitMargin = req.ProfitMargin,
            LinearCode = req.LinearCode,
            ImageUrl = imageUrl
        };

        await _writeRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var categoryName = await _categoryReadRepository.GetNameByIdAsync(product.CategoryId, cancellationToken);

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            ImageUrl = product.ImageUrl,
            Model3DUrl = product.Model3DUrl,
            IsHidden = product.IsHidden,
            CategoryId = product.CategoryId,
            CategoryName = categoryName,
            LinearCode = product.LinearCode,
            ProfitMargin = product.ProfitMargin,
            AverageRating = 0,
            ReviewCount = 0
        };
    }
}
