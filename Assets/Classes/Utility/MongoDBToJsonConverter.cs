using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class MongoDBToJsonConverter
{
    public static string ConvertMongoDBJsonToStandardJson(string mongodbJson)
    {
        var token = JToken.Parse(mongodbJson);

        // Rimuovere o trasformare i campi specifici di MongoDB
        ConvertToken(token);

        return token.ToString();
    }

    private static void ConvertToken(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
                ConvertObject((JObject)token);
                break;
            case JTokenType.Array:
                foreach (var item in token.Children())
                {
                    ConvertToken(item);
                }
                break;
            default:
                // Non necessita di conversione
                break;
        }
    }

    private static void ConvertObject(JObject obj)
    {
        var propertiesToRemove = new List<string>();
        foreach (var property in obj.Properties())
        {
            if (property.Value is JObject valueObj)
            {
                if (valueObj.TryGetValue("$oid", out var oid))
                {
                    obj[property.Name] = oid.ToString();
                }
                else if (valueObj.TryGetValue("$numberLong", out var numberLong))
                {
                    obj[property.Name] = long.Parse(numberLong.ToString());
                }
                else if (valueObj.TryGetValue("$date", out var date))
                {
                    // Assumendo che il valore sia in formato ISO 8601 o un timestamp Unix
                    // Converti in un formato di data/ora leggibile o in un timestamp Unix come stringa
                    obj[property.Name] = date["$date"].ToString();
                }
                else
                {
                    ConvertToken(property.Value);
                }
            }
            else if (property.Value is JArray)
            {
                ConvertToken(property.Value);
            }
        }

        foreach (var property in propertiesToRemove)
        {
            obj.Remove(property);
        }
    }

}

