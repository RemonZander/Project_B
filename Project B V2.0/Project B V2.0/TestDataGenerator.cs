using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        internal static (List<medewerker>, Exception) MaakGitsen(int hoeveelheid)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%&*()-_=+[]{}|;',.<>/?";
            Exception ex = new Exception();
            List<medewerker> medewerker = new List<medewerker>();
            Random rnd = new Random();
            try
            {
                for (int a = 0; a < hoeveelheid; a++)
                {
                    string code = "";
                    for (int b = 0; b < 9; b++)
                    {
                        code += chars[rnd.Next(0, chars.Length)];
                    }
                    medewerker.Add(new medewerker {
                        BeveiligingsCode = code,
                        Role = Roles.Gids,
                    });
                }
            }
            catch (Exception exception)
            {
                ex = exception;
            }
            return (medewerker, ex);
        }

        internal static (List<Rondleiding>, Exception) MaakRondleidingen(DateTime start, DateTime end, bool useScedule)
        {
            List<RondleidingSettingsDayOfWeek> Weekschedule = new List<RondleidingSettingsDayOfWeek>();
            if (useScedule)
            {
                Weekschedule = JsonManager.DeserializeRondleidingenWeekschema();
            }
            else
            {
                Weekschedule = TestDataGenerator.MaakStdWeekschema();
            }
            Exception ex = new Exception();
            Random rnd = new Random();
            start = new DateTime(start.Year, start.Month, start.Day, 11, 0, 0);
            List<Rondleiding> rondleidingen = new List<Rondleiding>();
            try
            {
                do
                {
                    List<Tuple<TimeOnly, int>>? rondleidingenSchema = Weekschedule[(int)start.DayOfWeek - 1].Rondleidingen;
                    foreach (var rondleiding in rondleidingenSchema)
                    {
                        rondleidingen.Add(new Rondleiding
                        {
                            Datum = new DateTime(start.Year, start.Month, start.Day, rondleiding.Item1.Hour, rondleiding.Item1.Minute, 0),
                            MaxGrootte = rondleiding.Item2,
                        });
                    }

                    if (start.DayOfWeek == DayOfWeek.Saturday) start = start.AddDays(2);
                    else start = start.AddDays(1);
                } while (start < end);
            }
            catch (Exception exception)
            {
                ex = exception;
            }

            return (rondleidingen, ex);
        }

        internal static List<RondleidingSettingsDayOfWeek> MaakStdWeekschema() 
        {
            List<RondleidingSettingsDayOfWeek> dagInfo = new List<RondleidingSettingsDayOfWeek>();
            TimeOnly tijd = new TimeOnly(11, 0);

            for (int d = 1; d < 7; d++) 
            {
                
                dagInfo.Add(new RondleidingSettingsDayOfWeek());
                dagInfo[d - 1].Rondleidingen = new List<Tuple<TimeOnly, int>>();
                DayOfWeek dag = (DayOfWeek)d;
                dagInfo[d-1].Day = dag;

                for (int i = 0; i < 18; i++)
                {
                    dagInfo[d-1].Rondleidingen?.Add(Tuple.Create(tijd, 13));
                    tijd = tijd.AddMinutes(20);
                }
                tijd = new TimeOnly(11, 0);
            }
            return dagInfo;
        }
    }
}
