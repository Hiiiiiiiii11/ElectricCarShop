using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;

namespace OrderService.Services
{
    public class CloudinaryImageStorageService : IImageStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryImageStorageService(IConfiguration config)
        {
            // Ưu tiên ENV, fallback sang appsettings:CloudinarySettings
            var cloud = config["CLOUDINARY_CLOUD_NAME"] ?? config["CloudinarySettings:CloudName"];
            var key = config["CLOUDINARY_API_KEY"] ?? config["CloudinarySettings:ApiKey"];
            var secret = config["CLOUDINARY_API_SECRET"] ?? config["CloudinarySettings:ApiSecret"];

            if (string.IsNullOrWhiteSpace(cloud) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("Cloudinary config is missing. Set ENV CLOUDINARY_* or CloudinarySettings in appsettings.json");

            _cloudinary = new Cloudinary(new Account(cloud, key, secret));
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("File is empty.");

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };
            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException($"Upload failed: {result.Error?.Message}");

            return result.SecureUrl?.ToString() ?? result.Url?.ToString()!;
        }
    }
}
