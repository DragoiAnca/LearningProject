using LearningProject.Models;

namespace LearningProject.Services.Impl
{
    public interface IUsers
    {
        Task<List<User>> GetAllUsers();
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
    }
}
