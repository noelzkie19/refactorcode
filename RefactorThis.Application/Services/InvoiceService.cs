using RefactorThis.Domain.Enum;
using RefactorThis.Domain.IRepositories;
using RefactorThis.Domain.Models;
using RefactorThis.Domain.Services;
using System;
using System.Linq;

namespace RefactorThis.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentProcessorService _paymentProcessorService;

        public InvoiceService(IInvoiceRepository invoiceRepository, IPaymentProcessorService paymentProcessorService)
        {
            _invoiceRepository = invoiceRepository;
            _paymentProcessorService = paymentProcessorService;
        }
        public string ProcessPayment(Payment payment)
        {
            var inv = _invoiceRepository.GetInvoice(payment.Reference);
            var responseMessage = string.Empty;

            if (inv == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            if (inv.Amount == 0)
            {
                if (inv.Payments?.Any() == true)
                {
                    throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
                }
                return "no payment needed";
            }

            responseMessage = _paymentProcessorService.ProcessPayment(inv, payment);
            _invoiceRepository.SaveInvoice(inv);

            return responseMessage;
        }

    }
}
