using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Project_B_V2._0
{
    internal static class JsonManager
    {

        internal static List<User> DeserializeGebruikers()
        {
            //TODO Implementeer logica om lijst / data te schrijven naar JSON
            if (!File.Exists("gebruikers.json"))
            {
                return new List<User>();
            }

            List<User> data = JsonSerializer.Deserialize<List<User>>(File.ReadAllText("gebruikers.json"));


            return data;
        }

        internal static List<string> DeserializeCodes()
        {
            if (!File.Exists("codes.json"))
            {
                return new List<string>();
            }

            List<string> data = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("codes.json"));

            return data;
        }

        internal static string SerializeGebruikers(List<User> data)
        {
            //TODO Implementeer logica om lijst / data te schrijven naar JSON
            if (File.Exists("gebruikers.json"))
            {
                File.Delete("gebruikers.json");
            }

            File.WriteAllText("gebruikers.json", JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));


            return "200: Success";
        }

        internal static string SerializeCodes(List<string> data)
        {
            //TODO Implementeer logica om lijst / data te schrijven naar JSON
            if (File.Exists("codes.json"))
            {
                File.Delete("codes.json");
            }

            File.WriteAllText("codes.json", JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));


            return "200: Success";
        }

        internal static string SerializeRondleidingen(List<Rondleiding> data)
        {
            //TODO Implementeer logica om lijst / data te schrijven naar JSON
            if (File.Exists("rondleidingen.json"))
            {
                File.Delete("rondleidingen.json");
            }

            File.WriteAllText("rondleidingen.json", JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));


            return "200: Success";
        }

        internal static List<Rondleiding> DeserializeRondleidingen()
        {
            //TODO Implementeer logica om lijst / data te schrijven naar JSON
            if (!File.Exists("rondleidingen.json"))
            {
                return new List<Rondleiding>();
            }

            List<Rondleiding> data = JsonSerializer.Deserialize<List<Rondleiding>>(File.ReadAllText("rondleidingen.json"));


            return data;
        }
    }
}
