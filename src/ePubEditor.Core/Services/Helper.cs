using CsvHelper;
using CsvHelper.Configuration;
using ePubEditor.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ePubEditor.Core.Services
{
    internal class Helper
    {
        internal async static Task<T> LoadObjectFromJson<T>(string jsonFileName)
        {
            // new($"{AppContext.BaseDirectory}/Ressources/stations.json"))
            string jsonPath = $"{AppContext.BaseDirectory}\\Ressources\\{jsonFileName}.json";
            string jsonContent = File.ReadAllText(jsonPath);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            T data = JsonSerializer.Deserialize<T>(jsonContent, options);
            return await Task.FromResult(data);
        }

        internal static List<T> LoadObjectsFromCSV<T>(string csvFileName)
        {
            // new($"{AppContext.BaseDirectory}/Ressources/stations.json"))
            string csvPath = $"{AppContext.BaseDirectory}\\Ressources\\{csvFileName}.csv";


            using (StreamReader reader = new StreamReader(csvPath))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    ShouldSkipRecord = args =>
                    {
                        if (args.Row.Parser.RawRow > 10)
                        {
                            var rawRow = args.Row.Parser.RawRecord;
                            if (rawRow.StartsWith("\"Date\",\"Time\",")) return true;
                        }

                        return false;
                    }
                };


                using (CsvReader csv = new CsvReader(reader, config))
                {
                    if (typeof(T) == typeof(InitialMetadata))
                    {
                        csv.Context.RegisterClassMap<InitialMetadataMap>();
                    }

                    IEnumerable<T> records = csv.GetRecords<T>();
                    return records.ToList();
                }
            }
        }
    }
}
