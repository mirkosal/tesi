using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using UnityEngine;
using Mono.Data.Sqlite;

public class LoginDBHandler : DatabaseObject<Login, bool, List<Login>, bool, bool>
{
    private string connectionString;

    public LoginDBHandler(string DB)
    {
        Debug.Log(DB);
        this.connectionString = DB;
    }

    // Implementazione dei metodi per caricare, salvare, modificare ed eliminare i dati di login
    // Ad esempio, il metodo per caricare un login potrebbe essere simile a questo:
    public override Login Load(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is string))
        {
            throw new ArgumentException("Username is required");
        }

        string username = (string)parameters[0];

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * FROM Logins WHERE Username = @Username";
                AddParameter(dbCmd, "@Username", username);

                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Login login = new Login
                        {
                            Username = reader["Username"].ToString(),
                            Password = reader["Password"].ToString()
                        };

                        return login;
                    }
                    else
                    {
                        throw new Exception("Login not found with Username: " + username);
                    }
                }
            }
        }
    }

    public override bool Save(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is Login))
        {
            throw new ArgumentException("Login object is required");
        }

        Login newLogin = (Login)parameters[0];

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "INSERT INTO Logins (Username, Password) VALUES (@Username, @Password)";
                AddParameter(dbCmd, "@Username", newLogin.Username);
                AddParameter(dbCmd, "@Password", newLogin.Password); // Assicurati che la password sia criptata

                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();
                return true;
            }
        }
    }
    public override bool Edit(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is Login))
        {
            throw new ArgumentException("Login object is required");
        }

        Login loginToUpdate = (Login)parameters[0];

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "UPDATE Logins SET Password = @Password WHERE Username = @Username";
                AddParameter(dbCmd, "@Password", loginToUpdate.Password); // Assicurati che la password sia criptata
                AddParameter(dbCmd, "@Username", loginToUpdate.Username);

                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();
                return true;
            }
        }
    }
    public override bool Delete(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is string))
        {
            throw new ArgumentException("Username is required");
        }

        string username = (string)parameters[0];

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "DELETE FROM Logins WHERE Username = @Username";
                AddParameter(dbCmd, "@Username", username);

                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();
                return true;
            }
        }
    }
    public override List<Login> Search(params object[] parameters)
    {
        string username = (string)parameters[0];
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentException("Username is required for search");
        }

        List<Login> foundLogins = new List<Login>();

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * FROM Logins WHERE Username = @Username";
                AddParameter(dbCmd, "@Username", username);

                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Login foundLogin = new Login
                        {
                            Username = reader["Username"].ToString(),
                            Password = reader["Password"].ToString() // Assicurati di gestire la password in modo sicuro
                        };

                        foundLogins.Add(foundLogin);
                    }
                }
            }
        }

        return foundLogins;
    }
    private void AddParameter(IDbCommand command, string parameterName, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = parameterName;
        param.Value = value ?? DBNull.Value;
        command.Parameters.Add(param);
    }

}