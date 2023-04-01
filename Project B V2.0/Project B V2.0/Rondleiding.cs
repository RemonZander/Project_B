using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project_B_V2._0
{
    public class Rondleiding
    {
        public DateTime Datum { get; set; }

        public int Bezettingsgraad { get; set; }

        public int CalculateBezettingsgraad(List<User> users)
        {
            List<User> reserverdUsers = new List<User>();

            foreach (User user in users)
            {
                if(user.Reservering == Datum) reserverdUsers.Add(user);
            }

            Bezettingsgraad = (reserverdUsers.Count / 13) * 100;

            return Bezettingsgraad;
        }
    }
}