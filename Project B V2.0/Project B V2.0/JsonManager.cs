using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_B_V2._0
{
    static class JsonManager
    {
 
        public static string DeserializeCodes()
        {
            //TODO Implementeer logica om JSON uit te lezen
            StreamReader stream = new("test.json");

            var data = stream.ReadToEnd();

            return data;
        }

        public string SerializeResults(List<string> data)
        {
            //TODO Implementeer logica om lijst / data te schrijven naar JSON
        }
    }
}
