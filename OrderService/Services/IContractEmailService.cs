using Microsoft.AspNetCore.Http;
using OrderRepository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface IContractEmailService
    {
        Task UploadAndUpdateContractEmailAsync(int contractId, string customerEmail, IFormFile file);
    }
}
