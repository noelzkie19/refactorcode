using RefactorThis.Domain.Models;

namespace RefactorThis.Domain.Services
{
    public interface IInvoiceService
    {
        string ProcessPayment(Payment payment);
    }
}
