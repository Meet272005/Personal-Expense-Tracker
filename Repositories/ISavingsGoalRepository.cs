using Expense_managment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Expense_managment.Repositories
{
    public interface ISavingsGoalRepository
    {
        Task<IEnumerable<SavingsGoal>> GetSavingsGoalsByUserAsync(int userId);
        Task<SavingsGoal?> GetSavingsGoalByIdAsync(int goalId, int userId);
        Task<SavingsGoal> AddAsync(SavingsGoal goal);
        Task UpdateAsync(SavingsGoal goal);
        Task<bool> DeleteAsync(int goalId, int userId);
    }
}
