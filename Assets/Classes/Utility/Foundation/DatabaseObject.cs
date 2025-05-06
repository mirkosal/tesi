using System;
using System.Collections.Generic;
using System.Data;

using Mono.Data.Sqlite;
using UnityEngine;

public abstract class DatabaseObject<TLoad, TSave, TSearch, TDelete, TEdit>
{
    public int id { get; set; }
    protected string NomeTabella { get; set; }
    protected string connectionString { get; set; }

    public DatabaseObject(string connectionString, string nomeTabella)
    {
        this.connectionString = connectionString;
        this.NomeTabella = nomeTabella;
    }

    public int getId()
    {
        Debug.Log($"Connection String: {connectionString}");
        Debug.Log($"Table Name: {NomeTabella}");

        string idColumnName;

        // Determina il nome della colonna ID in base alla tabella
        switch (NomeTabella)
        {
            case "Hands":
                idColumnName = "HandID";
                break;
            case "HandData":
                idColumnName = "HandDataID";
                break;
            case "Fingers":
                idColumnName = "Id";
                break;
            case "Bones":
                idColumnName = "ID";
                break;
            case "Activities":
                idColumnName = "ID";
                break;
            case "TaskGroups":
                idColumnName = "ID";
                break;
            default:
                idColumnName = "ID";
                break;
        }

        Debug.Log($"ID Column Name: {idColumnName}");

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string query = $"SELECT MAX({idColumnName}) as MaxID FROM {NomeTabella};";
                Debug.Log($"Executing Query: {query}");
                dbCmd.CommandText = query;

                try
                {
                    object result = dbCmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        this.id = Convert.ToInt32(result);
                        Debug.Log($"Max ID: {this.id}");
                        return this.id;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error executing query: {ex.Message}");
                }
            }
        }
        return 0;
    }

    protected void AddParameter(IDbCommand command, string paramName, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = paramName;
        param.Value = value;
        command.Parameters.Add(param);
    }

    public abstract TLoad Load(params object[] parameters);
    public abstract TSave Save(params object[] parameters);
    public abstract TSearch Search(params object[] parameters);
    public abstract TDelete Delete(params object[] parameters);
    public abstract TEdit Edit(params object[] parameters);
}
