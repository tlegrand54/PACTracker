using System.Collections.Generic;

namespace PACTracker.Core.Energy.DTOs
{
    public class YearSummaryDto
    {
        /// <summary>
        /// Year number (e.g., 2023)
        /// </summary>
        public int Year { get; set; }
        
        /// <summary>
        /// List of monthly summaries for the year
        /// </summary>
        public IReadOnlyList<MonthSummaryDto> MonthSummaries { get; set; } = new List<MonthSummaryDto>();

        /// <summary>
        /// Total electrical energy consumed for the year (kWh)
        /// </summary>
        public double YearProduction { get; set; }

        /// <summary>
        /// Total heat energy produced for the year (kWh)
        /// </summary>
        public double YearConsumption { get; set; }

        /// <summary>
        /// Year COP (Coefficient of Performance)
        /// </summary>
        public double YearCop => YearConsumption > 0 ? YearProduction / YearConsumption : 0;

        // True when at least one month has a record
        public bool HasData => MonthSummaries.Any(m => m.TotalProduction > 0 || m.TotalConsumption > 0);
    }
}
