using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservationApi.ReservationSystem.Domain.DB_Models
{
    [Table("manual_payment")]
    public class ManualPayment
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("payment_id")]
        public int PaymentId { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("city")]
        public string? City { get; set; }

        [Column("postal_code")]
        public string? PostalCode { get; set; }

        [Column("country")]
        public string? Country { get; set; }

        public DateTime? CreatedOn { get; set; }

        [Column("payment_status")]
        public bool? PaymentStatus { get; set; }

        [Column("booking_ref")]
        public string? BookingRef { get; set; }
    }
}
