namespace PACTracker.Models
{
    public class YearSummaryModel
    {
        /// <summary>
        /// Year number (e.g., 2023)
        /// </summary>
        public int Year { get; set; }
        
        /// <summary>
        /// List of monthly summaries for the year
        /// </summary>
        public List<MonthSummaryModel> MonthSummaries { get; set; } = new List<MonthSummaryModel>();

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
        public double YearCop { get; set; }
    }
}
