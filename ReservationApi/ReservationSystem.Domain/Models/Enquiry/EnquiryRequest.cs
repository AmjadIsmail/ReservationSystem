using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationSystem.Domain.Models.Enquiry
{
    public class EnquiryRequest
    {
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Lsas Name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^(?i)[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$", ErrorMessage = "Invalid email format.")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Messege is required.")]
        public string Message { get; set; }
        public string Source { get; set; } = "Contact-Us";
    }
}
