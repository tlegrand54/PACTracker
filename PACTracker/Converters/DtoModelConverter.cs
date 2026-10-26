using PACTracker.Core.Energy.DTOs;
using PACTracker.Models;

namespace PACTracker.Converters
{
    internal static class DtoModelConverter
    {
        internal static MonthSummaryModel ConvertDtoToModel(this MonthSummaryDto dto)
        {
            return new MonthSummaryModel
            {
                Month = dto.Month,
                TotalConsumption = dto.TotalConsumption,
                TotalProduction = dto.TotalProduction,
                MonthCop = dto.MonthCop
            };
        }

        internal static YearSummaryModel ConvertDtoToModel(this YearSummaryDto dto)
        {
            var yearSummaryModel = new YearSummaryModel
            {
                Year = dto.Year,
                YearProduction = dto.YearProduction,
                YearConsumption = dto.YearConsumption
            };
            
            foreach (var monthSummaryDto in dto.MonthSummaries)
            {
                yearSummaryModel.MonthSummaries.Add(monthSummaryDto.ConvertDtoToModel());
            }
            
            return yearSummaryModel;
        }
    }
}
