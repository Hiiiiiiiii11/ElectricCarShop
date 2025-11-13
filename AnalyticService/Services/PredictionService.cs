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
    startDate.Year,
    startDate.Month,
    endDate.Year,
    endDate.Month,
    null,     // vehicleId optional
    null      // agencyId optional
);

            // 2. Chuẩn bị Tensor đầu vào
            var inputData = new float[batchSize * _numFeatures];

            for (int i = 0; i < batchSize; i++)
            {
                var req = requests[i];

                // Tạo 1 hàng feature dựa trên TOÀN BỘ lịch sử của vehicle + agency
                var featureRow = CreateFeatureRow(req, historicalData);

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

        //private Monthly_Demand_Features FindHistory(IEnumerable<Monthly_Demand_Features> data, int vehicleId, int agencyId, int year, int month)
        //{
        //    var targetDate = new DateTime(year, month, 1);
        //    return data
        //        .Where(d => d.VehicleId == vehicleId && d.AgencyId == agencyId)
        //        .Where(d => new DateTime(d.Year, d.Month, 1) < targetDate)
        //        .OrderByDescending(d => d.Year)
        //        .ThenByDescending(d => d.Month)
        //        .FirstOrDefault();
        //}

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
        private float[] CreateFeatureRow(PredictDemandDto req, IEnumerable<Monthly_Demand_Features> allHistory)
        {
            var targetDate = new DateTime(req.Year, req.Month, 1);

            // Lấy toàn bộ lịch sử của (vehicleId, agencyId) trước tháng cần dự đoán
            var series = allHistory
                .Where(d => d.VehicleId == req.VehicleId
                         && d.AgencyId == req.AgencyId
                         && new DateTime(d.Year, d.Month, 1) < targetDate)
                .OrderByDescending(d => d.Year)
                .ThenByDescending(d => d.Month)
                .ToList();

            // Lấy các tháng gần nhất
            var last1 = series.ElementAtOrDefault(0);
            var last2 = series.ElementAtOrDefault(1);
            var last3 = series.ElementAtOrDefault(2);

            float unitsLast1 = last1?.UnitsSold ?? 0f;
            float unitsLast2 = last2?.UnitsSold ?? 0f;
            float unitsLast3 = last3?.UnitsSold ?? 0f;

            // Rolling 3, rolling 6
            var last3List = series.Take(3).Select(d => (float)d.UnitsSold).ToList();
            float rolling3 = last3List.Count > 0 ? last3List.Average() : 0f;

            var last6List = series.Take(6).Select(d => (float)d.UnitsSold).ToList();
            float rolling6 = last6List.Count > 0 ? last6List.Average() : 0f;

            float trend3 = rolling3 - rolling6;
            float momentum = unitsLast1 - rolling3;

            // Tỷ lệ hoàn thành chỉ tiêu tháng trước: unitsSoldLast1 / agencyTargetLastMonth
            float targetAchievedRate = 0f;
            if (last1 != null && last1.AgencyTarget > 0)
            {
                targetAchievedRate = (float)last1.UnitsSold / (float)last1.AgencyTarget;
            }

            // Chọn bản ghi dùng làm “tham số tĩnh” (giá, cấu hình xe, region…)
            var lastForStatics = last1 ?? series.LastOrDefault();

            float avgPrice = (float)(lastForStatics?.AvgPrice ?? 1_000_000_000);
            float testDrives = (float)(lastForStatics?.TestDrivesCount ?? 0);
            float quotations = (float)(lastForStatics?.QuotationsAcceptedCount ?? 0);

            // Giả định tương lai không khuyến mãi (hoặc em có thể cho user chọn trong UI)
            float wasOnPromotion = 0f;
            float promoAmount = 0f;

            float vehicleRange = lastForStatics?.VehicleRangeKM ?? 0f;

            int monthEncoded = EncodeCategory("month", req.Month.ToString());
            int vehicleEncoded = EncodeCategory("vehicleId", req.VehicleId.ToString());
            int agencyEncoded = EncodeCategory("agencyId", req.AgencyId.ToString());
            int regionEncoded = EncodeCategory("agencyRegion", lastForStatics?.AgencyRegion ?? "Unknown");
            int batteryEncoded = EncodeCategory("vehicleBatteryCapacity", lastForStatics?.VehicleBatteryCapacity ?? "Unknown");

            // Map theo ĐÚNG tên features đã dùng trong train.py
            var rowMap = new Dictionary<string, float>
            {
                ["month"] = monthEncoded,
                ["vehicleId"] = vehicleEncoded,
                ["agencyId"] = agencyEncoded,

                ["unitsSoldLast1"] = unitsLast1,
                ["unitsSoldLast2"] = unitsLast2,
                ["unitsSoldLast3"] = unitsLast3,

                ["rolling3"] = rolling3,
                ["rolling6"] = rolling6,
                ["trend3"] = trend3,
                ["momentum"] = momentum,
                ["targetAchievedRate"] = targetAchievedRate,

                ["avgPrice"] = avgPrice,
                ["wasOnPromotion"] = wasOnPromotion,
                ["promotionDiscountAmount"] = promoAmount,
                ["testDrivesCount"] = testDrives,
                ["quotationsAcceptedCount"] = quotations,

                ["vehicleRangeKM"] = vehicleRange,
                ["agencyRegion"] = regionEncoded,
                ["vehicleBatteryCapacity"] = batteryEncoded
            };

            var featureRow = new float[_numFeatures];

            // Đảm bảo đúng thứ tự feature như trong feature_list.json
            for (int i = 0; i < _numFeatures; i++)
            {
                string featureName = _featureOrder[i];

                if (!rowMap.TryGetValue(featureName, out featureRow[i]))
                {
                    _logger.LogWarning("Thiếu feature '{FeatureName}' khi tạo hàng dự đoán, gán 0.", featureName);
                    featureRow[i] = 0f;
                }
            }

            return featureRow;
        }
    }
}