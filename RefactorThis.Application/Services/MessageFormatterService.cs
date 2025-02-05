

using RefactorThis.Domain.Models;
using RefactorThis.Domain.Services;

namespace RefactorThis.Application.Services
{
    public class MessageFormatterService : IMessageFormatterService
    {
        public string GetPaymentMessage(Invoice inv, Payment payment, decimal remainingAmount)
        {
            bool isFullyPaid = inv.Amount == inv.AmountPaid;
            bool isFinalPartialPayment = remainingAmount == payment.Amount;

            if (isFullyPaid)
            {
                return "invoice is now fully paid";
            }

            return isFinalPartialPayment
                ? "final partial payment received, invoice is now fully paid"
                : "another partial payment received, still not fully paid";
        }
    }
}
