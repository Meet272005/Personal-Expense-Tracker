using Expense_managment.Models;

namespace Expense_managment.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetCategoriesByUserAsync(int userId);
        Task<Category?> GetCategoryByIdAsync(int id, int userId);
        Task<Category> AddAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id, int userId);
    }
}
