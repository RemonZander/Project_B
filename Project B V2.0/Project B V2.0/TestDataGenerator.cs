using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_B_V2._0
{
    internal static class TestDataGenerator
    {
        internal static (List<int>, Exception) MaakUniekeCodes(int hoeveelheid)
        {
            List<int> codes = new();
            Random rnd = new Random();
            Exception ex = new Exception();
            try
            {
                for (int i = 0; i < hoeveelheid; i++)
                {
                    int code = rnd.Next(10000000, 99999999);

                    codes.Add(code);
                }
            }
            catch (Exception exception)
            {
                ex = exception;
            }
            return (codes, ex);
        }

        internal static (List<User>, Exception) MaakGebruikers(int hoeveelheid)
        {
            Exception ex = new Exception();
            (List<int>, Exception) UniekeCodes = MaakUniekeCodes(hoeveelheid);
            if (UniekeCodes.Item2.Message != "Exception of type 'System.Exception' was thrown.") return (new List<User>(), UniekeCodes.Item2);
            List<User> Users = new List<User>();
            Random rnd = new Random();
            try
            {
                for (int a = 0; a < hoeveelheid; a++)
                {
                    if (rnd.Next(1, 5) == 3)
                    {
                        Users.Add(new User
                        {
                            UniekeCode = UniekeCodes.Item1[a].ToString(),
                            Reservering = new DateTime(1, 1, 1),
                        });
                        continue;
                    }
                    Users.Add(new User
                    {
                        UniekeCode = UniekeCodes.Item1[a].ToString(),
                        Reservering = new DateTime(DateTime.Now.Year, rnd.Next(1, 13), rnd.Next(1, 29)),
                    });
                }
            }
            catch (Exception exception)
            {
                ex = exception;
            }
            return (Users, ex);
        }
    }
}
