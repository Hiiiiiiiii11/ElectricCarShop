using AnalyticRepository.Model;
using AnalyticRepository.Model.DTO;
using AnalyticRepository.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AnalyticService.Services
{
    public class PredictionService : IPredictionService
    {
        private readonly ILogger<PredictionService> _logger;
        private readonly IAnalyticRepository _analyticRepo;
        private readonly InferenceSession _session;
        private readonly Dictionary<string, Dictionary<string, int>> _mappings;
        private readonly List<string> _featureOrder;
        private readonly int _numFeatures;

        public PredictionService(
            ILogger<PredictionService> logger,
            IAnalyticRepository analyticRepo,
            IHostEnvironment environment)
        {
            _logger = logger;
            _analyticRepo = analyticRepo;

            try
            {
                string contentRootPath = environment.ContentRootPath;
                string modelPath = Path.Combine(contentRootPath, "model_artifacts", "demand_model.onnx");
                string mappingsPath = Path.Combine(contentRootPath, "model_artifacts", "category_mappings.json");
                string featuresPath = Path.Combine(contentRootPath, "model_artifacts", "feature_list.json");

                _session = new InferenceSession(modelPath);
                _mappings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(
                    File.ReadAllText(mappingsPath)
                );
                _featureOrder = JsonSerializer.Deserialize<List<string>>(
                    File.ReadAllText(featuresPath)
                );
                _numFeatures = _featureOrder.Count;

                _logger.LogInformation("ONNX Model, Mappings, and {NumFeatures} Features loaded successfully.", _numFeatures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LOI NGHIEM TRONG: Khong the tai ML model artifacts.");
                throw;
            }
        }

        public async Task<List<PredictionResponseDto>> PredictAsync(List<PredictDemandDto> requests)
        {
            int batchSize = requests.Count;
            if (batchSize == 0) return new List<PredictionResponseDto>();

            _logger.LogInformation("Nhan duoc {BatchSize} yeu cau du doan...", batchSize);

            // 1. Lấy dữ liệu lịch sử (1 lần gọi)
            var firstReq = requests[0];
            var (startDate, endDate) = GetHistoricalDateRange(firstReq.Year, firstReq.Month);
            var historicalData = await _analyticRepo.GetFeaturesByDateRange(
                startDate.Year, startDate.Month, endDate.Year, endDate.Month);

            // 2. Chuẩn bị Tensor đầu vào
            var inputData = new float[batchSize * _numFeatures];

            for (int i = 0; i < batchSize; i++)
            {
                var req = requests[i];

                // Tìm dòng lịch sử GẦN NHẤT cho (vehicle, agency)
                var history = FindHistory(historicalData, req.VehicleId, req.AgencyId, req.Year, req.Month);

                // Tạo 1 hàng feature (ĐÃ SỬA)
                var featureRow = CreateFeatureRow(req, history);

                int offset = i * _numFeatures;
                for (int j = 0; j < _numFeatures; j++)
                {
                    inputData[offset + j] = featureRow[j];
                }
            }

            // 3. Chạy ONNX Model
            var inputTensor = new DenseTensor<float>(inputData, new[] { batchSize, _numFeatures });
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
            };

            using var results = _session.Run(inputs);

            // 4. Lấy kết quả
            var outputTensor = results.FirstOrDefault(r => r.Name == "variable")?.AsTensor<float>(); // Sửa: Dùng 'variable'
            if (outputTensor == null)
            {
                var outputNames = string.Join(", ", results.Select(r => r.Name));
                _logger.LogError("Không tìm thấy output node 'variable'. Các node tìm thấy: [{OutputNames}]", outputNames);
                outputTensor = results.FirstOrDefault(r => r.Name == "output_label")?.AsTensor<float>(); // Thử 'output_label'
                if (outputTensor == null)
                {
                    throw new Exception($"Không tìm thấy output node 'variable' hoặc 'output_label'. Các node có sẵn: {outputNames}");
                }
            }

            // 5. Map kết quả
            var responseList = new List<PredictionResponseDto>();
            for (int i = 0; i < batchSize; i++)
            {
                responseList.Add(new PredictionResponseDto
                {
                    VehicleId = requests[i].VehicleId,
                    AgencyId = requests[i].AgencyId,
                    Year = requests[i].Year,
                    Month = requests[i].Month,
                    ForecastedUnits = (int)Math.Max(0, Math.Round(outputTensor[i]))
                });
            }
            return responseList;
        }

        // === CÁC HÀM HELPER ===

        private (DateTime Start, DateTime End) GetHistoricalDateRange(int predictYear, int predictMonth)
        {
            var endDate = new DateTime(predictYear, predictMonth, 1).AddDays(-1);
            var startDate = endDate.AddYears(-1).AddDays(1);
            return (startDate, endDate);
        }

        private Monthly_Demand_Features FindHistory(IEnumerable<Monthly_Demand_Features> data, int vehicleId, int agencyId, int year, int month)
        {
            var targetDate = new DateTime(year, month, 1);
            return data
                .Where(d => d.VehicleId == vehicleId && d.AgencyId == agencyId)
                .Where(d => new DateTime(d.Year, d.Month, 1) < targetDate)
                .OrderByDescending(d => d.Year)
                .ThenByDescending(d => d.Month)
                .FirstOrDefault();
        }

        private int EncodeCategory(string key, string value)
        {
            if (_mappings.TryGetValue(key, out var map))
            {
                if (map.TryGetValue(value, out var code)) { return code; }
            }
            _logger.LogWarning("Không tìm thấy mapping cho key='{Key}', value='{Value}'. Trả về 0.", key, value);
            return 0;
        }

        // === LOGIC TẠO FEATURE ĐÃ SỬA ===
        private float[] CreateFeatureRow(PredictDemandDto req, Monthly_Demand_Features history)
        {
            // Tự động điền các trường bị thiếu từ lịch sử (hoặc giá trị mặc định)
            // Model vẫn cần 17 features, nhưng UI chỉ gửi 4

            var rowMap = new Dictionary<string, float>
            {
                // Dữ liệu từ request (4 trường)
                ["month"] = EncodeCategory("month", req.Month.ToString()),
                ["vehicleId"] = EncodeCategory("vehicleId", req.VehicleId.ToString()),
                ["agencyId"] = EncodeCategory("agencyId", req.AgencyId.ToString()),
                ["year"] = req.Year, // (Mặc dù 'year' không trong list, nhưng để đây)

                // Dữ liệu TỰ SUY LUẬN (dựa trên lịch sử)
                ["avgPrice"] = (float)(history?.AvgPrice ?? 1000000000), // Lấy giá của tháng trước
                ["wasOnPromotion"] = 0f, // Giả định TƯƠNG LAI không có KM
                ["promotionDiscountAmount"] = 0f,
                ["agencyTarget"] = (float)(history?.AgencyTarget ?? 10), // Lấy target tháng trước

                // Dữ liệu TỪ LỊCH SỬ (Lag/Rolling)
                ["unitsSoldLastMonth"] = history?.UnitsSold ?? 0f,
                ["unitsSoldSameMonthLastYear"] = history?.UnitsSoldSameMonthLastYear ?? 0f,
                ["rollingAvgSales3Months"] = (float)(history?.RollingAvgSales3Months ?? 0.0),
                ["rollingAvgSales6Months"] = (float)(history?.RollingAvgSales6Months ?? 0.0),

                // Dữ liệu TĨNH (Lấy từ lịch sử)
                ["vehicleRangeKM"] = history?.VehicleRangeKM ?? 0f,
                ["agencyRegion"] = EncodeCategory("agencyRegion", history?.AgencyRegion ?? "Unknown"),
                ["vehicleBatteryCapacity"] = EncodeCategory("vehicleBatteryCapacity", history?.VehicleBatteryCapacity ?? "Unknown"),

                // Dữ liệu KHÔNG DÙNG ĐỂ DỰ ĐOÁN (Chống rò rỉ target)
                ["testDrivesCount"] = 0f,
                ["quotationsAcceptedCount"] = 0f,
                ["agencyOrdersQuantity"] = 0f // (QUAN TRỌNG: Không dùng proxy nữa)
            };

            // Sắp xếp mảng theo đúng thứ tự
            var featureRow = new float[_numFeatures];
            for (int i = 0; i < _numFeatures; i++)
            {
                string featureName = _featureOrder[i];
                if (!rowMap.TryGetValue(featureName, out featureRow[i]))
                {
                    _logger.LogError("Thieu feature '{FeatureName}' khi tao hang du doan!", featureName);
                }
            }
            return featureRow;
        }
    }
}