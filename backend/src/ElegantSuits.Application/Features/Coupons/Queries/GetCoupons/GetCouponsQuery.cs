using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Coupons.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Coupons.Queries.GetCoupons;

public record GetCouponsQuery : IRequest<IEnumerable<CouponDTO>>;

public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, IEnumerable<CouponDTO>>
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponsQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<IEnumerable<CouponDTO>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        return await _couponRepository.GetAllCouponsAsync(cancellationToken);
    }
}
