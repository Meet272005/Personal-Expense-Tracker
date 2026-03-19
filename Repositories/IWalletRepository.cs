using Expense_managment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Expense_managment.Repositories
{
    public interface IWalletRepository
    {
        Task<IEnumerable<Wallet>> GetWalletsByUserAsync(int userId);
        Task<Wallet?> GetWalletByIdAsync(int walletId, int userId);
        Task<Wallet> CreateWalletAsync(Wallet wallet);
        Task UpdateWalletAsync(Wallet wallet);
        Task<bool> DeleteWalletAsync(int walletId, int userId);
    }
}
