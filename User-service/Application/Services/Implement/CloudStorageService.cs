using Application.Services.Interface;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Reflection;

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

        public async Task<Result<bool>> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                return Result<bool>.Failure("File is required");

            }
            try
            {
                var uri = new Uri(fileUrl);
                var segments = uri.AbsolutePath.Split("/");
                var filename = segments.Last();
                var publicId = "avatars/" + Path.GetFileNameWithoutExtension(filename);

                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Result == "ok" || result.Result == "not found")
                {
                    return Result<bool>.Success(true, "Delete file successful");
                }
                return Result<bool>.Failure("Delete file failed" + result.Result);

            }
            catch (Exception ex)
            {
                return Result<bool>.Failure("Delete file exception" + ex.Message);
            }       
        }

        public async Task<Result<string>> UploadFileAsync(IFormFile file, string fileName)
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = $"/{fileName}",
                Folder = "avatars",
                Overwrite = true,
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
