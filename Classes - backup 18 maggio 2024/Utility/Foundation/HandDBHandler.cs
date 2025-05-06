using System;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class HandDBHandler : DatabaseObject<Hand, bool, List<Hand>, bool, bool>
{
    private string connectionString;
    public const string NomeTabella = "Hands";


    public HandDBHandler(string dbPath)
    {
        this.connectionString = "URI=file:" + dbPath; // Assicurati che il percorso sia corretto
    }

    public override bool Save(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is Hand))
        {
            throw new ArgumentException("Hand object is required");
        }

        Hand hand = (Hand)parameters[0];
        bool success = false;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = @"
            INSERT INTO Hands 
            (FrameId, DeviceID, PalmPositionX, PalmPositionY, PalmPositionZ, PalmVelocityX, PalmVelocityY, PalmVelocityZ, PalmNormalX, PalmNormalY, PalmNormalZ, DirectionX, DirectionY, DirectionZ, RotationX, RotationY, RotationZ, RotationW, GrabStrength, PinchStrength, PinchDistance, IsExtended, TimeVisible) 
            VALUES 
            (@FrameId, @DeviceID, @PalmPositionX, @PalmPositionY, @PalmPositionZ, @PalmVelocityX, @PalmVelocityY, @PalmVelocityZ, @PalmNormalX, @PalmNormalY, @PalmNormalZ, @DirectionX, @DirectionY, @DirectionZ, @RotationX, @RotationY, @RotationZ, @RotationW, @GrabStrength, @PinchStrength, @PinchDistance, @IsExtended, @TimeVisible);
            SELECT last_insert_rowid();";
                dbCmd.CommandText = sqlQuery;

                AddParameter(dbCmd, "@FrameId", hand.FrameId);
                AddParameter(dbCmd, "@DeviceID", hand.DeviceID);
                AddParameter(dbCmd, "@PalmPositionX", hand.PalmPosition.x);
                AddParameter(dbCmd, "@PalmPositionY", hand.PalmPosition.y);
                AddParameter(dbCmd, "@PalmPositionZ", hand.PalmPosition.z);
                AddParameter(dbCmd, "@PalmVelocityX", hand.PalmVelocity.x);
                AddParameter(dbCmd, "@PalmVelocityY", hand.PalmVelocity.y);
                AddParameter(dbCmd, "@PalmVelocityZ", hand.PalmVelocity.z);
                AddParameter(dbCmd, "@PalmNormalX", hand.PalmNormal.x);
                AddParameter(dbCmd, "@PalmNormalY", hand.PalmNormal.y);
                AddParameter(dbCmd, "@PalmNormalZ", hand.PalmNormal.z);
                AddParameter(dbCmd, "@DirectionX", hand.Direction.x);
                AddParameter(dbCmd, "@DirectionY", hand.Direction.y);
                AddParameter(dbCmd, "@DirectionZ", hand.Direction.z);
                AddParameter(dbCmd, "@RotationX", hand.Rotation.x);
                AddParameter(dbCmd, "@RotationY", hand.Rotation.y);
                AddParameter(dbCmd, "@RotationZ", hand.Rotation.z);
                AddParameter(dbCmd, "@RotationW", hand.Rotation.w);
                AddParameter(dbCmd, "@GrabStrength", hand.GrabStrength);
                AddParameter(dbCmd, "@PinchStrength", hand.PinchStrength);
                AddParameter(dbCmd, "@PinchDistance", hand.PinchDistance);
                AddParameter(dbCmd, "@IsExtended", hand.IsExtended);
                AddParameter(dbCmd, "@TimeVisible", hand.TimeVisible);

                long handID = (long)dbCmd.ExecuteScalar();
                success = handID > 0;

                if (success && hand.Fingers != null)
                {
                    FingerDBHandler fingerDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<FingerDBHandler>("FingerDBHandler");
                    if (fingerDBHandler == null)
                    {
                        UnityEngine.Debug.LogError("Impossibile ottenere FingerDBHandler dal DatabaseManager");
                        return false; // o gestire diversamente
                    }

                    // Salvataggio di ogni Finger associato all'Hand
                    foreach (Finger finger in hand.Fingers)
                    {
                        finger.HandId = (int)handID; // Imposta l'HandId del Finger
                        fingerDBHandler.Save(finger);
                    }
                }
            }
        }

        return success;
    }

    private List<Finger> LoadFingersForHand(int handID)
    {
        List<Finger> fingers = new List<Finger>();

        // Utilizza il DatabaseManager per ottenere un'istanza di FingerDBHandler
        FingerDBHandler fingerDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<FingerDBHandler>("FingerDBHandler");
        if (fingerDBHandler == null)
        {
            UnityEngine.Debug.LogError("Impossibile ottenere FingerDBHandler dal DatabaseManager");
            return fingers; // Ritorna una lista vuota per evitare null reference exceptions
        }

        // Assumi che il metodo Search di FingerDBHandler possa accettare una coppia chiave/valore dove la chiave è il nome della colonna (in questo caso "HandId") e il valore è l'ID del Hand
        // La ricerca dovrebbe restituire tutte le Finger associate a quel HandId
        fingers = fingerDBHandler.Search("HandId", handID.ToString());

        return fingers;
    }

    public override Hand Load(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("HandID is required as an int parameter");
        }

        int handID = (int)parameters[0];
        Hand hand = null;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * FROM Hands WHERE Id = @HandID";
                dbCmd.CommandText = sqlQuery;

                // Utilizza AddParameter per impostare il parametro @HandID
                AddParameter(dbCmd, "@HandID", handID);

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hand = new Hand(); // Utilizza il costruttore senza parametri

                        // Imposta le proprietà una volta che l'istanza è stata creata
                        hand.FrameId = reader.GetInt32(reader.GetOrdinal("FrameId"));
                        hand.Id = reader.GetInt32(reader.GetOrdinal("Id"));
                        hand.PalmPosition = new Vector3(
                            reader.GetFloat(reader.GetOrdinal("PalmPositionX")),
                            reader.GetFloat(reader.GetOrdinal("PalmPositionY")),
                            reader.GetFloat(reader.GetOrdinal("PalmPositionZ"))
                        );
                        hand.PalmVelocity = new Vector3(
                            reader.GetFloat(reader.GetOrdinal("PalmVelocityX")),
                            reader.GetFloat(reader.GetOrdinal("PalmVelocityY")),
                            reader.GetFloat(reader.GetOrdinal("PalmVelocityZ"))
                        );
                        hand.PalmNormal = new Vector3(
                            reader.GetFloat(reader.GetOrdinal("PalmNormalX")),
                            reader.GetFloat(reader.GetOrdinal("PalmNormalY")),
                            reader.GetFloat(reader.GetOrdinal("PalmNormalZ"))
                        );
                        hand.Direction = new Vector3(
                            reader.GetFloat(reader.GetOrdinal("DirectionX")),
                            reader.GetFloat(reader.GetOrdinal("DirectionY")),
                            reader.GetFloat(reader.GetOrdinal("DirectionZ"))
                        );
                        hand.Rotation = new Quaternion(
                            reader.GetFloat(reader.GetOrdinal("RotationX")),
                            reader.GetFloat(reader.GetOrdinal("RotationY")),
                            reader.GetFloat(reader.GetOrdinal("RotationZ")),
                            reader.GetFloat(reader.GetOrdinal("RotationW"))
                        );
                        hand.GrabStrength = reader.GetFloat(reader.GetOrdinal("GrabStrength"));
                        hand.PinchStrength = reader.GetFloat(reader.GetOrdinal("PinchStrength"));
                        hand.PinchDistance = reader.GetFloat(reader.GetOrdinal("PinchDistance"));
                        hand.IsExtended = reader.GetBoolean(reader.GetOrdinal("IsExtended"));
                        hand.TimeVisible = reader.GetDouble(reader.GetOrdinal("TimeVisible"));
                    }
                }

                // Now load the associated Fingers for the Hand
                if (hand != null)
                {
                    hand.Fingers = LoadFingersForHand(hand.Id);
                }
            }
        }

        return hand;
    }


    public override List<Hand> Search(params object[] parameters)
    {
        if (parameters == null || parameters.Length < 2)
        {
            throw new ArgumentException("At least two parameters are required for searching: column name and search term.");
        }

        string columnName = parameters[0] as string;
        if (string.IsNullOrEmpty(columnName))
        {
            throw new ArgumentException("The column name must be a valid string.");
        }
        object searchTerm = parameters[1];
        List<Hand> foundHands = new List<Hand>();

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = $"SELECT * FROM Hands WHERE {columnName} = @SearchTerm";
                dbCmd.CommandText = sqlQuery;

                // Aggiunge il parametro di ricerca alla query
                AddParameter(dbCmd, "@SearchTerm", searchTerm);

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Hand hand = new Hand();  // Utilizza il costruttore senza parametri

                        // Imposta le proprietà una volta che l'istanza è stata creata
                        hand.FrameId = reader.GetInt32(reader.GetOrdinal("FrameId"));
                        hand.Id = reader.GetInt32(reader.GetOrdinal("Id"));
                        hand.PalmPosition = new Vector3(
                            reader.GetFloat(reader.GetOrdinal("PalmPositionX")),
                            reader.GetFloat(reader.GetOrdinal("PalmPositionY")),
                            reader.GetFloat(reader.GetOrdinal("PalmPositionZ"))
                        );
                        hand.PalmVelocity = new Vector3(
                            reader.GetFloat(reader.GetOrdinal("PalmVelocityX")),
                            reader.GetFloat(reader.GetOrdinal("PalmVelocityY")),
                            reader.GetFloat(reader.GetOrdinal("PalmVelocityZ"))
                        );
                        hand.PalmNormal = new Vector3(
                            reader.GetFloat(reader.GetOrdinal("PalmNormalX")),
                            reader.GetFloat(reader.GetOrdinal("PalmNormalY")),
                            reader.GetFloat(reader.GetOrdinal("PalmNormalZ"))
                        );
                        hand.Direction = new Vector3(
                            reader.GetFloat(reader.GetOrdinal("DirectionX")),
                            reader.GetFloat(reader.GetOrdinal("DirectionY")),
                            reader.GetFloat(reader.GetOrdinal("DirectionZ"))
                        );
                        hand.Rotation = new Quaternion(
                            reader.GetFloat(reader.GetOrdinal("RotationX")),
                            reader.GetFloat(reader.GetOrdinal("RotationY")),
                            reader.GetFloat(reader.GetOrdinal("RotationZ")),
                            reader.GetFloat(reader.GetOrdinal("RotationW"))
                        );
                        hand.GrabStrength = reader.GetFloat(reader.GetOrdinal("GrabStrength"));
                        hand.PinchStrength = reader.GetFloat(reader.GetOrdinal("PinchStrength"));
                        hand.PinchDistance = reader.GetFloat(reader.GetOrdinal("PinchDistance"));
                        hand.IsExtended = reader.GetBoolean(reader.GetOrdinal("IsExtended"));
                        hand.TimeVisible = reader.GetDouble(reader.GetOrdinal("TimeVisible"));

                        // Carica le Fingers associate a questo Hand
                        hand.Fingers = LoadFingersForHand(hand.Id);

                        foundHands.Add(hand);
                    }
                }
            }
        }

        return foundHands;
    }


    public override bool Delete(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("HandID is required as an int parameter.");
        }

        int handID = (int)parameters[0];
        bool success = false;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            // Prima, elimina tutte le Finger associate al Hand specificato
            bool fingersDeleted = DeleteFingersForHand(handID, dbConnection);

            if (!fingersDeleted)
            {
                UnityEngine.Debug.LogError("Errore nell'eliminazione delle Finger per il Hand con ID: " + handID);
                return false; // Oppure gestire diversamente l'errore
            }

            // Poi, elimina il Hand stesso
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "DELETE FROM Hands WHERE Id = @HandID";
                dbCmd.CommandText = sqlQuery;

                // Utilizza AddParameter per impostare il parametro @HandID
                AddParameter(dbCmd, "@HandID", handID);

                int rowsAffected = dbCmd.ExecuteNonQuery();

                success = rowsAffected > 0;
            }
        }

        return success;
    }
    private bool DeleteFingersForHand(int handID, IDbConnection dbConnection)
    {
        // Ottiene tutte le Finger associate al Hand
        FingerDBHandler fingerDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<FingerDBHandler>("FingerDBHandler");
        if (fingerDBHandler == null)
        {
            UnityEngine.Debug.LogError("Impossibile ottenere FingerDBHandler dal DatabaseManager");
            return false;
        }

        List<Finger> fingers = fingerDBHandler.Search("HandId", handID.ToString());
        bool allFingersDeleted = true;

        foreach (Finger finger in fingers)
        {
            bool deleted = fingerDBHandler.Delete(finger.Id.ToString());
            if (!deleted)
            {
                UnityEngine.Debug.LogError("Impossibile eliminare Finger con ID: " + finger.Id);
                allFingersDeleted = false;
                // Potresti decidere di interrompere il ciclo qui con un `break;` se vuoi evitare di continuare dopo il primo errore.
            }
        }

        return allFingersDeleted;
    }

    public override bool Edit(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is Hand))
        {
            throw new ArgumentException("Hand object is required for editing.");
        }

        Hand handToUpdate = (Hand)parameters[0];
        bool success = false;

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            // Aggiornamento dell'oggetto Hand nel database
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = @"
            UPDATE Hands
            SET 
                FrameId = @FrameId, DeviceID = @DeviceID, PalmPositionX = @PalmPositionX, PalmPositionY = @PalmPositionY,
                PalmPositionZ = @PalmPositionZ, PalmVelocityX = @PalmVelocityX, PalmVelocityY = @PalmVelocityY,
                PalmVelocityZ = @PalmVelocityZ, PalmNormalX = @PalmNormalX, PalmNormalY = @PalmNormalY,
                PalmNormalZ = @PalmNormalZ, DirectionX = @DirectionX, DirectionY = @DirectionY, DirectionZ = @DirectionZ,
                RotationX = @RotationX, RotationY = @RotationY, RotationZ = @RotationZ, RotationW = @RotationW,
                GrabStrength = @GrabStrength, PinchStrength = @PinchStrength, PinchDistance = @PinchDistance,
                IsExtended = @IsExtended, TimeVisible = @TimeVisible
            WHERE Id = @Id";
                dbCmd.CommandText = sqlQuery;

                // Aggiunge i parametri utilizzando AddParameter
                AddParameter(dbCmd, "@Id", handToUpdate.Id);
                AddParameter(dbCmd, "@FrameId", handToUpdate.FrameId);
                AddParameter(dbCmd, "@DeviceID", handToUpdate.DeviceID);
                AddParameter(dbCmd, "@PalmPositionX", handToUpdate.PalmPosition.x);
                AddParameter(dbCmd, "@PalmPositionY", handToUpdate.PalmPosition.y);
                AddParameter(dbCmd, "@PalmPositionZ", handToUpdate.PalmPosition.z);
                AddParameter(dbCmd, "@PalmVelocityX", handToUpdate.PalmVelocity.x);
                AddParameter(dbCmd, "@PalmVelocityY", handToUpdate.PalmVelocity.y);
                AddParameter(dbCmd, "@PalmVelocityZ", handToUpdate.PalmVelocity.z);
                AddParameter(dbCmd, "@PalmNormalX", handToUpdate.PalmNormal.x);
                AddParameter(dbCmd, "@PalmNormalY", handToUpdate.PalmNormal.y);
                AddParameter(dbCmd, "@PalmNormalZ", handToUpdate.PalmNormal.z);
                AddParameter(dbCmd, "@DirectionX", handToUpdate.Direction.x);
                AddParameter(dbCmd, "@DirectionY", handToUpdate.Direction.y);
                AddParameter(dbCmd, "@DirectionZ", handToUpdate.Direction.z);
                AddParameter(dbCmd, "@RotationX", handToUpdate.Rotation.x);
                AddParameter(dbCmd, "@RotationY", handToUpdate.Rotation.y);
                AddParameter(dbCmd, "@RotationZ", handToUpdate.Rotation.z);
                AddParameter(dbCmd, "@RotationW", handToUpdate.Rotation.w);
                AddParameter(dbCmd, "@GrabStrength", handToUpdate.GrabStrength);
                AddParameter(dbCmd, "@PinchStrength", handToUpdate.PinchStrength);
                AddParameter(dbCmd, "@PinchDistance", handToUpdate.PinchDistance);
                AddParameter(dbCmd, "@IsExtended", handToUpdate.IsExtended);
                AddParameter(dbCmd, "@TimeVisible", handToUpdate.TimeVisible);

                int rowsAffected = dbCmd.ExecuteNonQuery();
                success = rowsAffected > 0;
            }
        }

        return success;
    }

}
