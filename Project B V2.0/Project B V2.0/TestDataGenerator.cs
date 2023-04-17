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
            List<Rondleiding> rondleidingen = JsonManager.DeserializeRondleidingen();
            List<Rondleiding> rondleidingenNew = new List<Rondleiding>();

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
                    int nextRondleiding = rnd.Next(0, rondleidingen.Count);
                    Users.Add(new User
                    {
                        UniekeCode = UniekeCodes.Item1[a].ToString(),
                        Reservering = rondleidingen[nextRondleiding].Datum,
                    });
                    rondleidingen[nextRondleiding].Bezetting += 1;
                    if (rondleidingen[nextRondleiding].Bezetting >= 13)
                    {
                        rondleidingenNew.Add(rondleidingen[nextRondleiding]);
                        rondleidingen.RemoveAt(nextRondleiding);
                    }
                }

                rondleidingenNew.AddRange(rondleidingen);
                JsonManager.SerializeRondleidingen(rondleidingenNew);
            }
            catch (Exception exception)
            {
                ex = exception;
            }
            return (Users, ex);
        }

        internal static (List<Rondleiding>, Exception) MaakRondleidingen(DateTime start, DateTime end)
        {
            Exception ex = new Exception();
            Random rnd = new Random();
            List<Rondleiding> rondleidingen = new List<Rondleiding>();
            try
            {
                do
                {
                    rondleidingen.Add(new Rondleiding
                    {
                        Datum = start,
                        Bezettingsgraad = rnd.Next(1, 14)
                    });

                    if (start.Hour == 16 && start.Minute > 20 || start.Hour > 16)
                    {
                        start = start.AddDays(1);
                        start = start.AddHours(-start.Hour + 11);
                        start = start.AddMinutes(-start.Minute);
                    }
                    else
                    {
                        start = start.AddMinutes(20);

                    }
                } while (start < end);
            }
            catch (Exception exception)
            {
                ex = exception;
            }

            return (rondleidingen, ex);
        }
    }
}
