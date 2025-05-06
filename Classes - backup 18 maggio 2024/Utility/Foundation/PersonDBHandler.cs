using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using UnityEngine;
using Mono.Data.Sqlite;


public class PersonDBHandler: DatabaseObject<Person, bool, List<Person>, bool, bool>
{
 

    public PersonDBHandler(string DB)
    {
        connectionString = DB; 
            NomeTabella = "Persons";
        UnityEngine.Debug.Log(DB);
    }

    public override Person Load(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("ID is required");
        }

        int id = (int)parameters[0];

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * FROM Persons WHERE ID = @ID";
                AddParameter(dbCmd, "@ID", id);

                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Person
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Diagnosis = reader["Diagnosis"].ToString(),
                            DateOfBirth = DateTime.Parse(reader["DateOfBirth"].ToString()),
                            TaxCode = reader["TaxCode"].ToString()
                        };
                    }
                    else
                    {
                        throw new Exception("Person not found");
                    }
                }
            }
        }
    }


    public override bool Save(params object[] parameters)
    {
    if (parameters == null || parameters.Length == 0 || !(parameters[0] is Person))
    {
        throw new ArgumentException("Person object is required");
    }

    Person person = (Person)parameters[0];

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "INSERT INTO Persons (FirstName, LastName, Diagnosis, DateOfBirth, TaxCode) VALUES (@FirstName, @LastName, @Diagnosis, @DateOfBirth, @TaxCode)";

                AddParameter(dbCmd, "@FirstName", person.FirstName);
                AddParameter(dbCmd, "@LastName", person.LastName);
                AddParameter(dbCmd, "@Diagnosis", person.Diagnosis);
                AddParameter(dbCmd, "@DateOfBirth", person.DateOfBirth.ToString("yyyy-MM-dd"));
                AddParameter(dbCmd, "@TaxCode", person.TaxCode);

                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();
            }            
            dbConnection.Close();
            return true;
        }
    }

    private void AddParameter(IDbCommand command, string paramName, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = paramName;
        param.Value = value;
        command.Parameters.Add(param);
    }


    public override List<Person> Search(params object[] parameters)
    {
        if (parameters == null || parameters.Length != 2)
        {
            throw new ArgumentException("Two parameters are required: column name and search term");
        }

        string columnName = parameters[0] as string;
        string searchTerm = parameters[1] as string;
        Debug.Log("ciao");
        // Verifica la validità della colonna
        if (!IsValidColumn(columnName))
        {
            throw new ArgumentException("Invalid column name");
        }

        List<Person> foundPeople = new List<Person>();

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = $"SELECT * FROM Persons WHERE {columnName} LIKE @SearchTerm";
                AddParameter(dbCmd, "@SearchTerm", "%" + searchTerm + "%");

                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Person person = new Person
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Diagnosis = reader["Diagnosis"].ToString(),
                            DateOfBirth = DateTime.Parse(reader["DateOfBirth"].ToString()),
                            TaxCode = reader["TaxCode"].ToString()
                        };

                        foundPeople.Add(person);
                    }
                }
            }

            dbConnection.Close();
        }

        return foundPeople;
    }

    private bool IsValidColumn(string columnName)
    {
        // Lista di colonne valide
        var validColumns = new HashSet<string> { "FirstName", "LastName", "Diagnosis", "DateOfBirth", "TaxCode","ID" };
        return validColumns.Contains(columnName);
    }


    public override bool Delete(params object[] parameters)
    {
        if (parameters == null || parameters.Length == 0 || !(parameters[0] is int))
        {
            throw new ArgumentException("ID is required");
        }

        int id = (int)parameters[0];

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "DELETE FROM Persons WHERE ID = @ID";
                AddParameter(dbCmd, "@ID", id);

                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();
                return true;
            }
        }
    }


    public override bool Edit(params object[] parameters)
    {
        if (parameters == null || parameters.Length < 2 || !(parameters[0] is int) || !(parameters[1] is Person))
        {
            throw new ArgumentException("ID and Person object are required");
        }

        int id = (int)parameters[0];
        Person personToUpdate = (Person)parameters[1];

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "UPDATE Persons SET FirstName = @FirstName, LastName = @LastName, Diagnosis = @Diagnosis, DateOfBirth = @DateOfBirth WHERE ID = @ID";

                AddParameter(dbCmd, "@ID", id);
                AddParameter(dbCmd, "@FirstName", personToUpdate.FirstName);
                AddParameter(dbCmd, "@LastName", personToUpdate.LastName);
                AddParameter(dbCmd, "@Diagnosis", personToUpdate.Diagnosis);
                AddParameter(dbCmd, "@DateOfBirth", personToUpdate.DateOfBirth.ToString("yyyy-MM-dd"));

                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();
            }

            dbConnection.Close();
            return true;
        }
    }


}
