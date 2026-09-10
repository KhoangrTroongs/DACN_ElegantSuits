using ElegantSuits.Application.Common.Interfaces;
using MediatR;

namespace ElegantSuits.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(int Id) : IRequest<bool>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductWriteRepository _writeRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(
        IProductWriteRepository writeRepository,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork)
    {
        _writeRepository = writeRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _writeRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(product.ImageUrl))
        {
            await _fileStorage.DeleteFileAsync(product.ImageUrl, cancellationToken);
        }

        _writeRepository.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
