using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_managment.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        // Category Foreign Key
        [Required]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        // User Foreign Key
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int Amount { get; set; }

        [Column(TypeName = "nvarchar(75)")]
        public string? Note { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public bool IsRecurring { get; set; } = false;

        [Column(TypeName = "nvarchar(20)")]
        public string? RecurrenceFrequency { get; set; } // "Daily", "Weekly", "Monthly", "Yearly"

        public DateTime? NextRecurrenceDate { get; set; }

        // Optional Wallet Link
        public int? WalletId { get; set; }
        [ForeignKey("WalletId")]
        public Wallet? Wallet { get; set; }
    }
}
