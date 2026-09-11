using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Products.Contracts;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    int Id,
    UpdateProductRequest Request,
    Stream? ImageStream = null,
    string? ImageFileName = null,
    Stream? Model3DStream = null,
    string? Model3DFileName = null) : IRequest<ProductResponse?>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id sản phẩm không hợp lệ");

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

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponse?>
{
    private readonly IProductWriteRepository _writeRepository;
    private readonly ICategoryReadRepository _categoryReadRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
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

    public async Task<ProductResponse?> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _writeRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product == null)
        {
            return null;
        }

        var req = command.Request;

        var categoryExists = await _categoryReadRepository.ExistsAsync(req.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new ValidationException("Danh mục được chọn không tồn tại.");
        }

        if (command.ImageStream != null && !string.IsNullOrEmpty(command.ImageFileName))
        {
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                await _fileStorage.DeleteFileAsync(product.ImageUrl, cancellationToken);
            }
            product.ImageUrl = await _fileStorage.SaveFileAsync(command.ImageStream, command.ImageFileName, "images/products", cancellationToken);
        }
        else if (!string.IsNullOrEmpty(req.ImageUrl))
        {
            product.ImageUrl = req.ImageUrl;
        }

        if (command.Model3DStream != null && !string.IsNullOrEmpty(command.Model3DFileName))
        {
            if (!string.IsNullOrEmpty(product.Model3DUrl))
            {
                await _fileStorage.DeleteFileAsync(product.Model3DUrl, cancellationToken);
            }
            product.Model3DUrl = await _fileStorage.SaveFileAsync(command.Model3DStream, command.Model3DFileName, "models/products", cancellationToken);
        }
        else if (!string.IsNullOrEmpty(req.Model3DUrl))
        {
            product.Model3DUrl = req.Model3DUrl;
        }

        product.Name = req.Name;
        product.Description = req.Description;
        product.Price = req.Price;
        product.Quantity = req.Quantity;
        product.CategoryId = req.CategoryId;
        product.IsHidden = req.IsHidden;
        product.ProfitMargin = req.ProfitMargin;
        product.LinearCode = req.LinearCode;

        _writeRepository.Update(product);
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
            AverageRating = product.AverageRating,
            ReviewCount = product.ReviewCount
        };
    }
}
