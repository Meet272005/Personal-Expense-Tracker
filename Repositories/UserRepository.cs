using Microsoft.EntityFrameworkCore;
using Expense_managment.Data;
using Expense_managment.Models;

namespace Expense_managment.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            // In a real application, you should hash the password!
            // For now, we are matching plain text as per existing DB structure.
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User> RegisterAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserExistsByIdAsync(user.UserId))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
        
        private async Task<bool> UserExistsByIdAsync(int id)
        {
            return await _context.Users.AnyAsync(e => e.UserId == id);
        }
    }
}
