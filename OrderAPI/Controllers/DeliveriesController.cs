using Microsoft.AspNetCore.Mvc;
using OrderAPI.Controllers.Binding;
using OrderRepository.Model.Request;
using OrderService.Services;

namespace OrderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveriesController : ControllerBase
    {
        private readonly IDeliveryService _service;
        private readonly IImageStorageService _imageStore;

        public DeliveriesController(IDeliveryService service, IImageStorageService imageStore)
        {
            _service = service;
            _imageStore = imageStore;
        }

        // GET /api/deliveries?status=Delivered&from=2025-10-01&to=2025-10-31&page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] string? status, [FromQuery] DateTime? from,
                                              [FromQuery] DateTime? to, [FromQuery] int page = 1,
                                              [FromQuery] int pageSize = 20)
        {
            var (items, total) = await _service.ListAsync(status, from, to, page, pageSize);
            return Ok(new { total, page, pageSize, items });
        }

        // GET /api/deliveries/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var dto = await _service.GetAsync(id);
            return dto == null ? NotFound() : Ok(dto);
        }

        // GET /api/deliveries/by-order/{orderId}
        [HttpGet("by-order/{orderId:int}")]
        public async Task<IActionResult> ByOrder(int orderId)
        {
            var items = await _service.GetByOrderAsync(orderId);
            return Ok(items);
        }

        // POST /api/deliveries  (multipart/form-data)
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] DeliveryCreateForm form)
        {
            // Ưu tiên upload file nếu có
            string? beforeUrl = form.ImgUrlBefore;
            string? afterUrl = form.ImgUrlAfter;

            if (form.ImgBeforeFile != null && form.ImgBeforeFile.Length > 0)
                beforeUrl = await _imageStore.UploadAsync(form.ImgBeforeFile, "deliveries/before");

            if (form.ImgAfterFile != null && form.ImgAfterFile.Length > 0)
                afterUrl = await _imageStore.UploadAsync(form.ImgAfterFile, "deliveries/after");

            var req = new DeliveryCreateRequest
            {
                OrderId = form.OrderId,
                DeliveryDate = form.DeliveryDate,
                DeliveryStatus = form.DeliveryStatus,
                Notes = form.Notes,
                ImgUrlBefore = beforeUrl,
                ImgUrlAfter = afterUrl
            };

            var created = await _service.CreateAsync(req);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        // PUT /api/deliveries/{id} (multipart/form-data)
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] DeliveryUpdateForm form)
        {
            string? beforeUrl = form.ImgUrlBefore;
            string? afterUrl = form.ImgUrlAfter;

            if (form.ImgBeforeFile != null && form.ImgBeforeFile.Length > 0)
                beforeUrl = await _imageStore.UploadAsync(form.ImgBeforeFile, "deliveries/before");

            if (form.ImgAfterFile != null && form.ImgAfterFile.Length > 0)
                afterUrl = await _imageStore.UploadAsync(form.ImgAfterFile, "deliveries/after");

            var req = new DeliveryUpdateRequest
            {
                DeliveryDate = form.DeliveryDate,
                DeliveryStatus = form.DeliveryStatus,
                Notes = form.Notes,
                ImgUrlBefore = beforeUrl, // null = không đổi; string.Empty = xoá (nếu bạn muốn, xử lý thêm)
                ImgUrlAfter = afterUrl
            };

            var updated = await _service.UpdateAsync(id, req);
            return Ok(updated);
        }

        // DELETE /api/deliveries/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
