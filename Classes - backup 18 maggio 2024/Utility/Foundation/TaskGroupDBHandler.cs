using Mono.Data.Sqlite;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Data;

public class TaskGroupDBHandler : DatabaseObject<TaskGroup, bool, List<TaskGroup>, bool, bool>
{
    
    public TaskGroupDBHandler(string dbPath)
    {
        NomeTabella = "TaskGroups";
        this.connectionString = dbPath;
        UnityEngine.Debug.Log(dbPath);   
    }

    public override bool Save(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is TaskGroup))
        {
            throw new ArgumentException("TaskGroup object is required");
        }

        TaskGroup taskGroup = (TaskGroup)parameters[0];
        bool success = false;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = @"
        INSERT INTO TaskGroups (Name, PersonID) 
        VALUES (@Name, @PersonID);
        SELECT last_insert_rowid();";

                dbCmd.CommandText = sqlQuery;
                AddParameter(dbCmd, "@Name", taskGroup.Name);
                AddParameter(dbCmd, "@PersonID", taskGroup.PersonID);

                object result = dbCmd.ExecuteScalar();
                if (result != null && Convert.ToInt32(result) > 0)
                {
                    taskGroup.ID = Convert.ToInt32(result); // Aggiorna l'ID del TaskGroup con il valore generato dal database
                    success = true;
                }
            }
        }

        return success;
    }


    public override TaskGroup Load(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("TaskGroupID is required as an int parameter");
        }

        int taskGroupId = (int)parameters[0];
        TaskGroup taskGroup = null;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                dbCmd.CommandText = "SELECT * FROM TaskGroups WHERE ID = @TaskGroupID";
                AddParameter(dbCmd, "@TaskGroupID", taskGroupId);

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string name = reader["Name"].ToString();
                        int personId = reader.IsDBNull(reader.GetOrdinal("PersonID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PersonID"));
                        taskGroup = new TaskGroup(name, personId);
                        taskGroup.ID = taskGroupId;

                        // Carica le Activity associate a questo TaskGroup
                        taskGroup.Activities = LoadActivityForTaskGroup(taskGroupId);
                    }
                }
            }
        }

        return taskGroup;
    }


    private List<Activity> LoadActivityForTaskGroup(int taskGroupId)
    {
        List<Activity> activities = new List<Activity>();

        // Ottiene l'istanza di ActivityDBHandler dal DatabaseManager
        ActivityDBHandler activityDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<ActivityDBHandler>("ActivityDBHandler");
        if (activityDBHandler == null)
        {
            UnityEngine.Debug.LogError("Unable to obtain ActivityDBHandler from DatabaseManager");
            return activities;
        }

        // Utilizza il metodo Search di ActivityDBHandler per trovare le attività associate al TaskGroup
        // Assumendo che il metodo Search di ActivityDBHandler accetti un criterio di ricerca nel formato (nomeColonna, valore)
        // e restituisca una lista di Activity basate su quel criterio
        activities = activityDBHandler.Search("TaskGroupID", taskGroupId.ToString());

        return activities;
    }

    public override List<TaskGroup> Search(params object[] parameters)
    {
        if (parameters == null || parameters.Length < 2)
        {
            throw new ArgumentException("Search requires at least two parameters: column name and search term.");
        }

        string columnName = parameters[0] as string;
        string searchTerm = parameters[1] as string;
        bool loadActivities = parameters.Length > 2 && parameters[2] is bool && (bool)parameters[2];

        List<TaskGroup> taskGroups = new List<TaskGroup>();

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                dbCmd.CommandText = $"SELECT * FROM TaskGroups WHERE {columnName} LIKE @SearchTerm";
                AddParameter(dbCmd, "@SearchTerm", $"%{searchTerm}%");

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TaskGroup taskGroup = new TaskGroup(
                            reader["Name"].ToString(),
                            Convert.ToInt32(reader["PersonID"])
                        )
                        {
                            ID = Convert.ToInt32(reader["ID"])
                        };

                        // Condizionalmente carica le Activity associate a questo TaskGroup
                        if (loadActivities)
                        {
                            taskGroup.Activities = LoadActivityForTaskGroup(taskGroup.ID);
                        }

                        taskGroups.Add(taskGroup);
                    }
                }
            }
        }

        return taskGroups;
    }


    public override bool Delete(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("TaskGroupID is required as an int parameter.");
        }

        int taskGroupId = (int)parameters[0];
        bool success = true;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            // Prima cancella tutte le Activity associate a questo TaskGroup
            success = DeleteActivitiesForTaskGroup(taskGroupId, dbConnection);

            // Se la cancellazione delle Activity è riuscita, procedi con la cancellazione del TaskGroup
            if (success)
            {
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "DELETE FROM TaskGroups WHERE ID = @TaskGroupID";
                    AddParameter(dbCmd, "@TaskGroupID", taskGroupId);

                    int rowsAffected = dbCmd.ExecuteNonQuery();
                    success &= rowsAffected > 0;
                }
            }
        }

        return success;
    }

    private bool DeleteActivitiesForTaskGroup(int taskGroupId, IDbConnection dbConnection)
    {
        bool success = true;

        // Recupera gli ID di tutte le Activity associate a questo TaskGroup
        List<int> activityIds = new List<int>();
        using (IDbCommand dbCmd = dbConnection.CreateCommand())
        {
            dbCmd.CommandText = "SELECT ID FROM Activities WHERE TaskGroupID = @TaskGroupID";
            dbCmd.Parameters.Add(new SqliteParameter("@TaskGroupID", taskGroupId));

            using (IDataReader reader = dbCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    activityIds.Add(reader.GetInt32(0));
                }
            }
        }

        // Ottieni l'istanza di ActivityDBHandler per poter cancellare le Activity
        ActivityDBHandler activityDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<ActivityDBHandler>("ActivityDBHandler");
        if (activityDBHandler == null)
        {
            UnityEngine.Debug.LogError("Unable to obtain ActivityDBHandler from DatabaseManager");
            return false;
        }

        // Cancella ogni Activity utilizzando il suo ID
        foreach (int activityId in activityIds)
        {
            success &= activityDBHandler.Delete(new object[] { activityId });
            if (!success)
            {
                // Se la cancellazione di una qualsiasi Activity fallisce, interrompi il ciclo
                UnityEngine.Debug.LogError($"Failed to delete Activity with ID: {activityId}");
                break;
            }
        }

        return success;
    }

    public override bool Edit(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is TaskGroup))
        {
            throw new ArgumentException("TaskGroup object is required for editing.");
        }

        TaskGroup taskGroupToUpdate = (TaskGroup)parameters[0];
        bool success = false;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                dbCmd.CommandText = @"
        UPDATE TaskGroups
        SET
            Name = @Name,
            PersonID = @PersonID
        WHERE ID = @ID";

                AddParameter(dbCmd, "@Name", taskGroupToUpdate.Name);
                AddParameter(dbCmd, "@PersonID", taskGroupToUpdate.PersonID);
                AddParameter(dbCmd, "@ID", taskGroupToUpdate.ID);

                int rowsAffected = dbCmd.ExecuteNonQuery();
                success = rowsAffected > 0;
            }
        }

        return success;
    }

    public bool SaveAll(TaskGroup taskGroup)
    {
        bool success = true;

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Ottiene un'istanza di TaskGroupDBHandler tramite il DatabaseManager
                    var taskGroupHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<TaskGroupDBHandler>("TaskGroupDBHandler");
                    success &= taskGroupHandler.Save(taskGroup, transaction);

                    // Per ogni Activity nel TaskGroup
                    var activityHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<ActivityDBHandler>("ActivityDBHandler");
                    foreach (var activity in taskGroup.Activities)
                    {
                        success &= activityHandler.Save(activity, transaction);

                        // Per ogni HandData associata all'Activity
                        var handDataHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<HandDataDBHandler>("HandDataDBHandler");
                        foreach (var handData in activity.HandData) // Assumendo che ci sia una lista di HandDatas in Activity
                        {
                            success &= handDataHandler.Save(handData, transaction);

                            // Per ogni Hand in HandData
                            var handHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<HandDBHandler>("HandDBHandler");
                            foreach (var hand in handData.Hands) // Assumendo che ci sia una lista di Hands in HandData
                            {
                                success &= handHandler.Save(hand, transaction);

                                // Per ogni Finger in Hand
                                var fingerHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<FingerDBHandler>("FingerDBHandler");
                                foreach (var finger in hand.Fingers) // Assumendo che ci sia una lista di Fingers in Hand
                                {
                                    success &= fingerHandler.Save(finger, transaction);

                                    // Per ogni Bone in Finger
                                    var boneHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<BoneDBHandler>("BoneDBHandler");
                                    foreach (var bone in finger.bones) // Assumendo che ci sia una lista di Bones in Finger
                                    {
                                        success &= boneHandler.Save(bone, transaction);
                                    }
                                }
                            }
                        }
                    }

                    // Commit se tutto è andato a buon fine
                    if (success)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    success = false;
                }
            }
        }

        return success;
    }

}
