using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Domain.Models;
using RefactorThis.Application.Services;
using RefactorThis.Domain.IRepositories;
using RefactorThis.Domain.Services;
using RefactorThis.Domain.Enum;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        private IInvoiceRepository _invoiceRepository;
        private IPaymentProcessorService _paymentProcessorService;
        private InvoiceService _invoiceService;

        public class TaxServiceStub : ITaxService
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

        public class MessageFormatterServiceStub : IMessageFormatterService
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

        [SetUp]
        public void SetUp()
        {
            var taxServiceStub = new TaxServiceStub();
            var messageFormatterServiceStub = new MessageFormatterServiceStub();
            _invoiceRepository = new FakeInvoiceRepository();
            _paymentProcessorService = new PaymentProcessorService(taxServiceStub, messageFormatterServiceStub);
            _invoiceService = new InvoiceService(_invoiceRepository, _paymentProcessorService);
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            var payment = new Payment { Reference = "123" };
            _invoiceRepository.Add(null);

            var exception = Assert.Throws<InvalidOperationException>(() => _invoiceService.ProcessPayment(payment));
            Assert.AreEqual("There is no invoice matching this payment", exception.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            var invoice = new Invoice { Amount = 0, AmountPaid = 0, Payments = new List<Payment>() };
            _invoiceRepository.Add(invoice);

            var payment = new Payment();
            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual("no payment needed", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment> { new Payment { Amount = 10 } }
            };
            _invoiceRepository.Add(invoice);

            var payment = new Payment();
            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } }
            };
            _invoiceRepository.Add(invoice);

            var payment = new Payment { Amount = 6 };
            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual("the payment is greater than the partial amount remaining", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            var invoice = new Invoice { Amount = 5, AmountPaid = 0, Payments = new List<Payment>() };
            _invoiceRepository.Add(invoice);

            var payment = new Payment { Amount = 6 };
            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } }
            };
            _invoiceRepository.Add(invoice);

            var payment = new Payment { Amount = 5 };
            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            var invoice = new Invoice { Amount = 10, AmountPaid = 0, Payments = new List<Payment>() };
            _invoiceRepository.Add(invoice);

            var payment = new Payment { Amount = 1 };
            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual("invoice is now partially paid", result);
        }
    }

    public class FakeInvoiceRepository : IInvoiceRepository
    {
        private Invoice _invoice;

        public Invoice GetInvoice(string reference)
        {
            return _invoice;
        }

        public void SaveInvoice(Invoice invoice)
        {
            //saves the invoice to the database
        }

        public void Add(Invoice invoice)
        {
            _invoice = invoice;
        }
    }

}
