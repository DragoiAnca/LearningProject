using LearningProject.Data;
using LearningProject.Models;
using LearningProject.Services.Impl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningProject.Services
{
    public class Users : IUsers
    {
        private readonly LearningProjectContext _context;

        public Users(LearningProjectContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _context.User
                                 .Include(u => u.roluri)
                                 .ToListAsync();
        }

        //Get
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.User
                                 .Include(u => u.roluri)
                                 .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.User
                .Include(u => u.roluri)
                .Include(u => u.Departamente)
                .FirstOrDefaultAsync(u => u.IdUser == id);
        }

        public async Task AddAsync(User user)
        {
            _context.User.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {

            var updatedUser = await _context.User.FirstOrDefaultAsync(u => u.IdUser == user.IdUser);

            if (updatedUser == null)
                throw new Exception("User not found");

            updatedUser.Email = user.Email;
            updatedUser.Name = user.Name;
            updatedUser.id_departament = user.id_departament;
            updatedUser.roluriID = user.roluriID; 

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
