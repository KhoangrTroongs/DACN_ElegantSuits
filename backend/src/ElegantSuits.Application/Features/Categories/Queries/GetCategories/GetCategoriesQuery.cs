using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Categories.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<IEnumerable<CategoryDTO>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryDTO>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDTO>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllCategoriesAsync(cancellationToken);
        return categories.Select(c => new CategoryDTO
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        });
    }
}
