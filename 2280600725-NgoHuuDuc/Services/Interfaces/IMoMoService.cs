using NgoHuuDuc_2280600725.Models.MoMo;

namespace NgoHuuDuc_2280600725.Services.Interfaces
{
    public interface IMoMoService
    {
        Task<MoMoPaymentResponse> CreatePaymentAsync(string orderId, decimal amount, string orderInfo);
        bool ValidateSignature(MoMoPaymentResultRequest result);
    }
}

