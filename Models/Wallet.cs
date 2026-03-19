using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_managment.Models
{
    public class Wallet
    {
        [Key]
        public int WalletId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string Name { get; set; } // e.g., "Cash", "Bank Account", "Credit Card"

        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialBalance { get; set; } = 0;

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }
    }
}
