// JsonConverter.cs
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;
using UnityEngine;
using System;

public static class JsonConverter
{
    public static string ConvertMongoToJsonStandard(string filePath)
    {
        UnityEngine.Debug.Log("Processing file: " + filePath);
        if (!File.Exists(filePath))
        {
            UnityEngine.Debug.LogError("File not found: " + filePath);
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(json))
            {
                UnityEngine.Debug.LogError("Empty JSON content in file: " + filePath);
                return null;
            }

            var document = JToken.Parse(json);
            var newDocument = ConvertNode(document);

            return newDocument.ToString();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error processing JSON file: " + ex.Message);
            return null;
        }
    }

    private static JToken ConvertNode(JToken node)
    {
        switch (node.Type)
        {
            case JTokenType.Object:
                var newObj = new JObject();
                foreach (var property in node.Children<JProperty>())
                {
                    if (property.Name == "_id" && property.Value["$oid"] != null)
                    {
                        // Convert MongoDB _id.$oid to a simple string Id
                        newObj.Add("id0", property.Value["$oid"].ToString());
                    }
                    else if (property.Value is JObject && property.Value["$numberLong"] != null)
                    {
                        // Convert MongoDB $numberLong to a simple long value
                        newObj.Add(property.Name, long.Parse(property.Value["$numberLong"].ToString()));
                    }
                    else
                    {
                        // Recursively convert other properties
                        newObj.Add(property.Name, ConvertNode(property.Value));
                    }
                }
                return newObj;

            case JTokenType.Array:
                var newArray = new JArray();
                foreach (var item in node.Children())
                {
                    newArray.Add(ConvertNode(item));
                }
                return newArray;

            default:
                return node;  // Return the node as is for other types
        }
    }
}
