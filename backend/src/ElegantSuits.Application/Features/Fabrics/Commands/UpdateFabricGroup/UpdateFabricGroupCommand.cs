using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Fabrics.Contracts;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Fabrics.Commands.UpdateFabricGroup;

public record UpdateFabricGroupCommand(int Id, UpdateFabricGroupDTO DTO) : IRequest<FabricGroupDTO?>;

public class UpdateFabricGroupCommandValidator : AbstractValidator<UpdateFabricGroupCommand>
{
    public UpdateFabricGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("ID nhóm vải không hợp lệ");
        RuleFor(x => x.DTO.Name).NotEmpty().WithMessage("Tên nhóm vải không được để trống");
    }
}

public class UpdateFabricGroupCommandHandler : IRequestHandler<UpdateFabricGroupCommand, FabricGroupDTO?>
{
    private readonly IFabricRepository _fabricRepository;

    public UpdateFabricGroupCommandHandler(IFabricRepository fabricRepository)
    {
        _fabricRepository = fabricRepository;
    }

    public async Task<FabricGroupDTO?> Handle(UpdateFabricGroupCommand command, CancellationToken cancellationToken)
    {
        return await _fabricRepository.UpdateFabricGroupAsync(command.Id, command.DTO, cancellationToken);
    }
}
