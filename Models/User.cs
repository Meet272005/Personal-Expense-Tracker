using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_managment.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Email { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string Password { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? ProfileImage { get; set; }

        // Navigation property
        public ICollection<Category>? Categories { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<Budget>? Budgets { get; set; }
        public ICollection<Wallet>? Wallets { get; set; }
        public ICollection<SavingsGoal>? SavingsGoals { get; set; }
    }
}
