using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Project_B_V2._0
{
    /// <summary>
    /// This class is used to store the amount and max visitors of the tours per day.
    /// This is used for the week scedule.
    /// </summary>
    /// <param name="Day">This should be a day of the week.</param>
    /// <param name="Rondleidingen">this is the list of tuples where the date is the time notation of the tour and the int is the max amount of visitors</param>
    public class RondleidingSettingsDayOfWeek
    {
        public DayOfWeek Day { get; set; }

        public List<Tuple<TimeOnly, int>>? Rondleidingen { get; set; } 
    }
}
