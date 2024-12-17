using System.ComponentModel.DataAnnotations;

namespace ProblemAssignment3.Entities
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Address1 is required")]
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }

        [Required(ErrorMessage = "City is required")]

        public string? City { get; set; } = null!;
        [RegularExpression(@"^[A-Za-z]{2}$", ErrorMessage = "Province/State must be a 2-letter code")]

        [Required(ErrorMessage = "Province is required")]

        public string? ProvinceOrState { get; set; } = null!;
        [RegularExpression(@"^\d{5}(-\d{4})?$|^[A-Za-z]\d[A-Za-z] \d[A-Za-z]\d$", ErrorMessage = "Invalid postal/zip code")]


        [Required(ErrorMessage = "Postal code is required")]
        public string? ZipOrPostalCode { get; set; } = null!;

        [Required(ErrorMessage = "Phone Number is required")]
        [RegularExpression(@"^\+?1?[-.\s]?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}$", ErrorMessage = "Invalid phone number")]

        public string? Phone { get; set; }
        public string? ContactLastName { get; set; }
        public string? ContactFirstName { get; set; }

        //[Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage ="Email is invalid")]
        public string? ContactEmail { get; set; }

        public bool IsDeleted { get; set; } = false;

        public ICollection<Invoice> Invoices { get; set; }=new List<Invoice>();
    }
}
