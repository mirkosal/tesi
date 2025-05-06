using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

public class ReadDatabase : MonoBehaviour
{
    private string connectionString;

    void Start()
    {
        // Costruisci il percorso del database
        string dbDirectory = Path.Combine(Application.dataPath, "database");
        string dbPath = Path.Combine(dbDirectory, "database.db");

        connectionString = "URI=file:" + dbPath;

        // Leggere i dati dal database
        ReadData();
    }

    private void ReadData()
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                Debug.Log("ccc");
                string sqlQuery = "SELECT * FROM users";
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string username = reader.GetString(1);
                        string password = reader.GetString(2); // Nota: in una applicazione reale, non è consigliabile leggere o stampare le password in chiaro

                        // Stampa il nome utente e la password nel log della console di Unity
                        Debug.Log($"ID: {id}, Username: {username}, Password: {password}");
                    }

                    reader.Close();
                }
            }

            dbConnection.Close();
        }
    }

}
