using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Fabrics.Contracts;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Fabrics.Commands.CreateFabricGroup;

public record CreateFabricGroupCommand(CreateFabricGroupDTO DTO) : IRequest<FabricGroupDTO>;

public class CreateFabricGroupCommandValidator : AbstractValidator<CreateFabricGroupCommand>
{
    public CreateFabricGroupCommandValidator()
    {
        RuleFor(x => x.DTO.Name).NotEmpty().WithMessage("Tên nhóm vải không được để trống");
    }
}

public class CreateFabricGroupCommandHandler : IRequestHandler<CreateFabricGroupCommand, FabricGroupDTO>
{
    private readonly IFabricRepository _fabricRepository;

    public CreateFabricGroupCommandHandler(IFabricRepository fabricRepository)
    {
        _fabricRepository = fabricRepository;
    }

    public async Task<FabricGroupDTO> Handle(CreateFabricGroupCommand command, CancellationToken cancellationToken)
    {
        return await _fabricRepository.AddFabricGroupAsync(command.DTO, cancellationToken);
    }
}
