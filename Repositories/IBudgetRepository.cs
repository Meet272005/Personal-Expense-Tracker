using Expense_managment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Expense_managment.Repositories
{
    public interface IBudgetRepository
    {
        Task<IEnumerable<Budget>> GetBudgetsByUserAsync(int userId, int? month = null, int? year = null);
        Task<Budget?> GetBudgetByIdAsync(int budgetId);
        Task<Budget?> GetBudgetByCategoryAsync(int userId, int categoryId, int month, int year);
        Task<IEnumerable<Budget>> GetCurrentBudgetsAsync(int userId);
        Task<Budget> CreateBudgetAsync(Budget budget);
        Task UpdateBudgetAsync(Budget budget);
        Task DeleteBudgetAsync(int budgetId);
        Task<bool> BudgetExistsAsync(int budgetId);
    }
}
