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
            //Implementeer logica om JSON uit te lezen
            var jsonData = File.ReadAllText("test.json");

            List<User> users = JsonSerializer.Deserialize<List<User>>(jsonData);

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
