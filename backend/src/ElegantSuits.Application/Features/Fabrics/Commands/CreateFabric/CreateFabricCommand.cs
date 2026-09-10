using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Fabrics.Contracts;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Fabrics.Commands.CreateFabric;

public record CreateFabricCommand(CreateFabricDTO DTO) : IRequest<FabricDTO>;

public class CreateFabricCommandValidator : AbstractValidator<CreateFabricCommand>
{
    public CreateFabricCommandValidator()
    {
        RuleFor(x => x.DTO.Name).NotEmpty().WithMessage("Tên vải không được để trống");
        RuleFor(x => x.DTO.FabricGroupId).GreaterThan(0).WithMessage("Nhóm vải không hợp lệ");
    }
}

public class CreateFabricCommandHandler : IRequestHandler<CreateFabricCommand, FabricDTO>
{
    private readonly IFabricRepository _fabricRepository;

    public CreateFabricCommandHandler(IFabricRepository fabricRepository)
    {
        _fabricRepository = fabricRepository;
    }

    public async Task<FabricDTO> Handle(CreateFabricCommand command, CancellationToken cancellationToken)
    {
        return await _fabricRepository.AddFabricAsync(command.DTO, cancellationToken);
    }
}
