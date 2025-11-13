using AnalyticRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticService.Services
{
    public interface IPredictionService
    {
        Task<List<PredictionResponseDto>> PredictAsync(List<PredictDemandDto> requests);
    }
}
