using System;
using System.Collections.Generic;
using System.Text;

namespace PACTracker.ViewModels
{
    public class PeriodOption
    {
        public string Label { get; set; } = string.Empty;
        public int FirstMonth { get; set; }
        public int LastMonth { get; set; }
    }
}
