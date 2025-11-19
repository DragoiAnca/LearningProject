using LearningProject.Services.Impl;

namespace LearningProject.Services
{
    public class BufferedFileUploadLocalService : IBufferedFileUploadService
    {
     public async Task<bool> UploadFile(IFormFile file, string path)
        {
            try
            {
                if (file.Length > 0)
                {
                    // Creează folderul dacă nu există
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    var filePath = Path.Combine(path, file.FileName);

                    // Dacă fișierul există deja, fă backup cu timestamp
                    if (File.Exists(filePath))
                    {
                        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        File.Copy(filePath, Path.Combine(path, $"{timestamp}-{file.FileName}"));
                    }

                    // Scrie fișierul
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("File Copy Failed", ex);
            }
        }
}
}