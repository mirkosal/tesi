using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Mono.Data.Sqlite;



public class BoneDBHandler : DatabaseObject<Bone, bool, List<Bone>, bool, bool>
{
    private static BoneDBHandler _instance;
    private static readonly object _lock = new object();

    public const string NomeTabella = "Bones";

    private BoneDBHandler(string DB) : base(DB, NomeTabella)
    {
    }

    public static BoneDBHandler Instance(string dbPath)
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new BoneDBHandler(dbPath);
                }
            }
        }
        return _instance;
    }
    // Metodo per aggiungere un osso al database
    public override bool Save(params object[] parameters)
    {
        Debug.Log("bone "+parameters[0]);
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is Bone))
        {
            throw new ArgumentException("Bone object is required");
        }

        Bone bone = (Bone)parameters[0];
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
            INSERT INTO Bones 
            (PrevJointX, PrevJointY, PrevJointZ, NextJointX, NextJointY, NextJointZ, CenterX, CenterY, CenterZ, RotationX, RotationY, RotationZ, RotationW, Length, Width, Type, FingerID) 
            VALUES 
            (@PrevJointX, @PrevJointY, @PrevJointZ, @NextJointX, @NextJointY, @NextJointZ, @CenterX, @CenterY, @CenterZ, @RotationX, @RotationY, @RotationZ, @RotationW, @Length, @Width, @Type, @FingerID)";
                dbCmd.CommandText = sqlQuery;

                // Utilizzo di AddParameter per aggiungere i parametri
                AddParameter(dbCmd, "@PrevJointX", bone.PrevJoint.x);
                AddParameter(dbCmd, "@PrevJointY", bone.PrevJoint.y);
                AddParameter(dbCmd, "@PrevJointZ", bone.PrevJoint.z);
                AddParameter(dbCmd, "@NextJointX", bone.NextJoint.x);
                AddParameter(dbCmd, "@NextJointY", bone.NextJoint.y);
                AddParameter(dbCmd, "@NextJointZ", bone.NextJoint.z);
                AddParameter(dbCmd, "@CenterX", bone.Center.x);
                AddParameter(dbCmd, "@CenterY", bone.Center.y);
                AddParameter(dbCmd, "@CenterZ", bone.Center.z);
                AddParameter(dbCmd, "@RotationX", bone.Rotation.x);
                AddParameter(dbCmd, "@RotationY", bone.Rotation.y);
                AddParameter(dbCmd, "@RotationZ", bone.Rotation.z);
                AddParameter(dbCmd, "@RotationW", bone.Rotation.w);
                AddParameter(dbCmd, "@Length", bone.Length);
                AddParameter(dbCmd, "@Width", bone.Width);
                AddParameter(dbCmd, "@Type", bone.Type);
                AddParameter(dbCmd, "@FingerID", bone.FingerID);

                dbCmd.ExecuteNonQuery();
                success = true;
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Error saving Bone: " + e.Message);
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

    public override Bone Load(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("BoneID is required as an int parameter");
        }

        int boneID = (int)parameters[0];
        Bone bone = null;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * FROM Bones WHERE BoneID = @BoneID";
                dbCmd.CommandText = sqlQuery;

                // Utilizzo di AddParameter per aggiungere il parametro @BoneID
                AddParameter(dbCmd, "@ID", boneID);

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        bone = new Bone
                        {
                            BoneID = boneID,
                            PrevJoint = new Vector3(reader.GetFloat(reader.GetOrdinal("PrevJointX")), reader.GetFloat(reader.GetOrdinal("PrevJointY")), reader.GetFloat(reader.GetOrdinal("PrevJointZ"))),
                            NextJoint = new Vector3(reader.GetFloat(reader.GetOrdinal("NextJointX")), reader.GetFloat(reader.GetOrdinal("NextJointY")), reader.GetFloat(reader.GetOrdinal("NextJointZ"))),
                            Center = new Vector3(reader.GetFloat(reader.GetOrdinal("CenterX")), reader.GetFloat(reader.GetOrdinal("CenterY")), reader.GetFloat(reader.GetOrdinal("CenterZ"))),
                            Rotation = new Quaternion(reader.GetFloat(reader.GetOrdinal("RotationX")), reader.GetFloat(reader.GetOrdinal("RotationY")), reader.GetFloat(reader.GetOrdinal("RotationZ")), reader.GetFloat(reader.GetOrdinal("RotationW"))),
                            Length = reader.GetFloat(reader.GetOrdinal("Length")),
                            Width = reader.GetFloat(reader.GetOrdinal("Width")),
                            Type = reader.GetInt32(reader.GetOrdinal("Type")),
                            FingerID = reader.IsDBNull(reader.GetOrdinal("FingerID")) ? 0 : reader.GetInt32(reader.GetOrdinal("FingerID"))
                        };
                    }
                    else
                    {
                        throw new Exception($"Bone not found with ID: {boneID}");
                    }
                }
            }
        }

        return bone;
    }


        public override List<Bone> Search(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                throw new ArgumentException("Search method requires at least two parameters: column name and search term.");
            }

            string columnName = parameters[0] as string;
            string searchTerm = parameters[1] as string;
            IDbTransaction transaction = parameters.Length > 2 && parameters[2] is IDbTransaction ? (IDbTransaction)parameters[2] : null;

            List<Bone> bones = new List<Bone>();

            using (IDbCommand dbCmd = transaction != null ? transaction.Connection.CreateCommand() : new SqliteConnection(connectionString).CreateCommand())
            {
                if (transaction != null)
                {
                    dbCmd.Transaction = transaction;
                }
                dbCmd.CommandText = $"SELECT * FROM Bones WHERE {columnName} LIKE @SearchTerm";
                AddParameter(dbCmd, "@SearchTerm", $"%{searchTerm}%");

                if (transaction == null)
                {
                    dbCmd.Connection.Open();
                }

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Bone bone = new Bone();
                        try
                        {
                            bone.BoneID = reader.GetInt32(reader.GetOrdinal("ID"));
                            bone.PrevJoint = new Vector3(reader.GetFloat(reader.GetOrdinal("PrevJointX")),
                                                         reader.GetFloat(reader.GetOrdinal("PrevJointY")),
                                                         reader.GetFloat(reader.GetOrdinal("PrevJointZ")));
                            bone.NextJoint = new Vector3(reader.GetFloat(reader.GetOrdinal("NextJointX")),
                                                         reader.GetFloat(reader.GetOrdinal("NextJointY")),
                                                         reader.GetFloat(reader.GetOrdinal("NextJointZ")));
                            bone.Center = new Vector3(reader.GetFloat(reader.GetOrdinal("CenterX")),
                                                      reader.GetFloat(reader.GetOrdinal("CenterY")),
                                                      reader.GetFloat(reader.GetOrdinal("CenterZ")));
                            bone.Rotation = new Quaternion(reader.GetFloat(reader.GetOrdinal("RotationX")),
                                                           reader.GetFloat(reader.GetOrdinal("RotationY")),
                                                           reader.GetFloat(reader.GetOrdinal("RotationZ")),
                                                           reader.GetFloat(reader.GetOrdinal("RotationW")));
                            bone.Length = reader.GetFloat(reader.GetOrdinal("Length"));
                            bone.Width = reader.GetFloat(reader.GetOrdinal("Width"));
                            bone.Type = reader.GetInt32(reader.GetOrdinal("Type"));
                            bone.FingerID = reader.IsDBNull(reader.GetOrdinal("FingerID")) ? 0 : reader.GetInt32(reader.GetOrdinal("FingerID"));

                            bones.Add(bone);
                        }
                        catch (IndexOutOfRangeException ex)
                        {
                            Debug.LogError($"Column not found: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error loading Bone: {ex.Message}");
                        }
                    }
                }

                if (transaction == null)
                {
                    dbCmd.Connection.Close();
                }
            }

            return bones;
        }
    


    private bool IsValidColumn(string columnName)
    {
        // Assicurati che l'elenco delle colonne valide includa il nuovo attributo BoneID
        var validColumns = new HashSet<string> { "ID", "PrevJointX", "PrevJointY", "PrevJointZ", "NextJointX", "NextJointY", "NextJointZ", "CenterX", "CenterY", "CenterZ", "RotationX", "RotationY", "RotationZ", "RotationW", "Length", "Width", "Type", "FingerID" };
        return validColumns.Contains(columnName);
    }


    private void AddParameter(IDbCommand command, string paramName, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = paramName;
        param.Value = value;
        command.Parameters.Add(param);
    }

    public override bool Delete(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("BoneID is required as an int parameter.");
        }

        int boneID = (int)parameters[0];
        bool success = false;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "DELETE FROM Bones WHERE ID = @BoneID";
                dbCmd.CommandText = sqlQuery;

                // Utilizzo di AddParameter per aggiungere il parametro @BoneID
                AddParameter(dbCmd, "@BoneID", boneID);

                int rowsAffected = dbCmd.ExecuteNonQuery();
                success = rowsAffected > 0;
            }
        }

        return success;
    }


    public override bool Edit(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is Bone))
        {
            throw new ArgumentException("Bone object is required for editing.");
        }

        Bone boneToUpdate = (Bone)parameters[0];

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = @"
            UPDATE Bones
            SET 
                PrevJointX = @PrevJointX, PrevJointY = @PrevJointY, PrevJointZ = @PrevJointZ, 
                NextJointX = @NextJointX, NextJointY = @NextJointY, NextJointZ = @NextJointZ, 
                CenterX = @CenterX, CenterY = @CenterY, CenterZ = @CenterZ, 
                RotationX = @RotationX, RotationY = @RotationY, RotationZ = @RotationZ, RotationW = @RotationW, 
                Length = @Length, Width = @Width, Type = @Type, FingerID = @FingerID
            WHERE BoneID = @BoneID";

                dbCmd.CommandText = sqlQuery;

                // Utilizzo di AddParameter per aggiungere i parametri al comando
                AddParameter(dbCmd, "@PrevJointX", boneToUpdate.PrevJoint.x);
                AddParameter(dbCmd, "@PrevJointY", boneToUpdate.PrevJoint.y);
                AddParameter(dbCmd, "@PrevJointZ", boneToUpdate.PrevJoint.z);
                AddParameter(dbCmd, "@NextJointX", boneToUpdate.NextJoint.x);
                AddParameter(dbCmd, "@NextJointY", boneToUpdate.NextJoint.y);
                AddParameter(dbCmd, "@NextJointZ", boneToUpdate.NextJoint.z);
                AddParameter(dbCmd, "@CenterX", boneToUpdate.Center.x);
                AddParameter(dbCmd, "@CenterY", boneToUpdate.Center.y);
                AddParameter(dbCmd, "@CenterZ", boneToUpdate.Center.z);
                AddParameter(dbCmd, "@RotationX", boneToUpdate.Rotation.x);
                AddParameter(dbCmd, "@RotationY", boneToUpdate.Rotation.y);
                AddParameter(dbCmd, "@RotationZ", boneToUpdate.Rotation.z);
                AddParameter(dbCmd, "@RotationW", boneToUpdate.Rotation.w);
                AddParameter(dbCmd, "@Length", boneToUpdate.Length);
                AddParameter(dbCmd, "@Width", boneToUpdate.Width);
                AddParameter(dbCmd, "@Type", boneToUpdate.Type);
                AddParameter(dbCmd, "@FingerID", boneToUpdate.FingerID);
                AddParameter(dbCmd, "@BoneID", boneToUpdate.BoneID);

                int rowsAffected = dbCmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }

}
