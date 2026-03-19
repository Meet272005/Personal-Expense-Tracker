using Expense_managment.Data;
using Expense_managment.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Expense_managment.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly ApplicationDbContext _context;

        public WalletRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Wallet>> GetWalletsByUserAsync(int userId)
        {
            return await _context.Wallets
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task<Wallet?> GetWalletByIdAsync(int walletId, int userId)
        {
            return await _context.Wallets
                .FirstOrDefaultAsync(w => w.WalletId == walletId && w.UserId == userId);
        }

        public async Task<Wallet> CreateWalletAsync(Wallet wallet)
        {
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        public async Task UpdateWalletAsync(Wallet wallet)
        {
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteWalletAsync(int walletId, int userId)
        {
            var wallet = await GetWalletByIdAsync(walletId, userId);
            if (wallet == null) return false;

            _context.Wallets.Remove(wallet);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
