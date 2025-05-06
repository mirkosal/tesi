using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Mono.Data.Sqlite;
using UnityEngine;

public class HandDataDBHandler : DatabaseObject<HandData, bool, List<HandData>, bool, bool>
{
    private static HandDataDBHandler _instance;
    private static readonly object _lock = new object();

    public const string NomeTabella = "HandData";

    private HandDataDBHandler(string DB) : base(DB, NomeTabella)
    { 
    }

    public static HandDataDBHandler Instance(string dbPath)
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new HandDataDBHandler(dbPath);
                }
            }
        }
        return _instance;
    }

    public override bool Save(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is HandData))
        {
            throw new ArgumentException("HandData object is required");
        }

        HandData handData = (HandData)parameters[0];
        IDbConnection dbConnection = parameters.Length > 1 && parameters[1] is IDbConnection ? (IDbConnection)parameters[1] : null;
        IDbTransaction transaction = parameters.Length > 2 && parameters[2] is IDbTransaction ? (IDbTransaction)parameters[2] : null;

        bool localConnection = dbConnection == null;
        bool success = false;

        if (localConnection)
        {
            dbConnection = new SqliteConnection(connectionString);
            dbConnection.Open();
        }

        try
        {
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                if (transaction != null)
                {
                    dbCmd.Transaction = transaction;
                }

                string sqlQuery = @"
                INSERT INTO HandData (DeviceID, Id, Timestamp, CurrentFramesPerSecond, ActivityID) 
                VALUES (@DeviceID, @Id, @Timestamp, @CurrentFramesPerSecond, @ActivityID)";
                dbCmd.CommandText = sqlQuery;

                // Usando il metodo AddParameter per aggiungere i parametri
                AddParameter(dbCmd, "@DeviceID", handData.DeviceID);
                AddParameter(dbCmd, "@Id", handData.Id);
                AddParameter(dbCmd, "@Timestamp", handData.Timestamp);
                AddParameter(dbCmd, "@CurrentFramesPerSecond", handData.CurrentFramesPerSecond);
                AddParameter(dbCmd, "@ActivityID", handData.ActivityID);

                dbCmd.ExecuteNonQuery();
                success = true;
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Error saving HandData: " + e.Message);
            success = false;
        }
        finally
        {
            if (localConnection)
            {
                dbConnection.Close();
            }
        }

        return success;
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
                string sqlQuery = "SELECT * FROM HandData WHERE Id = @HandDataID";
                dbCmd.CommandText = sqlQuery;

                // Utilizzo di AddParameter per aggiungere il parametro della query
                AddParameter(dbCmd, "@HandDataID", handDataID);

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        handData = new HandData
                        {
                            DeviceID = Convert.ToInt32(reader["DeviceID"]),
                            Id = Convert.ToInt32(reader["Id"]),
                            Timestamp = Convert.ToInt64(reader["Timestamp"]),
                            CurrentFramesPerSecond = Convert.ToSingle(reader["CurrentFramesPerSecond"]),
                            ActivityID = Convert.ToInt32(reader["ActivityID"]),
                            Hands = new List<Hand>()
                        };
                        // Ora carica le Hands associate a questo HandData
          
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
            string searchTerm = parameters[1] as string;
            IDbTransaction transaction = parameters.Length > 2 && parameters[2] is IDbTransaction ? (IDbTransaction)parameters[2] : null;

            List<HandData> handDataList = new List<HandData>();

            using (IDbCommand dbCmd = transaction != null ? transaction.Connection.CreateCommand() : new SqliteConnection(connectionString).CreateCommand())
            {
                if (transaction != null)
                {
                    dbCmd.Transaction = transaction;
                }
                dbCmd.CommandText = $"SELECT * FROM HandData WHERE {columnName} LIKE @SearchTerm";
                AddParameter(dbCmd, "@SearchTerm", $"%{searchTerm}%");

                if (transaction == null)
                {
                    dbCmd.Connection.Open();
                }

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        HandData handData = new HandData
                        {
                            DeviceID = Convert.ToInt32(reader["DeviceID"]),
                            Id = Convert.ToInt32(reader["Id"]),
                            Timestamp = Convert.ToInt64(reader["Timestamp"]),
                            CurrentFramesPerSecond = Convert.ToSingle(reader["CurrentFramesPerSecond"]),
                            ActivityID = Convert.ToInt32(reader["ActivityID"]),
                            Hands = new List<Hand>()
                        };


                        handDataList.Add(handData);
                    }
                }

                if (transaction == null)
                {
                    dbCmd.Connection.Close();
                }
            }

            return handDataList;
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
