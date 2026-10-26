using PACTracker.Core.Energy.DTOs;
using PACTracker.Domain.Entities;

namespace PACTracker.Core.Energy.Services
{
    public interface IEnergyService
    {

        /// <summary>
        /// Get all record for a dedicated month and year
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        Task<IReadOnlyList<EnergyEntity>> GetAllRecordsByMonthAsync(int year, int month);

        /// <summary>
        /// Get all records for a dedicated year
        /// </summary>
        /// <param name="year">Year for which to retrieve records.</param>
        /// <returns>A task representing the asynchronous operation, containing a read-only list of energy entities.</returns>
        Task<YearSummaryDto> GetAllRecordsByYearAsync(int year);

        /// <summary>
        /// Get a record by the year, the month and the day
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns>return an EnergyRecord from the list otherwise return null </returns>
        EnergyEntity? GetEnergyRecord(int year, int month, int day, IReadOnlyList<EnergyEntity> energyRecords);

        /// <summary>
        /// Create and save a new EnergyRecord
        /// </summary>
        /// <param name="date"></param>
        /// <param name="consumption"></param>
        /// <param name="production"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SaveRecordAsync(DateTime date, double consumption, double production, CancellationToken ct = default);
    }
}
