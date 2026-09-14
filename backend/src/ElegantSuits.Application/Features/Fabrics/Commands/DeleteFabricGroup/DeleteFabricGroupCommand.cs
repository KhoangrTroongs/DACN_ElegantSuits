using ElegantSuits.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Fabrics.Commands.DeleteFabricGroup;

public record DeleteFabricGroupCommand(int Id) : IRequest<bool>;

public class DeleteFabricGroupCommandValidator : AbstractValidator<DeleteFabricGroupCommand>
{
    public DeleteFabricGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("ID nhóm vải không hợp lệ");
    }
}

public class DeleteFabricGroupCommandHandler : IRequestHandler<DeleteFabricGroupCommand, bool>
{
    private readonly IFabricRepository _fabricRepository;

    public DeleteFabricGroupCommandHandler(IFabricRepository fabricRepository)
    {
        _fabricRepository = fabricRepository;
    }

    public async Task<bool> Handle(DeleteFabricGroupCommand command, CancellationToken cancellationToken)
    {
        return await _fabricRepository.DeleteFabricGroupAsync(command.Id, cancellationToken);
    }
}
