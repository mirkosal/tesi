using Mono.Data.Sqlite;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Diagnostics;

public class ActivityDBHandler : DatabaseObject<Activity, bool, List<Activity>, bool, bool>
{
    private static ActivityDBHandler _instance;
    private static readonly object _lock = new object();
    public const string NomeTabella = "Activities";
    private ActivityDBHandler(string DB) : base(DB, NomeTabella)
    {
      
    }

    public static ActivityDBHandler Instance(string dbPath)
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new ActivityDBHandler(dbPath);
                }
            }
        }
        return _instance;
    }

    public override bool Save(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is Activity))
        {
            throw new ArgumentException("Activity object is required");
        }

        Activity activity = (Activity)parameters[0];
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
            INSERT INTO Activities (TaskGroupID, Repetitions) 
            VALUES (@TaskGroupID, @Repetitions);
            SELECT last_insert_rowid();";

                dbCmd.CommandText = sqlQuery;

                AddParameter(dbCmd, "@TaskGroupID", activity.TaskGroupID);
                AddParameter(dbCmd, "@Repetitions", activity.Repetitions);

                object result = dbCmd.ExecuteScalar();
                success = result != null && Convert.ToInt32(result) > 0;

                if (success)
                {
                    activity.Id = Convert.ToInt32(result);
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Error saving Activity: " + e.Message);
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


    public override Activity Load(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("ActivityID is required as an int parameter");
        }

        int activityID = (int)parameters[0];
        Activity activity = null;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            // Carica l'Activity specificata dall'ID
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                dbCmd.CommandText = "SELECT * FROM Activities WHERE ID = @ActivityID";
                AddParameter(dbCmd, "@ActivityID", activityID);

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Assumi che ci sia un costruttore di Activity che accetti Repetitions e un elenco vuoto di HandData
                        activity = new Activity(
                            reader.GetInt32(reader.GetOrdinal("ID")),
                            reader.GetInt32(reader.GetOrdinal("TaskGroupID")),
                            reader.GetInt32(reader.GetOrdinal("Repetitions")),
                            new List<HandData>() // Inizializza un elenco vuoto che sarà popolato nel passo successivo
                        );
                    }
                }
            }

            // Carica gli HandData associati a questa Activity

        }

        return activity;
    }

        public override List<Activity> Search(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                throw new ArgumentException("Search method requires at least two parameters: column name and search term.");
            }

            string columnName = parameters[0] as string;
            string searchTerm = parameters[1] as string;
            IDbTransaction transaction = parameters.Length > 2 && parameters[2] is IDbTransaction ? (IDbTransaction)parameters[2] : null;

            List<Activity> activities = new List<Activity>();

            using (IDbCommand dbCmd = transaction != null ? transaction.Connection.CreateCommand() : new SqliteConnection(connectionString).CreateCommand())
            {
                if (transaction != null)
                {
                    dbCmd.Transaction = transaction;
                }
                dbCmd.CommandText = $"SELECT * FROM Activities WHERE {columnName} LIKE @SearchTerm";
                AddParameter(dbCmd, "@SearchTerm", $"%{searchTerm}%");

                if (transaction == null)
                {
                    dbCmd.Connection.Open();
                }

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int activityID = reader.GetInt32(reader.GetOrdinal("ID"));
                        Activity activity = new Activity(
                            reader.GetInt32(reader.GetOrdinal("ID")),
                            reader.GetInt32(reader.GetOrdinal("TaskGroupID")),
                            reader.GetInt32(reader.GetOrdinal("Repetitions")),
                            new List<HandData>() // Lista vuota che verrà popolata di seguito
                        );

                        // Carica gli HandData associati a questa Activity
          

                        activities.Add(activity);
                    }
                }

                if (transaction == null)
                {
                    dbCmd.Connection.Close();
                }
            }

            return activities;
        }




   
    


    public override bool Delete(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("ActivityID is required as an int parameter.");
        }

        int activityID = (int)parameters[0];
        bool success = true;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            // Elimina tutti gli HandData associati
            success = DeleteHandDataForActivity(activityID, dbConnection);

            // Procedi con l'eliminazione dell'Activity stessa solo se tutti gli HandData sono stati eliminati con successo
            if (success)
            {
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "DELETE FROM Activities WHERE ID = @ActivityID";
                    AddParameter(dbCmd, "@ActivityID", activityID);

                    int rowsAffected = dbCmd.ExecuteNonQuery();
                    success &= rowsAffected > 0;
                }
            }
        }

        return success;
    }

    private bool DeleteHandDataForActivity(int activityID, IDbConnection dbConnection)
    {
        // Ottieni l'istanza di HandDataDBHandler dal DatabaseManager
        HandDataDBHandler handDataDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<HandDataDBHandler>("HandDataDBHandler");
        if (handDataDBHandler == null)
        {
            UnityEngine.Debug.LogError("Unable to obtain HandDataDBHandler from DatabaseManager");
            return false;
        }

        // Recupera gli ID di tutti gli HandData associati all'Activity
        List<int> handDataIds = new List<int>();
        using (IDbCommand dbCmd = dbConnection.CreateCommand())
        {
            dbCmd.CommandText = "SELECT ID FROM HandData WHERE ActivityID = @ActivityID";
            dbCmd.Parameters.Add(new SqliteParameter("@ActivityID", activityID));

            using (IDataReader reader = dbCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    handDataIds.Add(reader.GetInt32(0));
                }
            }
        }

        // Elimina ciascun HandData associato all'Activity
        bool success = true;
        foreach (int handDataId in handDataIds)
        {
            success &= handDataDBHandler.Delete(new object[] { handDataId });
            if (!success)
            {
                UnityEngine.Debug.LogError($"Failed to delete HandData with ID: {handDataId}");
                break;
            }
        }

        return success;
    }

    public override bool Edit(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is Activity))
        {
            throw new ArgumentException("Activity object is required for editing.");
        }

        Activity activityToUpdate = (Activity)parameters[0];
        bool success = false;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                dbCmd.CommandText = @"
            UPDATE Activities
            SET
                Repetitions = @Repetitions,
                TaskGroupID = @TaskGroupID
            WHERE ID = @ID";

                AddParameter(dbCmd, "@Repetitions", activityToUpdate.Repetitions);
                AddParameter(dbCmd, "@TaskGroupID", activityToUpdate.TaskGroupID);
                AddParameter(dbCmd, "@ID", activityToUpdate.Id);

                int rowsAffected = dbCmd.ExecuteNonQuery();
                success = rowsAffected > 0;
            }
        }

        return success;
    }
}
