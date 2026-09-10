using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Categories.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(int Id) : IRequest<CategoryDTO?>;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDTO?>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDTO?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var c = await _categoryRepository.GetCategoryByIdAsync(request.Id, cancellationToken);
        if (c == null) return null;

        return new CategoryDTO
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        };
    }
}
