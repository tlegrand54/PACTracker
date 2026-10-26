using System;
using System.Collections.Generic;
using System.Text;

namespace PACTracker.ViewModels
{
    public class YearCardViewModel
    {
        public int Year { get; set; }
        public double Production { get; set; }
        public double Consumption { get; set; }
        public double? Cop { get; set; }
    }
}
