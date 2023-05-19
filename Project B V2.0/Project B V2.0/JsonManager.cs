using Newtonsoft.Json;

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

            List<User> data = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText("gebruikers.json"));


            return data;
        }

        internal static List<string> DeserializeCodes()
        {
            if (!File.Exists("codes.json"))
            {
                return new List<string>();
            }

            List<string> data = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("codes.json"));

            return data;
        }

        internal static string SerializeGebruikers(List<User> data)
        {
            //TODO Implementeer logica om lijst / data te schrijven naar JSON
            if (File.Exists("gebruikers.json"))
            {
                File.Delete("gebruikers.json");
            }

            File.WriteAllText("gebruikers.json", JsonConvert.SerializeObject(data, Formatting.Indented));


            return "200: Success";
        }

        internal static string SerializeRondleidingen(List<Rondleiding> data)
        {
            //TODO Implementeer logica om lijst / data te schrijven naar JSON
            if (File.Exists("rondleidingen.json"))
            {
                File.Delete("rondleidingen.json");
            }

            File.WriteAllText("rondleidingen.json", JsonConvert.SerializeObject(data, Formatting.Indented));


            return "200: Success";
        }

        internal static List<Rondleiding> DeserializeRondleidingen()
        {
            //TODO Implementeer logica om lijst / data te schrijven naar JSON
            if (!File.Exists("rondleidingen.json"))
            {
                return new List<Rondleiding>();
            }

            List<Rondleiding> data = JsonConvert.DeserializeObject<List<Rondleiding>>(File.ReadAllText("rondleidingen.json"));


            return data;
        }

        internal static string SerializeRondleidingenWeekschema(List<RondleidingSettingsDayOfWeek> data)
        {
            //TODO Implementeer logica om lijst / data te schrijven naar JSON
            if (File.Exists("rondleidingenweekschema.json"))
            {
                File.Delete("rondleidingenweekschema.json");
            }

            File.WriteAllText("rondleidingenweekschema.json", JsonConvert.SerializeObject(data, Formatting.Indented));


            return "200: Success";
        }

        internal static List<RondleidingSettingsDayOfWeek> DeserializeRondleidingenWeekschema()
        {
            //TODO Implementeer logica om lijst / data te schrijven naar JSON
            if (!File.Exists("rondleidingenweekschema.json"))
            {
                return new List<RondleidingSettingsDayOfWeek>();
            }

            List<RondleidingSettingsDayOfWeek> data = JsonConvert.DeserializeObject<List<RondleidingSettingsDayOfWeek>>(File.ReadAllText("rondleidingenweekschema.json"));


            return data;
        }

    }
}
