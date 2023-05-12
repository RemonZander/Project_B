using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project_B_V2._0
{
    public class Rondleiding
    {
        public DateTime Datum { get; set; }

        public int MaxGrootte { get; set; } = 13;

        public int Bezetting { get; set; }

        public bool TourIsStarted { get; set; }
    }
}