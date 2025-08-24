using Application.Services.Interface;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Application.Services.Implement
{
    public class CloudStorageService : ICloudStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudStorageService(IConfiguration configuration)
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
             );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<Result<string>> UploadFileAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "avatars",
                Transformation = new Transformation().Width(300).Height(300).Crop("fill").Gravity("face")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Result<string>.Success(uploadResult.SecureUrl.ToString(), "Upload file successful");
            }
            return Result<string>.Failure("Upload file failed");
        }
    }
}
