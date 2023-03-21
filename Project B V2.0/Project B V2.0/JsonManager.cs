using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Project_B_V2._0
{
    static class JsonManager
    {
 
        public static List<User> DeserializeCodes()
        {
            //TODO Implementeer logica om JSON uit te lezen
            StreamReader stream = new("test.json");

            var data = stream.ReadToEnd();

            List<User> users = JsonSerializer.Deserialize<List<User>>(data);

            return users;
        }

        public static string SerializeResults(List<User> data)
        {
            //TODO Implementeer logica om lijst / data te schrijven naar JSON
            var json = JsonSerializer.Serialize(data);

            StreamReader stream = new("test.json");

            
            return "200: Success";
        }
    }
}
