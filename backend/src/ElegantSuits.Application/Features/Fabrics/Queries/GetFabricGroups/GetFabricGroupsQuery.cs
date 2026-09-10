using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Fabrics.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Fabrics.Queries.GetFabricGroups;

public record GetFabricGroupsQuery : IRequest<IEnumerable<FabricGroupDTO>>;

public class GetFabricGroupsQueryHandler : IRequestHandler<GetFabricGroupsQuery, IEnumerable<FabricGroupDTO>>
{
    private readonly IFabricRepository _fabricRepository;

    public GetFabricGroupsQueryHandler(IFabricRepository fabricRepository)
    {
        _fabricRepository = fabricRepository;
    }

    public async Task<IEnumerable<FabricGroupDTO>> Handle(GetFabricGroupsQuery request, CancellationToken cancellationToken)
    {
        return await _fabricRepository.GetAllFabricGroupsAsync(cancellationToken);
    }
}
