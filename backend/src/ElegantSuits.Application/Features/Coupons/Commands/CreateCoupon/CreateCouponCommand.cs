using ElegantSuits.Application.Common.Interfaces;
using ElegantSuits.Application.Features.Coupons.Contracts;
using FluentValidation;
using MediatR;

namespace ElegantSuits.Application.Features.Coupons.Commands.CreateCoupon;

public record CreateCouponCommand(CreateCouponDTO DTO) : IRequest<CouponDTO>;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.DTO.Code).NotEmpty().WithMessage("Mã giảm giá không được để trống");
        RuleFor(x => x.DTO.DiscountPercentage).InclusiveBetween(1, 100).WithMessage("Phần trăm giảm giá từ 1 đến 100%");
    }
}

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, CouponDTO>
{
    private readonly ICouponRepository _couponRepository;

    public CreateCouponCommandHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<CouponDTO> Handle(CreateCouponCommand command, CancellationToken cancellationToken)
    {
        return await _couponRepository.AddCouponAsync(command.DTO, cancellationToken);
    }
}
