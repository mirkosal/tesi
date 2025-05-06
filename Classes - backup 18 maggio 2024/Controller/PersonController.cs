using System.Collections.Generic;
using UnityEngine;

public class PersonController
{
    private DatabaseManager databaseManager;

    public PersonController()
    {
        databaseManager = DatabaseManager.Instance;
    }

    public List<(Person, List<TaskGroup>)> SearchForPersonsAndTaskGroups(string searchField, string searchTerm)
    {
        PersonDBHandler personDbHandler = databaseManager.GetDatabaseObjectInstance<PersonDBHandler>("PersonDBHandler");
        if (personDbHandler == null)
        {
            throw new System.Exception("PersonDBHandler instance not found");
        }

        var persons = personDbHandler.Search(searchField, searchTerm);
        List<(Person, List<TaskGroup>)> results = new List<(Person, List<TaskGroup>)>();

        foreach (var person in persons)
        {
            TaskGroupDBHandler taskGroupDbHandler = databaseManager.GetDatabaseObjectInstance<TaskGroupDBHandler>("TaskGroupDBHandler");
            if (taskGroupDbHandler == null)
            {
                throw new System.Exception("TaskGroupDBHandler instance not found");
            }

            var taskGroups = taskGroupDbHandler.Search("PersonID", person.ID.ToString(), false);
            results.Add((person, taskGroups));
        }

        return results;
    }

    public void HandleConfirmButton()
    {
        TaskGroupDBHandler taskGroupDbHandler = databaseManager.GetDatabaseObjectInstance<TaskGroupDBHandler>("TaskGroupDBHandler");
    
        // Crea un nuovo TaskGroup e imposta la persona corrente come proprietario
        TaskGroup newTaskGroup = new TaskGroup("Nome del nuovo TaskGroup", DataManager.CurrentPerson.ID);

        // Carica il TaskGroup creato in DataManager
        DataManager.LoadTaskGroup(newTaskGroup);
        DataManager.CurrentTaskGroup.ID = taskGroupDbHandler.getId();
        UnityEngine.Debug.Log(DataManager.CurrentTaskGroup.ID);
        UnityEngine.SceneManagement.SceneManager.LoadScene("creareTask");
    }

}
