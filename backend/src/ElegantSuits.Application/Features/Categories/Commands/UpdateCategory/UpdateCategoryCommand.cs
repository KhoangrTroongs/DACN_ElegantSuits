using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Categories.Contracts;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(int Id, UpdateCategoryDTO DTO) : IRequest<CategoryDTO?>;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id danh mục không hợp lệ");
        RuleFor(x => x.DTO.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống")
            .MaximumLength(100).WithMessage("Tên danh mục không được vượt quá 100 ký tự");
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDTO?>
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDTO?> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(command.Id, cancellationToken);
        if (category == null) return null;

        category.Name = command.DTO.Name;
        category.Description = command.DTO.Description;

        await _categoryRepository.UpdateCategoryAsync(category, cancellationToken);

        return new CategoryDTO
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}
