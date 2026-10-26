using PACTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PACTracker.Models
{
    public class EnergyModel
    {
        // Date when the data is set
        public DateTime Date { get; set; }

        // Electrical energy consumed (kWh)
        public double Consumption { get; set; }

        // Heat energy produced (kWh)
        public double Production { get; set; }

        // COP = production (kWh) ÷ consommation (kWh).
        public double Cop { get; set; }
    }
}
