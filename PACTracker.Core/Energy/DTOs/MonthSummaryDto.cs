using System;
using System.Collections.Generic;
using System.Text;

namespace PACTracker.Core.Energy.DTOs
{
    public class MonthSummaryDto
    {
        /// <summary>
        /// Month number (1-12)
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Total electrical energy consumed (kWh)
        /// </summary>
        public double TotalConsumption { get; set; }

        /// <summary>
        /// Total heat energy produced (kWh)
        /// </summary>
        public double TotalProduction { get; set; }

        /// <summary>
        /// Monthly COP (Coefficient of Performance)
        /// </summary>
        public double MonthCop => TotalConsumption > 0 ? TotalProduction / TotalConsumption : 0;
    }
}
