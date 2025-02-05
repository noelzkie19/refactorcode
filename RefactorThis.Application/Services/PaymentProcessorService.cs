
using RefactorThis.Domain.Models;
using RefactorThis.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace RefactorThis.Application.Services
{
    public class PaymentProcessorService : IPaymentProcessorService
    {
        private readonly ITaxService _taxService;
        private readonly IMessageFormatterService _messageFormatterService;
        private readonly ILogger<PaymentProcessorService> _logger;

        public PaymentProcessorService(ITaxService taxService, IMessageFormatterService messageFormatterService, ILogger<PaymentProcessorService> logger)
        {
            _taxService = taxService;
            _messageFormatterService = messageFormatterService;
            _logger = logger;
        }

        public string ProcessPayment(Invoice inv, Payment payment)
        {
            _logger.LogInformation("Processing payment for invoice ID {InvoiceId}. Payment amount: {PaymentAmount}.", inv.Id, payment.Amount);
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

            _logger.LogInformation("Payment Successful for invoice ID {InvoiceId}" , inv.Id);
            return _messageFormatterService.GetPaymentMessage(inv, payment, remainingAmount);

        }
    }
}
