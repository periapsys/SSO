namespace PERI.SK.Domain.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
