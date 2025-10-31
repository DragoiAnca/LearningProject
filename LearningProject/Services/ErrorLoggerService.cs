using LearningProject.Data;
using LearningProject.Models;

namespace LearningProject.Services
{
    public class ErrorLoggerService
    {
        private readonly LearningProjectContext _context;

        public ErrorLoggerService(LearningProjectContext context)
        {
            _context = context;
        }

        public async Task LogErrorAsync(Exception ex)
        {
            var error = new ErrorLog
            {
                ErrorMessage = ex.Message,
                StackTrace = ex.StackTrace,
                DateOccurred = DateTime.Now
            };

            _context.ErrorLogs.Add(error);
            await _context.SaveChangesAsync();
        }
    }
}

