namespace LearningProject.Services.Impl
{
    public interface IBufferedFileUploadService
    {
        Task<bool> UploadFile(IFormFile file, string path);
    }
}
