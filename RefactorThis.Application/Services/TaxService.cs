

using RefactorThis.Domain.Enum;
using RefactorThis.Domain.Services;
using System;

namespace RefactorThis.Application.Services
{
    public class TaxService : ITaxService
    {
        public decimal CalculateTax(decimal paymentAmount, InvoiceType invoiceType)
        {
            if (invoiceType == InvoiceType.Commercial)
            {
                return paymentAmount * 0.14m;
            }

            throw new ArgumentOutOfRangeException(nameof(invoiceType), invoiceType, "Invoice type is not recognized or is invalid.");
        }
    }
}
