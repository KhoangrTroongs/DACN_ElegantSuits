using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Coupons.Contracts;
using MediatR;

namespace ElegantSuits.Application.Features.Coupons.Queries.ValidateCoupon;

public record ValidateCouponQuery(string Code, decimal OrderAmount) : IRequest<ValidateCouponResult>;

public class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, ValidateCouponResult>
{
    private readonly ICouponRepository _couponRepository;

    public ValidateCouponQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<ValidateCouponResult> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        return await _couponRepository.ValidateCouponAsync(request.Code, request.OrderAmount, cancellationToken);
    }
}
