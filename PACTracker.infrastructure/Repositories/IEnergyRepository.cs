using PACTracker.Domain.Entities;

namespace PACTracker.Infrastructure.Repositories
{
    public interface IEnergyRepository
    {
        /// <summary>
        /// Get an energy entity by its date.
        /// </summary>
        /// <param name="date">Date of the energy entity to retrieve.</param>
        /// <param name="ct">Cancellation token to cancel the asynchronous operation.</param>
        /// <returns>Task representing the asynchronous operation, containing the energy entity if found; otherwise, null.</returns>
        Task<EnergyEntity?> GetByDateAsync(DateTime date, CancellationToken ct = default);

        /// <summary>
        /// Get all energy entities for a specific month and year.
        /// </summary>
        /// <param name="year">Year for which to retrieve entities.</param>
        /// <param name="month">Month for which to retrieve entities.</param>
        /// <param name="ct">Cancellation token to cancel the asynchronous operation.</param>
        /// <returns>Read-only list of energy entities corresponding to the specified month and year.</returns>
        Task<IReadOnlyList<EnergyEntity>> GetByMonthAsync(int year, int month, CancellationToken ct = default);

        /// <summary>
        /// Get all energy entities for a specific year.
        /// </summary>
        /// <param name="year">Year for which to retrieve entities.</param>
        /// <param name="ct">Cancellation token to cancel the asynchronous operation.</param>
        /// <returns>Task representing the asynchronous operation, containing the read-only list of energy entities.</returns>
        Task<IReadOnlyList<EnergyEntity>> GetByYearAsync(int year, CancellationToken ct = default);

        // Inserts the record, or updates the existing one for the same day
        Task UpsertAsync(EnergyEntity energy, CancellationToken ct = default);
    }
}
