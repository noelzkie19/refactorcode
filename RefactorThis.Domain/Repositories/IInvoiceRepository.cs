﻿
using RefactorThis.Domain.Models;

namespace RefactorThis.Domain.IRepositories
{
    public interface IInvoiceRepository
    {
        Invoice GetInvoice(string reference);
        void SaveInvoice(Invoice invoice);
        void Add(Invoice invoice);
    }
}
