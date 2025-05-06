using Mono.Data.Sqlite;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

public class DatabaseManager
{
    private static DatabaseManager _instance;
    public static DatabaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DatabaseManager();
            }
            return _instance;
        }
    }

    private string connectionString;
    private Dictionary<string, Func<object>> factoryDictionary;

    private DatabaseManager()
    {
        string dbDirectory = Path.Combine(UnityEngine.Application.dataPath, "database");
        if (!Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }
        string dbPath = Path.Combine(dbDirectory, "sqlite_unity.db");
        // dbPath = dbDirectory.Replace("\\", "/");
     


        this.connectionString = "URI=file://" + dbPath;
        UnityEngine.Debug.Log("Database path: " + dbPath); // Stampa il percorso del database

        factoryDictionary = new Dictionary<string, Func<object>>
        {
            { "PersonDBHandler", () => new PersonDBHandler(connectionString) },
            { "LoginDBHandler", () => new LoginDBHandler(connectionString) },
            { "HandDBHandler", () => new HandDBHandler(connectionString) },
            // Aggiunte in base alla tua richiesta
            { "TaskGroupDBHandler", () => new TaskGroupDBHandler(connectionString) },
            { "ActivityDBHandler", () => new ActivityDBHandler(connectionString) },
            { "BoneDBHandler", () => new BoneDBHandler(connectionString) },
            { "FingerDBHandler", () => new FingerDBHandler(connectionString) },
            { "HandDataDBHandler", () => new HandDataDBHandler(connectionString) }
            // Aggiungi altri DBHandler qui seguendo lo stesso schema se necessario
        };
    }

    public T GetDatabaseObjectInstance<T>(string className) where T : class
    {
        if (factoryDictionary.TryGetValue(className, out Func<object> factory))
        {
            return factory() as T;
        }
        else
        {
            UnityEngine.Debug.LogError("Classe non trovata: " + className);
            return null;
        }
    }
}
