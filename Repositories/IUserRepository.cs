using Expense_managment.Models;

namespace Expense_managment.Repositories
{
    public interface IUserRepository
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User> RegisterAsync(User user);
        Task<bool> UserExistsAsync(string email);
        Task<bool> UpdateUserAsync(User user);
    }
}
