namespace PACTracker.Domain.Entities
{
    public class EnergyEntity()
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public double Consumption { get; set; }

        public double Production { get; set; }

        public double Cop => Consumption > 0 ? Production / Consumption : 0;
    }
}
