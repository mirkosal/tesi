using System.Collections.Generic;
using UnityEngine;

public class OptionController
{
    private CustomLocaleManager localeManager;
    private static OptionController _instance;
    public static OptionController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new OptionController();
            }
            return _instance;
        }
    }

    private DatabaseManager dbManager;
    private LoginDBHandler loginDBHandler;
    private PersonDBHandler personDBHandler;

    private OptionController()
    {
        dbManager = DatabaseManager.Instance;
        loginDBHandler = dbManager.GetDatabaseObjectInstance<LoginDBHandler>("LoginDBHandler");
        personDBHandler = dbManager.GetDatabaseObjectInstance<PersonDBHandler>("PersonDBHandler");
        localeManager = new CustomLocaleManager();
    }

    // Metodi esistenti per Login
    public void AddLogin(string username, string password)
    {
        Login newLogin = new Login { Username = username, Password = password };
        loginDBHandler.Save(newLogin);
    }

    public void DeleteLogin(string username)
    {
        loginDBHandler.Delete(username);
    }

    public List<string> GetAllLogins()
    {
        List<string> loginUsernames = new List<string>();
        List<Login> logins = loginDBHandler.Search("%");
        foreach (var login in logins)
        {
            loginUsernames.Add(login.Username);
        }
        return loginUsernames;
    }

    // Nuovi metodi per Person
    public void AddPerson(Person person)
    {
        personDBHandler.Save(person);
    }

    public void DeletePersonByName(string fullName)
    {
        var person = GetPersonByName(fullName);
        if (person != null)
        {
            personDBHandler.Delete(person.ID);
        }
    }

    public void SetLanguage(string language)
    {
        localeManager.SetLanguage(language);
    }

    public string GetCurrentLanguage()
    {
        Debug.Log(localeManager.GetCurrentLanguage());
        return localeManager.GetCurrentLanguage();
    }

    public List<Person> GetAllPersons()
    {
        return personDBHandler.Search("ID", "%");
    }

    public Person GetPersonByName(string fullName)
    {
        var names = fullName.Split(' ');
        if (names.Length < 2) return null;

        string firstName = names[0];
        string lastName = names[1];

        var persons = personDBHandler.Search("FirstName", firstName);
        foreach (var person in persons)
        {
            if (person.LastName == lastName)
            {
                return person;
            }
        }

        return null;
    }
}
