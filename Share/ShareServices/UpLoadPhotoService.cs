using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public class UpLoadPhotoService : IUploadPhotoService
    {
        private readonly Cloudinary _cloudinary;
        public UpLoadPhotoService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }   
        public string UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty", nameof(file));
            }
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Transformation = new Transformation().Quality("auto").FetchFormat("auto").Width(500).Height(500).Crop("fill").Gravity("face")
            };
            var uploadResult = _cloudinary.Upload(uploadParams);
            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");
            }
            return uploadResult.SecureUrl.AbsoluteUri;
        }
    }
}
