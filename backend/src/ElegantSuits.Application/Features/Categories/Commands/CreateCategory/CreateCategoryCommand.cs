using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Categories.Contracts;
using ElegantSuits.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(CreateCategoryDTO DTO) : IRequest<CategoryDTO>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.DTO.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống")
            .MaximumLength(100).WithMessage("Tên danh mục không được vượt quá 100 ký tự");
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDTO>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDTO> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = command.DTO.Name,
            Description = command.DTO.Description
        };

        await _categoryRepository.AddCategoryAsync(category, cancellationToken);

        return new CategoryDTO
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}
