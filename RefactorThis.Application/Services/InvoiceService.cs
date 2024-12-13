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

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }
        public string ProcessPayment(Payment payment)
        {
            var inv = _invoiceRepository.GetInvoice(payment.Reference);
            var responseMessage = string.Empty;

            if (inv == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }
            else
            {
                if (inv.Amount == 0)
                {
                    if (inv.Payments?.Any() == true)
                    {
                        throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
                    }

                    responseMessage = "no payment needed";
                }
                else
                {
                    responseMessage = HandlePayment(inv, payment);
                }
            }

            _invoiceRepository.SaveInvoice(inv);
            return responseMessage;
        }

        public string HandlePayment(Invoice inv, Payment payment)
        {
            var message = "";
            decimal totalPaid = inv.Payments?.Sum(x => x.Amount) ?? 0m;
            bool isWithPayment = totalPaid > 0;
            bool isFullyPaidPartial = (inv.Amount - inv.AmountPaid) == payment.Amount;
            bool isFullyPaid = inv.Amount == payment.Amount;
            if (isWithPayment)
            {
                if (totalPaid == inv.Amount)
                {
                    message = "invoice was already fully paid";
                }
                else if (payment.Amount > (inv.Amount - totalPaid))
                {
                    message = "the payment is greater than the partial amount remaining";
                }
                else
                {
                    inv.AmountPaid += payment.Amount;
                    switch (inv.Type)
                    {
                        case InvoiceType.Commercial:
                            inv.TaxAmount += payment.Amount * 0.14m;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(inv.Type), inv.Type, "Invoice type is not recognized or is invalid.");
                    }
                    inv.Payments.Add(payment);

                    message = isFullyPaidPartial
                     ? "final partial payment received, invoice is now fully paid"
                     : "another partial payment received, still not fully paid";
                }
            }
            else
            {
                if (payment.Amount > inv.Amount)
                {
                    message = "the payment is greater than the invoice amount";
                }
                else
                {
                    inv.AmountPaid = payment.Amount;
                    inv.TaxAmount = payment.Amount * 0.14m;
                    inv.Payments.Add(payment);

                    message = isFullyPaid
                    ? "invoice is now fully paid"
                    : "invoice is now partially paid";
                }
            }

            return message;
        }
    }
}
