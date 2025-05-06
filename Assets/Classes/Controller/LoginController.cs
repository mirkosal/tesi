using System;
using System.Collections.Generic;
using UnityEngine;

public class LoginController : IController<Login>
{
    private DatabaseManager databaseManager;
    private CustomLocaleManager localeManager;

    public LoginController()
    {
        databaseManager = DatabaseManager.Instance;
        localeManager = new CustomLocaleManager();
    }

    public void SetLanguage(string language)
    {
        localeManager.SetLanguage(language);
    }

    public Login ExecuteTask(params object[] parameters)
    {
        if (parameters.Length < 2 || parameters[0] == null || parameters[1] == null)
        {
            throw new ArgumentException("Username and password are required.");
        }

        string username = parameters[0].ToString();
        string password = parameters[1].ToString();

        var dbLogin = databaseManager.GetDatabaseObjectInstance<LoginDBHandler>("LoginDBHandler");
        List<Login> searchResult = dbLogin.Search(username);

        foreach (var login in searchResult)
        {
            if (login.Password == password)
            {
                return login;
            }
        }

        return null;
    }
}
