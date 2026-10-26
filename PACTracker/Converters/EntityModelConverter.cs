using PACTracker.Domain.Entities;
using PACTracker.Models;

namespace PACTracker.Converters
{
    internal static class EntityModelConverter
    {
        internal static EnergyModel ConvertEntityToModel(this EnergyEntity entity)
        {
            return new EnergyModel
            {
                Date = entity.Date,
                Consumption = entity.Consumption,
                Production = entity.Production,
                Cop = entity.Cop
            };
        }

    }
}
