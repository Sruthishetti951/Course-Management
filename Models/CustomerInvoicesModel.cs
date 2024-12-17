using ProblemAssignment3.Entities;

namespace ProblemAssignment3.Models
{
    public class CustomerInvoicesModel
    {
        public Customer? Customer { get; set; }
        public Invoice? SelectedInvoice { get; set; }
        public ICollection<PaymentTerms>? PaymentTerms { get; set; }
        public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    }
}
