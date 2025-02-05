
using RefactorThis.Domain.Models;

namespace RefactorThis.Domain.Services
{
    public interface IMessageFormatterService
    {
        string GetPaymentMessage(Invoice inv, Payment payment, decimal remainingAmount);
    }
}
