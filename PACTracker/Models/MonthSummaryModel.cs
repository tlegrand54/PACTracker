using System;
using System.Collections.Generic;
using System.Text;

namespace PACTracker.Models
{
    public class MonthSummaryModel
    {
        /// <summary>
        /// Month number (1-12)
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Name of the month (e.g., January, February, etc.)
        /// </summary>
        public string MonthName => new DateTime(1, Month, 1).ToString("MMMM");

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
        public double MonthCop { get; set; }
    }
}
