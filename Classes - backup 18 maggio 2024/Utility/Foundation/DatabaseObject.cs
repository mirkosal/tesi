using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;


public abstract class DatabaseObject<TLoad, TSave, TSearch, TDelete, TEdit>
{
    public int id { get; set; }
    protected string NomeTabella { get; set; }
    protected string connectionString { get; set; }

    public int getId()
    {
        UnityEngine.Debug.Log(connectionString);
        UnityEngine.Debug.Log(NomeTabella);
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                // Query per ottenere l'ID massimo dalla tabella
                string query = $"SELECT MAX(ID) as MaxID FROM {NomeTabella};";
                dbCmd.CommandText = query;

                object result = dbCmd.ExecuteScalar();  // Esegue la query e restituisce il primo valore della prima colonna del risultato
                if (result != DBNull.Value)  // Verifica se il risultato non è DBNull, che indica l'assenza di righe
                {
                    this.id = Convert.ToInt32(result);
                    return this.id;  // Ritorna l'ID massimo trovato
                }
            }
        }
        return 0;  // Ritorna 0 se non ci sono righe nella tabella o in caso di altri errori
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
