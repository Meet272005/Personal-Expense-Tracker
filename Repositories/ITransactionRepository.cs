using Expense_managment.Models;

namespace Expense_managment.Repositories
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetTransactionsByUserAsync(int userId);
        Task<Transaction?> GetTransactionByIdAsync(int id, int userId);
        Task<Transaction> AddAsync(Transaction transaction);
        Task<Transaction> UpdateAsync(Transaction transaction);
        Task<bool> DeleteAsync(int id, int userId);
        Task<IEnumerable<Transaction>> GetPendingRecurringTransactionsAsync(DateTime date);
    }
}
