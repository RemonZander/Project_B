using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Project_B_V2._0
{
    internal class RondleidingenAdviesAlgoritme
    {
        public RondleidingenAdviesAlgoritme() { }

        Func<int, int, int> calcPercentage = (x, y) => x * 100 / y;

        public List<int> BerekenGemiddeldeBezettingPerDag(List<List<Rondleiding>> data)
        {
            int averageCoverage = 0;
            List<int> averageCoverageList = new();

            foreach (List<Rondleiding> rondleidingen in data)
            {
                if (rondleidingen.Count() == 0)
                {
                    averageCoverageList.Add(0);
                }
                else
                {
                    rondleidingen.ForEach(r => averageCoverage += r.Bezetting);
                    averageCoverage /= rondleidingen.Count();
                    averageCoverageList.Add(averageCoverage);
                }
            }

            return averageCoverageList;
        }

        public List<int> BerekenGemiddeldeGroepsgroottePerDag(List<List<Rondleiding>> data)
        {
            int averageSize = 0;
            List<int> averageSizeList = new();

            foreach (List<Rondleiding> rondleidingen in data)
            {
                if (rondleidingen.Count() == 0)
                {
                    averageSizeList.Add(0);
                }
                else
                {
                    rondleidingen.ForEach(r => averageSize += r.MaxGrootte);
                    averageSize /= rondleidingen.Count();
                    averageSizeList.Add(averageSize);
                }
            }

            return averageSizeList;
        }

        public string PrintAdviesPerDag(List<int> sizeList, List<int> coverageList)
        {
            string advicePerDay = "";

            string[] days = { "maandag", "dinsdag", "woensdag", "donderdag", "vrijdag" };

            try
            {
                if (sizeList.Count() == coverageList.Count())
                {
                    for (int i = 0; i < sizeList.Count(); i++)
                    {
                        if (coverageList[i] <= 0 || sizeList[i] <= 0)
                        {
                            continue;
                        }
                        if (calcPercentage(coverageList[i], sizeList[i]) <= 30)
                        {
                            advicePerDay += $"Verlaag de groeps grootte of verlaag het aantal rondleidingen voor {days[i]}\n";
                        }
                        else if (calcPercentage(coverageList[i], sizeList[i]) > 30 && calcPercentage(coverageList[i], sizeList[i]) < 70)
                        {
                            advicePerDay += $"Verlaag de groeps grootte met een klein aantal, ongeveer 2 plaatsen voor {days[i]}\n";
                        }
                        else if (calcPercentage(coverageList[i], sizeList[i]) >= 100)
                        {
                            advicePerDay += $"Verhoog de groeps grootte met een klein aantal, ongeveer 2 plaatsen voor {days[i]}\n";
                        }
                        else
                        {
                            advicePerDay += $"Geen aanpassingen nodig voor {days[i]}\n";
                        }
                    }
                }
            }
            catch
            {
                return "Er is een fout opgetreden tijdens het verwerken van de adviezen!";
            }

            return $"Het advies luid: \n{advicePerDay}";
        }

        public string PrintAdviesPerRondleiding(List<Rondleiding> data)
        {
            string advicePerRondleiding = "";
            DateTime current = DateTime.Now;

            foreach(Rondleiding rondleiding in data)
            {
                if (rondleiding.Datum > current) continue; //Skip if rondleiding hasn't occured yet

                int bezettingsgraad = calcPercentage(rondleiding.Bezetting, rondleiding.MaxGrootte);

                if (bezettingsgraad < 30 && rondleiding.MaxGrootte <= 5)
                {
                    advicePerRondleiding += $"De bezetting is {bezettingsgraad}% op {rondleiding.Datum}, advies is om deze rondleiding te verwijderen.";
                }
                else if (bezettingsgraad < 30)
                {
                    advicePerRondleiding += $"De bezetting is {bezettingsgraad}% op {rondleiding.Datum}, verlaag de groeps grootte met ongeveer 50%\n";
                }
                else if (bezettingsgraad > 30 && bezettingsgraad < 70)
                {
                    advicePerRondleiding += $"De bezetting is {bezettingsgraad}% op {rondleiding.Datum}, verlaag de groeps grootte met ongeveer 1 of 2 plaatsen\n";
                }
                else if (bezettingsgraad > 70 && bezettingsgraad < 90)
                {
                    advicePerRondleiding += $"De bezetting is {bezettingsgraad}% op {rondleiding.Datum}, geen veranderingen nodig\n";
                }
                else if(bezettingsgraad >= 100)
                {
                    advicePerRondleiding += $"De bezetting is {bezettingsgraad}% op {rondleiding.Datum}, verhoog de groeps grootte met 1 of 2 plaatsen\n";
                }
            }

            return $"Het advies luid: \n{advicePerRondleiding}";
        }
    }
}
