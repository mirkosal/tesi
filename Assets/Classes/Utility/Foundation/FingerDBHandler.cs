using System;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections.Generic;


public class FingerDBHandler : DatabaseObject<Finger, bool, List<Finger>, bool, bool>
{
    private static FingerDBHandler _instance;
    private static readonly object _lock = new object();
 
    public const string NomeTabella = "Fingers";

    private FingerDBHandler(string DB) : base(DB, NomeTabella)
    {
        Debug.Log(DB);
    }

    public static FingerDBHandler Instance(string dbPath)
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new FingerDBHandler(dbPath);
                }
            }
        }
        return _instance;
    }

    public override bool Save(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is Finger))
        {
            throw new ArgumentException("Finger object is required");
        }

        Finger finger = (Finger)parameters[0];
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
                INSERT INTO Fingers 
                (Type, HandId, TipPositionX, TipPositionY, TipPositionZ, DirectionX, DirectionY, DirectionZ, Width, Length, IsExtended, TimeVisible) 
                VALUES 
                (@Type, @HandId, @TipPositionX, @TipPositionY, @TipPositionZ, @DirectionX, @DirectionY, @DirectionZ, @Width, @Length, @IsExtended, @TimeVisible);
                SELECT last_insert_rowid();";

                dbCmd.CommandText = sqlQuery;

                // Utilizzo del metodo AddParameter per aggiungere i parametri
                AddParameter(dbCmd, "@Type", finger.Type);
                AddParameter(dbCmd, "@HandId", finger.HandId);
                AddParameter(dbCmd, "@TipPositionX", finger.TipPosition.x);
                AddParameter(dbCmd, "@TipPositionY", finger.TipPosition.y);
                AddParameter(dbCmd, "@TipPositionZ", finger.TipPosition.z);
                AddParameter(dbCmd, "@DirectionX", finger.Direction.x);
                AddParameter(dbCmd, "@DirectionY", finger.Direction.y);
                AddParameter(dbCmd, "@DirectionZ", finger.Direction.z);
                AddParameter(dbCmd, "@Width", finger.Width);
                AddParameter(dbCmd, "@Length", finger.Length);
                AddParameter(dbCmd, "@IsExtended", finger.IsExtended);
                AddParameter(dbCmd, "@TimeVisible", finger.TimeVisible);

                long fingerID = (long)dbCmd.ExecuteScalar();
                success = fingerID > 0;

                if (success && finger.bones != null)
                {
                    BoneDBHandler boneDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<BoneDBHandler>("BoneDBHandler");
                    if (boneDBHandler == null)
                    {
                        UnityEngine.Debug.LogError("Unable to obtain BoneDBHandler from DatabaseManager");
                        return false; // or handle differently
                    }

                    foreach (Bone bone in finger.bones)
                    {
                        bone.FingerID = (int)fingerID;
                        boneDBHandler.Save(bone, dbConnection, transaction);
                    }
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Error saving Finger: " + e.Message);
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

 

    // I seguenti metodi devono essere implementati per completare la classe.
    // Sono lasciati come esercizio al lettore.
    public override Finger Load(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("FingerID is required as an int parameter");
        }

        int fingerID = (int)parameters[0];
        Finger finger = null;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * FROM Fingers WHERE Id = @FingerID";
                dbCmd.CommandText = sqlQuery;

                // Utilizza AddParameter per aggiungere il parametro @FingerID
                AddParameter(dbCmd, "@FingerID", fingerID);

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        finger = new Finger
                        {
                            // Supponendo che Finger abbia una struttura simile. Adatta questi campi come necessario.
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Type = reader.GetInt32(reader.GetOrdinal("Type")),
                            HandId = reader.GetInt32(reader.GetOrdinal("HandId")),
                            TipPosition = new Vector3(
                                reader.GetFloat(reader.GetOrdinal("TipPositionX")),
                                reader.GetFloat(reader.GetOrdinal("TipPositionY")),
                                reader.GetFloat(reader.GetOrdinal("TipPositionZ"))),
                            Direction = new Vector3(
                                reader.GetFloat(reader.GetOrdinal("DirectionX")),
                                reader.GetFloat(reader.GetOrdinal("DirectionY")),
                                reader.GetFloat(reader.GetOrdinal("DirectionZ"))),
                            Width = reader.GetFloat(reader.GetOrdinal("Width")),
                            Length = reader.GetFloat(reader.GetOrdinal("Length")),
                            IsExtended = reader.GetBoolean(reader.GetOrdinal("IsExtended")),
                            TimeVisible = reader.GetDouble(reader.GetOrdinal("TimeVisible"))
                        };

                        // Potresti voler caricare qui ulteriori dettagli o relazioni per il Finger, come i Bones
                       
                    }
                }
            }
        }

        return finger;
    }




        public override List<Finger> Search(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                throw new ArgumentException("Search method requires at least two parameters: column name and search term.");
            }

            string columnName = parameters[0] as string;
            string searchTerm = parameters[1] as string;
            IDbTransaction transaction = parameters.Length > 2 && parameters[2] is IDbTransaction ? (IDbTransaction)parameters[2] : null;

            List<Finger> fingers = new List<Finger>();

            using (IDbCommand dbCmd = transaction != null ? transaction.Connection.CreateCommand() : new SqliteConnection(connectionString).CreateCommand())
            {
                if (transaction != null)
                {
                    dbCmd.Transaction = transaction;
                }
                dbCmd.CommandText = $"SELECT * FROM Fingers WHERE {columnName} LIKE @SearchTerm";
                AddParameter(dbCmd, "@SearchTerm", $"%{searchTerm}%");

                if (transaction == null)
                {
                    dbCmd.Connection.Open();
                }

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Finger finger = new Finger();
                        try
                        {
                            finger.Id = reader.GetInt32(reader.GetOrdinal("Id"));
                            finger.Type = reader.GetInt32(reader.GetOrdinal("Type"));
                            finger.HandId = reader.GetInt32(reader.GetOrdinal("HandId"));
                            finger.TipPosition = new Vector3(reader.GetFloat(reader.GetOrdinal("TipPositionX")),
                                                             reader.GetFloat(reader.GetOrdinal("TipPositionY")),
                                                             reader.GetFloat(reader.GetOrdinal("TipPositionZ")));
                            finger.Direction = new Vector3(reader.GetFloat(reader.GetOrdinal("DirectionX")),
                                                           reader.GetFloat(reader.GetOrdinal("DirectionY")),
                                                           reader.GetFloat(reader.GetOrdinal("DirectionZ")));
                            finger.Width = reader.GetFloat(reader.GetOrdinal("Width"));
                            finger.Length = reader.GetFloat(reader.GetOrdinal("Length"));
                            finger.IsExtended = reader.GetBoolean(reader.GetOrdinal("IsExtended"));
                            finger.TimeVisible = reader.GetDouble(reader.GetOrdinal("TimeVisible"));
                            finger.bones = new List<Bone>();

                           

                            fingers.Add(finger);
                        }
                        catch (IndexOutOfRangeException ex)
                        {
                            Debug.LogError($"Column not found: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error loading Finger: {ex.Message}");
                        }
                    }
                }

                if (transaction == null)
                {
                    dbCmd.Connection.Close();
                }
            }

            return fingers;
        }

 



    public override bool Delete(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("FingerID is required as an int parameter.");
        }

        int fingerID = (int)parameters[0];
        bool success = false;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            // Elimina prima tutti i Bones associati al Finger specificato
            success = DeleteBonesForFinger(fingerID, dbConnection);

            if (!success)
            {
                UnityEngine.Debug.LogError("Failed to delete Bones for Finger with ID: " + fingerID);
                return false;
            }

            // Ora elimina il Finger stesso
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "DELETE FROM Fingers WHERE Id = @FingerID";
                dbCmd.CommandText = sqlQuery;

                // Utilizzo di AddParameter per aggiungere il parametro @FingerID
                AddParameter(dbCmd, "@FingerID", fingerID);

                int rowsAffected = dbCmd.ExecuteNonQuery();
                success = rowsAffected > 0;
            }
        }

        return success;
    }

    private bool DeleteBonesForFinger(int fingerID, IDbConnection dbConnection)
    {
        // Utilizza il DatabaseManager per ottenere un'istanza di BoneDBHandler
        BoneDBHandler boneDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<BoneDBHandler>("BoneDBHandler");
        if (boneDBHandler == null)
        {
            UnityEngine.Debug.LogError("Unable to obtain BoneDBHandler from DatabaseManager");
            return false;
        }

        // Supponendo che BoneDBHandler abbia un metodo Search che accetti FingerID come parametro di ricerca
        List<Bone> bonesToDelete = boneDBHandler.Search("FingerID", fingerID.ToString());

        foreach (Bone bone in bonesToDelete)
        {
            // Supponendo che il metodo Delete in BoneDBHandler accetti l'Id del Bone come parametro
            bool deleted = boneDBHandler.Delete(bone.BoneID);
            if (!deleted)
            {
                UnityEngine.Debug.LogError("Failed to delete Bone with ID: " + bone.BoneID);
                // Potresti decidere di continuare e tentare di eliminare le altre Bones
                return false;
            }
        }

        return true;
    }


    public override bool Edit(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is Finger))
        {
            throw new ArgumentException("Finger object is required for editing.");
        }

        Finger fingerToUpdate = (Finger)parameters[0];
        bool success = false;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = @"
            UPDATE Fingers
            SET 
                Type = @Type, 
                HandId = @HandId, 
                TipPositionX = @TipPositionX, 
                TipPositionY = @TipPositionY, 
                TipPositionZ = @TipPositionZ, 
                DirectionX = @DirectionX, 
                DirectionY = @DirectionY, 
                DirectionZ = @DirectionZ, 
                Width = @Width, 
                Length = @Length, 
                IsExtended = @IsExtended, 
                TimeVisible = @TimeVisible
            WHERE Id = @Id";

                dbCmd.CommandText = sqlQuery;

                // Utilizzo di AddParameter per aggiungere i parametri al comando
                AddParameter(dbCmd, "@Type", fingerToUpdate.Type);
                AddParameter(dbCmd, "@HandId", fingerToUpdate.HandId);
                AddParameter(dbCmd, "@TipPositionX", fingerToUpdate.TipPosition.x);
                AddParameter(dbCmd, "@TipPositionY", fingerToUpdate.TipPosition.y);
                AddParameter(dbCmd, "@TipPositionZ", fingerToUpdate.TipPosition.z);
                AddParameter(dbCmd, "@DirectionX", fingerToUpdate.Direction.x);
                AddParameter(dbCmd, "@DirectionY", fingerToUpdate.Direction.y);
                AddParameter(dbCmd, "@DirectionZ", fingerToUpdate.Direction.z);
                AddParameter(dbCmd, "@Width", fingerToUpdate.Width);
                AddParameter(dbCmd, "@Length", fingerToUpdate.Length);
                AddParameter(dbCmd, "@IsExtended", fingerToUpdate.IsExtended);
                AddParameter(dbCmd, "@TimeVisible", fingerToUpdate.TimeVisible);
                AddParameter(dbCmd, "@Id", fingerToUpdate.Id);

                int rowsAffected = dbCmd.ExecuteNonQuery();
                success = rowsAffected > 0;
            }
        }

        return success;
    }

    

}
