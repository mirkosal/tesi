using Mono.Data.Sqlite;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;
public class TaskGroupDBHandler : DatabaseObject<TaskGroup, bool, List<TaskGroup>, bool, bool>
{
    private static TaskGroupDBHandler _instance;
    private static readonly object _lock = new object();
    public const string NomeTabella = "TaskGroups";
    private TaskGroupDBHandler(string DB) : base(DB, NomeTabella)
    {
    }
    public static TaskGroupDBHandler Instance(string dbPath)
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new TaskGroupDBHandler(dbPath);
                }
            }
        }
        return _instance;
    }
    public override bool Save(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is TaskGroup))
        {
            throw new ArgumentException("TaskGroup object is required");
        }
        TaskGroup taskGroup = (TaskGroup)parameters[0];
        SqliteConnection sqliteConnection = parameters.Length > 1 && parameters[1] is SqliteConnection ? (SqliteConnection)parameters[1] : null;
        IDbTransaction transaction = parameters.Length > 2 && parameters[2] is IDbTransaction ? (IDbTransaction)parameters[2] : null;
        bool localConnection = sqliteConnection == null;
        bool success = false;
        if (localConnection)
        {
            sqliteConnection = new SqliteConnection(connectionString);
            sqliteConnection.Open();
        }
        try
        {
            using (IDbCommand dbCmd = sqliteConnection.CreateCommand())
            {
                if (transaction != null)
                {
                    dbCmd.Transaction = transaction;
                }
                string sqlQuery = @"
                INSERT INTO TaskGroups 
                (Name, PersonID) 
                VALUES 
                (@Name, @PersonID)";
                dbCmd.CommandText = sqlQuery;
                AddParameter(dbCmd, "@Name", taskGroup.Name);
                AddParameter(dbCmd, "@PersonID", taskGroup.PersonID);
                dbCmd.ExecuteNonQuery();
                dbCmd.CommandText = "SELECT last_insert_rowid()";
                taskGroup.ID = Convert.ToInt32(dbCmd.ExecuteScalar());
                success = true;
            }
            foreach (var activity in taskGroup.Activities)
            {
                if (activity == null)
                {
                    throw new ArgumentException("Activity cannot be null.");
                }
                activity.TaskGroupID = taskGroup.ID;
                success &= SaveActivity(activity, sqliteConnection, transaction);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("Error saving TaskGroup: " + e.Message);
            success = false;
        }
        finally
        {
            if (localConnection)
            {
                sqliteConnection.Close();
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
                        taskGroup.Activities = LoadActivityForTaskGroup(taskGroupId);
                    }
                }
            }
        }

        return taskGroup;
    }
    public IEnumerator LoadAllCoroutine(int taskGroupId, Action<TaskGroup> onComplete)
    {
        Debug.Log($"Caricamento TaskGroup con ID: {taskGroupId}");
        TaskGroup taskGroup = null;
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                taskGroup = Load(taskGroupId, transaction);
                if (taskGroup != null)
                {
                    yield return CoroutineRunner.Instance.StartManagedCoroutine(LoadAllActivities(taskGroup, transaction));
                    Debug.Log($"TaskGroup {taskGroup.Name} caricato con {taskGroup.Activities.Count} attivit�");
                    transaction.Commit();
                }
                else
                {
                    Debug.LogError("TaskGroup non trovato");
                }
            }
        }

        onComplete?.Invoke(taskGroup);
    }
    private IEnumerator LoadAllActivities(TaskGroup taskGroup, IDbTransaction transaction)
    {
        ActivityDBHandler activityDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<ActivityDBHandler>("ActivityDBHandler");

        if (activityDBHandler != null)
        {
            taskGroup.Activities = activityDBHandler.Search("TaskGroupID", taskGroup.ID.ToString(), transaction);
            foreach (var activity in taskGroup.Activities)
            {
                yield return CoroutineRunner.Instance.StartManagedCoroutine(LoadAllHandData(activity, transaction));
                Debug.Log($"Attivit� {activity.Id} caricata con {activity.HandData.Count} HandData");
            }
        }
        else
        {
            Debug.LogError("ActivityDBHandler non trovato");
        }
        yield return null;
    }
    private IEnumerator LoadAllHandData(Activity activity, IDbTransaction transaction)
    {
        HandDataDBHandler handDataDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<HandDataDBHandler>("HandDataDBHandler");
        if (handDataDBHandler != null)
        {
            activity.HandData = handDataDBHandler.Search("ActivityID", activity.Id.ToString(), transaction);
            foreach (var handData in activity.HandData)
            {
                yield return CoroutineRunner.Instance.StartManagedCoroutine(LoadAllHands(handData, transaction));
                Debug.Log($"HandData {handData.Id} caricata con {handData.Hands.Count} mani");
            }
        }
        else
        {
            Debug.LogError("HandDataDBHandler non trovato");
        }

        yield return null;
    }
    private IEnumerator LoadAllHands(HandData handData, IDbTransaction transaction)
    {
        HandDBHandler handDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<HandDBHandler>("HandDBHandler");
        if (handDBHandler != null)
        {
            handData.Hands = handDBHandler.Search("HandDataID", handData.Id.ToString(), transaction);
            foreach (var hand in handData.Hands)
            {
                yield return CoroutineRunner.Instance.StartManagedCoroutine(LoadAllFingers(hand, transaction));
                Debug.Log($"Hand {hand.Id} caricata con {hand.Fingers.Count} dita");
            }
        }
        else
        {
            Debug.LogError("HandDBHandler non trovato");
        }
        yield return null;
    }
    private IEnumerator LoadAllFingers(Hand hand, IDbTransaction transaction)
    {
        FingerDBHandler fingerDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<FingerDBHandler>("FingerDBHandler");

        if (fingerDBHandler != null)
        {
            hand.Fingers = fingerDBHandler.Search("HandID", hand.Id.ToString(), transaction);
            foreach (var finger in hand.Fingers)
            {
                finger.bones = LoadAllBones(finger.Id, transaction);
                Debug.Log($"Finger {finger.Id} caricata con {finger.bones.Count} ossa");
            }
        }
        else
        {
            Debug.LogError("FingerDBHandler non trovato");
        }

        yield return null;
    }
    private List<Bone> LoadAllBones(int fingerId, IDbTransaction transaction)
    {
        List<Bone> bones = new List<Bone>();
        BoneDBHandler boneDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<BoneDBHandler>("BoneDBHandler");

        if (boneDBHandler != null)
        {
            bones = boneDBHandler.Search("FingerID", fingerId.ToString(), transaction);
        }
        else
        {
            Debug.LogError("BoneDBHandler non trovato");
        }
        return bones;
    }
    private List<Activity> LoadActivityForTaskGroup(int taskGroupId)
    {
        List<Activity> activities = new List<Activity>();
        ActivityDBHandler activityDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<ActivityDBHandler>("ActivityDBHandler");
        if (activityDBHandler == null)
        {
            UnityEngine.Debug.LogError("Unable to obtain ActivityDBHandler from DatabaseManager");
            return activities;
        }
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
            success = DeleteActivitiesForTaskGroup(taskGroupId, dbConnection);
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
        ActivityDBHandler activityDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<ActivityDBHandler>("ActivityDBHandler");
        if (activityDBHandler == null)
        {
            UnityEngine.Debug.LogError("Unable to obtain ActivityDBHandler from DatabaseManager");
            return false;
        }
        foreach (int activityId in activityIds)
        {
            success &= activityDBHandler.Delete(new object[] { activityId });
            if (!success)
            {
                UnityEngine.Debug.LogError($"Failed to delete Activity with ID: {activityId}");
                break;
            }
        }
        return success;
    }
    public override bool Edit(params object[] parameters)
    {
        if (parameters == null || parameters.Length < 2 || !(parameters[0] is int) || !(parameters[1] is TaskGroup))
        {
            throw new ArgumentException("ID and TaskGroup object are required");
        }
        int id = (int)parameters[0];
        TaskGroup taskGroupToUpdate = (TaskGroup)parameters[1];
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
                AddParameter(dbCmd, "@ID", id);
                int rowsAffected = dbCmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }
    public bool SaveAll(TaskGroup taskGroup)
    {
        bool success = true;
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    success &= SaveTaskGroup(taskGroup, dbConnection, transaction);
                    if (!success)
                    {
                        UnityEngine.Debug.LogError("Failed to save TaskGroup.");
                        transaction.Rollback();
                        return false;
                    }
                    UnityEngine.Debug.Log("TaskGroup salvato con successo.");
                    foreach (var activity in taskGroup.Activities)
                    {
                        if (activity == null)
                        {
                            UnityEngine.Debug.LogError("Activity is null.");
                            transaction.Rollback();
                            return false;
                        }
                        UnityEngine.Debug.Log($"Saving Activity: {activity.Id}");
                        success &= SaveActivity(activity, dbConnection, transaction);
                        if (!success)
                        {
                            UnityEngine.Debug.LogError("Failed to save Activity.");
                            transaction.Rollback();
                            return false;
                        }
                        UnityEngine.Debug.Log($"Activity salvata con successo. ID: {activity.Id}");
                        foreach (var handData in activity.HandData)
                        {
                            if (handData == null)
                            {
                                UnityEngine.Debug.LogError("HandData is null.");
                                transaction.Rollback();
                                return false;
                            }
                            UnityEngine.Debug.Log($"Saving HandData: {handData.Id}");
                            success &= SaveHandData(handData, dbConnection, transaction);
                            if (!success)
                            {
                                UnityEngine.Debug.LogError("Failed to save HandData.");
                                transaction.Rollback();
                                return false;
                            }
                            UnityEngine.Debug.Log($"HandData salvata con successo. ID: {handData.Id}");
                            foreach (var hand in handData.Hands)
                            {
                                if (hand == null)
                                {
                                    UnityEngine.Debug.LogError("Hand is null.");
                                    transaction.Rollback();
                                    return false;
                                }
                                UnityEngine.Debug.Log($"Saving Hand: {hand.Id}");
                                success &= SaveHand(hand, dbConnection, transaction);
                                if (!success)
                                {
                                    UnityEngine.Debug.LogError("Failed to save Hand.");
                                    transaction.Rollback();
                                    return false;
                                }
                                UnityEngine.Debug.Log($"Hand salvata con successo. ID: {hand.Id}");
                                foreach (var finger in hand.Fingers)
                                {
                                    if (finger == null)
                                    {
                                        UnityEngine.Debug.LogError("Finger is null.");
                                        transaction.Rollback();
                                        return false;
                                    }
                                    UnityEngine.Debug.Log($"Saving Finger: {finger.Id}");
                                    success &= SaveFinger(finger, dbConnection, transaction);
                                    if (!success)
                                    {
                                        UnityEngine.Debug.LogError("Failed to save Finger.");
                                        transaction.Rollback();
                                        return false;
                                    }
                                    UnityEngine.Debug.Log($"Finger salvata con successo. ID: {finger.Id}");
                                    foreach (var bone in finger.bones)
                                    {
                                        if (bone == null)
                                        {
                                            UnityEngine.Debug.LogError("Bone is null.");
                                            transaction.Rollback();
                                            return false;
                                        }
                                        UnityEngine.Debug.Log($"Saving Bone: ");
                                        success &= SaveBone(bone, dbConnection, transaction);
                                        if (!success)
                                        {
                                            UnityEngine.Debug.LogError("Failed to save Bone.");
                                            transaction.Rollback();
                                            return false;
                                        }
                                        UnityEngine.Debug.Log($"Bone salvata con successo. ID: {bone.BoneID}");
                                    }
                                }
                            }
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Error during SaveAll: " + e.Message);
                    transaction.Rollback();
                    success = false;
                }
            }
        }
        return success;
    }
    private bool SaveTaskGroup(TaskGroup taskGroup, IDbConnection dbConnection, IDbTransaction transaction)
    {
        var taskGroupHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<TaskGroupDBHandler>("TaskGroupDBHandler");
        if (taskGroupHandler != null)
        {
            return taskGroupHandler.Save(taskGroup, dbConnection, transaction);
        }
        else
        {
            UnityEngine.Debug.LogError("Unable to obtain TaskGroupDBHandler from DatabaseManager");
            return false;
        }
    }
    private bool SaveActivity(Activity activity, IDbConnection dbConnection, IDbTransaction transaction)
    {
        var activityHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<ActivityDBHandler>("ActivityDBHandler");
        if (activityHandler != null)
        {
            return activityHandler.Save(activity, dbConnection, transaction);
        }
        else
        {
            UnityEngine.Debug.LogError("Unable to obtain ActivityDBHandler from DatabaseManager");
            return false;
        }
    }
        private bool SaveHandData(HandData handData, IDbConnection dbConnection, IDbTransaction transaction)
        {
            var handDataHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<HandDataDBHandler>("HandDataDBHandler");
            if (handDataHandler != null)
            {
                return handDataHandler.Save(handData, dbConnection, transaction);
            }
            else
            {
                UnityEngine.Debug.LogError("Unable to obtain HandDataDBHandler from DatabaseManager");
                return false;
            }
        }

        private bool SaveHand(Hand hand, IDbConnection dbConnection, IDbTransaction transaction)
        {
            var handHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<HandDBHandler>("HandDBHandler");
            if (handHandler != null)
            {
                return handHandler.Save(hand, dbConnection, transaction);
            }
            else
            {
                UnityEngine.Debug.LogError("Unable to obtain HandDBHandler from DatabaseManager");
                return false;
            }
        }
        private bool SaveFinger(Finger finger, IDbConnection dbConnection, IDbTransaction transaction)
        {
            var fingerHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<FingerDBHandler>("FingerDBHandler");
            if (fingerHandler != null)
            {
                return fingerHandler.Save(finger, dbConnection, transaction);
            }
            else
            {
                UnityEngine.Debug.LogError("Unable to obtain FingerDBHandler from DatabaseManager");
                return false;
            }
        }
        private bool SaveBone(Bone bone, IDbConnection dbConnection, IDbTransaction transaction)
        {
            var boneHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<BoneDBHandler>("BoneDBHandler");
            UnityEngine.Debug.Log(bone);
            if (boneHandler != null)
            {
                UnityEngine.Debug.Log(bone);
                return boneHandler.Save(bone, dbConnection, transaction);
            }
            else
            {
                UnityEngine.Debug.Log("Unable to obtain BoneDBHandler from DatabaseManager");
                return false;
            }
        }
        public IEnumerator SaveAllCoroutine(TaskGroup taskGroup, Action<bool> callback)
        {
            bool success = true;
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    bool errorOccurred = false;
                    yield return CoroutineRunner.Instance.StartManagedCoroutine(ExecuteSaveOperation(() =>
                    {
                        try
                        {
                            success &= SaveTaskGroup(taskGroup, dbConnection, transaction);
                            if (!success)
                            {
                                Debug.LogError("Failed to save TaskGroup.");
                                transaction.Rollback();
                                errorOccurred = true;
                            }
                            Debug.Log("TaskGroup salvato con successo.");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Exception while saving TaskGroup: {ex.Message}");
                            transaction.Rollback();
                            errorOccurred = true;
                        }
                    }));
                    if (errorOccurred) { callback(false); yield break; }
                    foreach (var activity in taskGroup.Activities)
                    {
                        yield return CoroutineRunner.Instance.StartManagedCoroutine(ExecuteSaveOperation(() =>
                        {
                            try
                            {
                                if (activity == null)
                                {
                                    Debug.LogError("Activity is null.");
                                    transaction.Rollback();
                                    errorOccurred = true;
                                }
                                Debug.Log($"Saving Activity: {activity.Id}");
                                success &= SaveActivity(activity, dbConnection, transaction);
                                if (!success)
                                {
                                    Debug.LogError("Failed to save Activity.");
                                    transaction.Rollback();
                                    errorOccurred = true;
                                }
                                Debug.Log($"Activity salvata con successo. ID: {activity.Id}");
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"Exception while saving Activity: {ex.Message}");
                                transaction.Rollback();
                                errorOccurred = true;
                            }
                        }));
                        if (errorOccurred) { callback(false); yield break; }
                        foreach (var handData in activity.HandData)
                        {
                            yield return CoroutineRunner.Instance.StartManagedCoroutine(ExecuteSaveOperation(() =>
                            {
                                try
                                {
                                    if (handData == null)
                                    {
                                        Debug.LogError("HandData is null.");
                                        transaction.Rollback();
                                        errorOccurred = true;
                                    }

                                    Debug.Log($"Saving HandData: {handData.Id}");
                                    success &= SaveHandData(handData, dbConnection, transaction);
                                    if (!success)
                                    {
                                        Debug.LogError("Failed to save HandData.");
                                        transaction.Rollback();
                                        errorOccurred = true;
                                    }
                                    Debug.Log($"HandData salvata con successo. ID: {handData.Id}");
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Exception while saving HandData: {ex.Message}");
                                    transaction.Rollback();
                                    errorOccurred = true;
                                }
                            }));
                            if (errorOccurred) { callback(false); yield break; }
                            foreach (var hand in handData.Hands)
                            {
                                yield return CoroutineRunner.Instance.StartManagedCoroutine(ExecuteSaveOperation(() =>
                                {
                                    try
                                    {
                                        if (hand == null)
                                        {
                                            Debug.LogError("Hand is null.");
                                            transaction.Rollback();
                                            errorOccurred = true;
                                        }

                                        Debug.Log($"Saving Hand: {hand.Id}");
                                        success &= SaveHand(hand, dbConnection, transaction);
                                        if (!success)
                                        {
                                            Debug.LogError("Failed to save Hand.");
                                            transaction.Rollback();
                                            errorOccurred = true;
                                        }
                                        Debug.Log($"Hand salvata con successo. ID: {hand.Id}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogError($"Exception while saving Hand: {ex.Message}");
                                        transaction.Rollback();
                                        errorOccurred = true;
                                    }
                                }));
                                if (errorOccurred) { callback(false); yield break; }
                                foreach (var finger in hand.Fingers)
                                {
                                    yield return CoroutineRunner.Instance.StartManagedCoroutine(ExecuteSaveOperation(() =>
                                    {
                                        try
                                        {
                                            if (finger == null)
                                            {
                                                Debug.LogError("Finger is null.");
                                                transaction.Rollback();
                                                errorOccurred = true;
                                            }
                                            Debug.Log($"Saving Finger: {finger.Id}");
                                            success &= SaveFinger(finger, dbConnection, transaction);
                                            if (!success)
                                            {
                                                Debug.LogError("Failed to save Finger.");
                                                transaction.Rollback();
                                                errorOccurred = true;
                                            }
                                            Debug.Log($"Finger salvata con successo. ID: {finger.Id}");
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.LogError($"Exception while saving Finger: {ex.Message}");
                                            transaction.Rollback();
                                            errorOccurred = true;
                                        }
                                    }));
                                    if (errorOccurred) { callback(false); yield break; }
                                    foreach (var bone in finger.bones)
                                    {
                                        yield return CoroutineRunner.Instance.StartManagedCoroutine(ExecuteSaveOperation(() =>
                                        {
                                            try
                                            {
                                                if (bone == null)
                                                {
                                                    Debug.LogError("Bone is null.");
                                                    transaction.Rollback();
                                                    errorOccurred = true;
                                                }

                                                Debug.Log($"Saving Bone: {bone.BoneID}");
                                                success &= SaveBone(bone, dbConnection, transaction);
                                                if (!success)
                                                {
                                                    Debug.LogError("Failed to save Bone.");
                                                    transaction.Rollback();
                                                    errorOccurred = true;
                                                }
                                                Debug.Log($"Bone salvata con successo. ID: {bone.BoneID}");
                                            }
                                            catch (Exception ex)
                                            {
                                                Debug.LogError($"Exception while saving Bone: {ex.Message}");
                                                transaction.Rollback();
                                                errorOccurred = true;
                                            }
                                        }));
                                        if (errorOccurred) { callback(false); yield break; }
                                    }
                                }
                            }
                        }
                    }
                    if (!errorOccurred)
                    {
                        transaction.Commit();
                        callback(true);
                    }
                }
            }
        }
        private IEnumerator ExecuteSaveOperation(Action saveOperation)
        {
            saveOperation();
            yield return null;
        }
    } 





    



