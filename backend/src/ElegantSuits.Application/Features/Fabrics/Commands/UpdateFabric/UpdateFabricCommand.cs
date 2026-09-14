using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Fabrics.Contracts;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Fabrics.Commands.UpdateFabric;

public record UpdateFabricCommand(int Id, UpdateFabricDTO DTO) : IRequest<FabricDTO?>;

public class UpdateFabricCommandValidator : AbstractValidator<UpdateFabricCommand>
{
    public UpdateFabricCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("ID vải không hợp lệ");
        RuleFor(x => x.DTO.Name).NotEmpty().WithMessage("Tên vải không được để trống");
        RuleFor(x => x.DTO.FabricGroupId).GreaterThan(0).WithMessage("Nhóm vải không hợp lệ");
    }
}

public class UpdateFabricCommandHandler : IRequestHandler<UpdateFabricCommand, FabricDTO?>
{
    private readonly IFabricRepository _fabricRepository;

    public UpdateFabricCommandHandler(IFabricRepository fabricRepository)
    {
        _fabricRepository = fabricRepository;
    }

    public async Task<FabricDTO?> Handle(UpdateFabricCommand command, CancellationToken cancellationToken)
    {
        return await _fabricRepository.UpdateFabricAsync(command.Id, command.DTO, cancellationToken);
    }
}
