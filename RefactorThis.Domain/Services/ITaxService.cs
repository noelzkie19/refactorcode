


using RefactorThis.Domain.Enum;

namespace RefactorThis.Domain.Services
{
    public interface ITaxService
    {
        decimal CalculateTax(decimal paymentAmount, InvoiceType invoiceType);
    }
}
