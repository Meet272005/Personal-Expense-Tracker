using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_managment.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(5)")]
        public string Icon { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(10)")]
        public string Type { get; set; }

        // Foreign Key
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<Budget>? Budgets { get; set; }
    }
}
