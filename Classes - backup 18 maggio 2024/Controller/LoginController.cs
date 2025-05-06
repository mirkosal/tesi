using System;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

public class LoginController : IController<Login>
{
    private DatabaseManager databaseManager;

    public LoginController()
    {
        databaseManager = DatabaseManager.Instance;
    }

    public Login ExecuteTask(params object[] parameters)
    {
        if (parameters.Length < 2 || parameters[0] == null || parameters[1] == null)
        {
            throw new ArgumentException("Username and password are required.");
        }

        string username = parameters[0].ToString();
        string password = parameters[1].ToString();

        // Ottieni l'istanza di LoginDBHandler dal DatabaseManager
        var dbLogin = databaseManager.GetDatabaseObjectInstance<LoginDBHandler>("LoginDBHandler");

        // Cerca in base all'username
        List<Login> searchResult = dbLogin.Search(username);
        foreach (var login in searchResult)
        {
            if (login.Password == password) // Assumi che la password sia già stata criptata/confrontata in modo sicuro
            {
                return login;
            }
        }

        // Nessun login valido trovato
        return null;
    }
}
