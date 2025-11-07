using OrderRepository.Model;
using OrderRepository.Model.OrderDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface IOrderDetailService
    {
        Task<OrderDetailResponse> CreateOrderDetailAsync(CreateOrderDetailRequest request);
        Task<OrderDetailResponse> UpdateOrderDetailPriceAsync(int orderDetailId, UpdateOrderDetailPriceRequest request);
        Task DeleteOrderDetailAsync(int orderDetailId);
        Task<OrderDetailResponse> GetOrderDetailByIdAsync(int orderDetailId);
        Task<IEnumerable<OrderDetailResponse>> GetOrderDetailsByOrderIdAsync(int orderId);
    }

}
