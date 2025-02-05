
using RefactorThis.Domain.Models;
using RefactorThis.Domain.Services;
using System.Linq;

namespace RefactorThis.Application.Services
{
    public class PaymentProcessorService : IPaymentProcessorService
    {
        private readonly ITaxService _taxService;
        private readonly IMessageFormatterService _messageFormatterService;

        public PaymentProcessorService(ITaxService taxService, IMessageFormatterService messageFormatterService)
        {
            _taxService = taxService;
            _messageFormatterService = messageFormatterService;
        }

        public string ProcessPayment(Invoice inv, Payment payment)
        {
            decimal totalPaid = inv.Payments?.Sum(x => x.Amount) ?? 0m;
            decimal remainingAmount = inv.Amount - totalPaid;

            if (totalPaid == inv.Amount)
            {
                return "invoice was already fully paid";
            }

            if (payment.Amount > remainingAmount)
            {
                return "the payment is greater than the partial amount remaining";
            }

            inv.AmountPaid += payment.Amount;
            inv.TaxAmount += _taxService.CalculateTax(payment.Amount, inv.Type);
            inv.Payments.Add(payment);

            return _messageFormatterService.GetPaymentMessage(inv, payment, remainingAmount);
        }
    }
}
