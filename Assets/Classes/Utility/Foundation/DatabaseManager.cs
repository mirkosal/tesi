using Mono.Data.Sqlite;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Diagnostics;

public class DatabaseManager
{
    private static DatabaseManager _instance;
    private static readonly object _lock = new object();
    public static DatabaseManager Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new DatabaseManager();
                }
                return _instance;
            }
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
        this.connectionString = "URI=file://" + dbPath;
        UnityEngine.Debug.Log("Database path: " + dbPath); // Stampa il percorso del database

        factoryDictionary = new Dictionary<string, Func<object>>
        {
            { "PersonDBHandler", () => PersonDBHandler.Instance(connectionString) },
            { "LoginDBHandler", () => LoginDBHandler.Instance(connectionString) },
            { "HandDBHandler", () => HandDBHandler.Instance(connectionString) },
            { "TaskGroupDBHandler", () => TaskGroupDBHandler.Instance(connectionString) },
            { "ActivityDBHandler", () => ActivityDBHandler.Instance(connectionString) },
            { "BoneDBHandler", () => BoneDBHandler.Instance(connectionString) },
            { "FingerDBHandler", () => FingerDBHandler.Instance(connectionString) },
            { "HandDataDBHandler", () => HandDataDBHandler.Instance(connectionString) }
            // Aggiungi altri DBHandler qui seguendo lo stesso schema se necessario
        };
    }

    public T GetDatabaseObjectInstance<T>(string className) where T : class
    {
        lock (_lock)
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
}
