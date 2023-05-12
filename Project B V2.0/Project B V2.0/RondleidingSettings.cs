using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_B_V2._0
{
    /// <summary>
    /// This is a record. A record is a immutable object to store a small datastructure..
    /// This record is used to store the amount and max visitors of the tours per day.
    /// This is used for the year settings.
    /// </summary>
    /// <param name="Date">this should be the date for wich these tour settings will apply</param>
    /// <param name="Rondleidingen">this is the list of tuples where the date is the time notation of the tour and the int is the max amount of visitors</param>
    public record struct RondleidingSettings(DateOnly Date, List<Tuple<TimeOnly, int>> Rondleidingen);

    /// <summary>
    /// This is a record. A record is a immutable object to store a small datastructure..
    /// This record is used to store the amount and max visitors of the tours per day.
    /// This is used for the week scedule.
    /// </summary>
    /// <param name="Day">This should be a day of the week.</param>
    /// <param name="Rondleidingen">this is the list of tuples where the date is the time notation of the tour and the int is the max amount of visitors</param>
    public record struct RondleidingSettingsWeekly(DayOfWeek Day, List<Tuple<TimeOnly, int>> Rondleidingen);
}
