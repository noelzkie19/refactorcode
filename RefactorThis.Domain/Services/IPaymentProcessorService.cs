using RefactorThis.Domain.Models;

namespace RefactorThis.Domain.Services
{
    public interface IPaymentProcessorService
    {
        string ProcessPayment(Invoice invoice, Payment payment);
    }
}