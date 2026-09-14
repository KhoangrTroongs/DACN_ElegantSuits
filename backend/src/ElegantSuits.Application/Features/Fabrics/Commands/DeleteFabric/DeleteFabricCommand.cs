using ElegantSuits.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Fabrics.Commands.DeleteFabric;

public record DeleteFabricCommand(int Id) : IRequest<bool>;

public class DeleteFabricCommandValidator : AbstractValidator<DeleteFabricCommand>
{
    public DeleteFabricCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("ID vải không hợp lệ");
    }
}

public class DeleteFabricCommandHandler : IRequestHandler<DeleteFabricCommand, bool>
{
    private readonly IFabricRepository _fabricRepository;

    public DeleteFabricCommandHandler(IFabricRepository fabricRepository)
    {
        _fabricRepository = fabricRepository;
    }

    public async Task<bool> Handle(DeleteFabricCommand command, CancellationToken cancellationToken)
    {
        return await _fabricRepository.DeleteFabricAsync(command.Id, cancellationToken);
    }
}
