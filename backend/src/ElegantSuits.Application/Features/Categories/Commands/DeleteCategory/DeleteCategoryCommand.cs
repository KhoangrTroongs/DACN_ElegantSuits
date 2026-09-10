using ElegantSuits.Application.Common.Interfaces;
using MediatR;

namespace ElegantSuits.Application.Features.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(int Id) : IRequest<bool>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<bool> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(command.Id, cancellationToken);
        if (category == null) return false;

        await _categoryRepository.DeleteCategoryAsync(command.Id, cancellationToken);
        return true;
    }
}
