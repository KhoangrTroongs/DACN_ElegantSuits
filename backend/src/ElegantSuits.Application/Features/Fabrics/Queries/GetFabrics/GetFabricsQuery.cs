using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Fabrics.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Fabrics.Queries.GetFabrics;

public record GetFabricsQuery : IRequest<IEnumerable<FabricDTO>>;

public class GetFabricsQueryHandler : IRequestHandler<GetFabricsQuery, IEnumerable<FabricDTO>>
{
    private readonly IFabricRepository _fabricRepository;

    public GetFabricsQueryHandler(IFabricRepository fabricRepository)
    {
        _fabricRepository = fabricRepository;
    }

    public async Task<IEnumerable<FabricDTO>> Handle(GetFabricsQuery request, CancellationToken cancellationToken)
    {
        return await _fabricRepository.GetAllFabricsAsync(cancellationToken);
    }
}
