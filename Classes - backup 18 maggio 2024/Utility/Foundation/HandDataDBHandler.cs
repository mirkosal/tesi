using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Mono.Data.Sqlite;
using UnityEngine;

public class HandDataDBHandler : DatabaseObject<HandData, bool, List<HandData>, bool, bool>
{
    private string connectionString;
    public const string NomeTabella = "HandData";

    public HandDataDBHandler(string dbPath)
    {
        this.connectionString = "URI=file:" + dbPath;
    }

    public override bool Save(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is HandData))
        {
            throw new ArgumentException("HandData object is required");
        }

        HandData handData = (HandData)parameters[0];
        bool success = false;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = @"
            INSERT INTO HandData (DeviceID, Id, Timestamp, CurrentFramesPerSecond) 
            VALUES (@DeviceID, @Id, @Timestamp, @CurrentFramesPerSecond)";

                dbCmd.CommandText = sqlQuery;

                // Usando il metodo AddParameter per aggiungere i parametri
                AddParameter(dbCmd, "@DeviceID", handData.DeviceID);
                AddParameter(dbCmd, "@Id", handData.Id);
                AddParameter(dbCmd, "@Timestamp", handData.Timestamp);
                AddParameter(dbCmd, "@CurrentFramesPerSecond", handData.CurrentFramesPerSecond);

                try
                {
                    dbCmd.ExecuteNonQuery();
                    success = true;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Error saving HandData: " + e.Message);
                }
            }
        }

        return success;
    }

    private List<Hand> LoadHandsForHandData(int handDataID)
    {
        // Utilizza il DatabaseManager per ottenere un'istanza di HandDBHandler
        HandDBHandler handDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<HandDBHandler>("HandDBHandler");
        if (handDBHandler == null)
        {
            UnityEngine.Debug.LogError("Impossibile ottenere HandDBHandler dal DatabaseManager");
            return new List<Hand>(); // Ritorna una lista vuota per evitare null reference exceptions
        }

        // Chiama il metodo Search di HandDBHandler, passando "HandDataID" e il valore di handDataID come parametri.
        // Assumiamo che Search accetti una stringa per il nome della colonna e un oggetto per il valore di ricerca
        // e che restituisca una List<Hand> basata sui criteri di ricerca forniti.
        return handDBHandler.Search("HandDataID", handDataID.ToString());
    }


    public override HandData Load(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("HandDataID is required as an int parameter");
        }

        int handDataID = (int)parameters[0];
        HandData handData = null;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * FROM HandData WHERE HandDataID = @HandDataID";
                dbCmd.CommandText = sqlQuery;

                // Utilizzo di AddParameter per aggiungere il parametro della query
                AddParameter(dbCmd, "@HandDataID", handDataID);

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        handData = new HandData
                        {
                            // Supponendo che HandData abbia questi campi. Adattali secondo la tua implementazione.
                            DeviceID = Convert.ToInt32(reader["DeviceID"]),
                            Id = Convert.ToInt32(reader["Id"]),
                            Timestamp = Convert.ToInt64(reader["Timestamp"]),
                            CurrentFramesPerSecond = Convert.ToSingle(reader["CurrentFramesPerSecond"]),
                            // Hands verrà caricato dopo se necessario
                            Hands = new List<Hand>()
                        };
                        // Ora carica le Hands associate a questo HandData
                        handData.Hands = LoadHandsForHandData(handData.Id);
                    }
                }
            }
        }

        return handData;
    }

    public override List<HandData> Search(params object[] parameters)
    {
        if (parameters == null || parameters.Length < 2)
        {
            throw new ArgumentException("Search method requires at least two parameters: column name and search term.");
        }

        string columnName = parameters[0] as string;
        if (string.IsNullOrEmpty(columnName))
        {
            throw new ArgumentException("The column name must be a valid string.");
        }
        object searchTerm = parameters[1];
        List<HandData> foundHandDataList = new List<HandData>();

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = $"SELECT * FROM HandData WHERE {columnName} LIKE @SearchTerm";
                dbCmd.CommandText = sqlQuery;

                // Usando il metodo AddParameter per aggiungere il parametro di ricerca
                AddParameter(dbCmd, "@SearchTerm", "%" + searchTerm + "%");

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        HandData handData = new HandData
                        {
                            // Assumendo che HandData abbia questi campi; adatta i nomi dei campi e le conversioni come necessario
                            DeviceID = Convert.ToInt32(reader["DeviceID"]),
                            Id = Convert.ToInt32(reader["Id"]),
                            Timestamp = Convert.ToInt64(reader["Timestamp"]),
                            CurrentFramesPerSecond = Convert.ToSingle(reader["CurrentFramesPerSecond"]),
                            Hands = new List<Hand>() // Assumiamo che verranno caricate dopo, se necessario
                        };

                        // Qui potresti voler caricare le Hands associate usando un metodo separato
                        // Ad esempio:
                        handData.Hands = LoadHandsForHandData(handData.Id);

                        foundHandDataList.Add(handData);
                    }
                }
            }
        }

        return foundHandDataList;
    }


    public override bool Delete(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("HandDataID is required as an int parameter.");
        }

        int handDataID = (int)parameters[0];
        bool success = true;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            // Prima elimina tutte le Hands associate a questo HandData
            success = DeleteHandsForHandData(handDataID, dbConnection);

            if (!success)
            {
                UnityEngine.Debug.LogError("Failed to delete Hands for HandData with ID: " + handDataID);
                return false;
            }

            // Ora elimina il HandData stesso
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "DELETE FROM HandData WHERE HandDataID = @HandDataID";
                dbCmd.CommandText = sqlQuery;

                // Utilizzo di AddParameter per aggiungere il parametro @HandDataID
                AddParameter(dbCmd, "@HandDataID", handDataID);

                int rowsAffected = dbCmd.ExecuteNonQuery();
                success = rowsAffected > 0;
            }
        }

        return success;
    }

    private bool DeleteHandsForHandData(int handDataID, IDbConnection dbConnection)
    {
        // Assumi che esista un metodo o un handler per eliminare le Hands specifiche, per esempio in HandDBHandler
        HandDBHandler handDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<HandDBHandler>("HandDBHandler");
        if (handDBHandler == null)
        {
            UnityEngine.Debug.LogError("Unable to obtain HandDBHandler from DatabaseManager");
            return false;
        }

        // Ottieni tutte le Hands associate a questo HandData
        List<Hand> hands = handDBHandler.Search("HandDataID", handDataID.ToString());

        foreach (Hand hand in hands)
        {
            bool deleted = handDBHandler.Delete(hand.Id);
            if (!deleted)
            {
                UnityEngine.Debug.LogError("Failed to delete Hand with ID: " + hand.Id);
                return false; // Potresti decidere di continuare e tentare di eliminare le altre Hands
            }
        }

        return true;
    }


    public override bool Edit(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is HandData))
        {
            throw new ArgumentException("HandData object is required for editing.");
        }

        HandData handDataToUpdate = (HandData)parameters[0];
        bool success = false;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                dbCmd.CommandText = @"
            UPDATE HandData
            SET 
                DeviceID = @DeviceID, 
                Timestamp = @Timestamp, 
                CurrentFramesPerSecond = @CurrentFramesPerSecond
            WHERE HandDataID = @HandDataID";

                // Utilizzo di AddParameter per aggiungere i parametri al comando
                AddParameter(dbCmd, "@DeviceID", handDataToUpdate.DeviceID);
                AddParameter(dbCmd, "@Timestamp", handDataToUpdate.Timestamp);
                AddParameter(dbCmd, "@CurrentFramesPerSecond", handDataToUpdate.CurrentFramesPerSecond);
                AddParameter(dbCmd, "@HandDataID", handDataToUpdate.Id);

                int rowsAffected = dbCmd.ExecuteNonQuery();
                success = rowsAffected > 0;
            }
        }

        return success;
    }

}
