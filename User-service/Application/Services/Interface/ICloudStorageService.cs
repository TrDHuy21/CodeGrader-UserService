using Common;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Interface
{
    public interface ICloudStorageService
    {
        Task<Result<string>> UploadFileAsync(IFormFile file, string fileName);
        Task<Result<bool>> DeleteFileAsync(string fileUrl);
        
    }
}
