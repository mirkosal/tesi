using System.IO;
using UnityEngine;
using System.Collections.Generic;

public static class JsonDataExtractor
{
    public static HandData LoadHandData(string filePath)
    {
        try
        {
            string jsonData = File.ReadAllText(filePath);
            return JsonUtility.FromJson<HandData>(jsonData);
        }
        catch (IOException e)
        {
            Debug.LogError("Errore nel caricamento del file: " + e.Message);
            return null;
        }
    }
}
