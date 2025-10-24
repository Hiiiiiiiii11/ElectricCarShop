using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface IImageStorageService
    {
        Task<string> UploadAsync(IFormFile file, string folder);
    }
}
