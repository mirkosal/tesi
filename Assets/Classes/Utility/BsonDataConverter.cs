using MongoDB.Bson;
using System;
using MongoDB.Bson.Serialization;

public static class BsonDataConverter
{
    // Serializza un oggetto in una stringa BSON
    public static string SerializeToBson<T>(T data)
    {
        var bsonDocument = data.ToBsonDocument();
        return bsonDocument.ToJson();
    }

    public static T DeserializeFromBson<T>(string bson)
    {
        try
        {
            // Prova a deserializzare direttamente nel tipo target T
            return BsonSerializer.Deserialize<T>(bson);
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
        {
            // In caso di eccezione, verifica se il BSON rappresenta un array e tenta la deserializzazione come tale
            var array = BsonSerializer.Deserialize<BsonArray>(bson);
            return BsonSerializer.Deserialize<T>(array.ToJson());
        }
    }



}
