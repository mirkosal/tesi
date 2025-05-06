using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using System;
using System.Linq;


public class CreateTaskController
{
    public List<Hand> Hands { get; private set; } = new List<Hand>();
    public TaskGroup CurrentTaskGroup { get; private set; } // Mantiene lo stato dell'oggetto TaskGroup corrente

    public CreateTaskController()
    {
        InitializeCurrentTaskGroup();
    }

    private void InitializeCurrentTaskGroup()
    {
        // Crea un'istanza di TaskGroupDBHandler tramite DatabaseManager
        TaskGroupDBHandler taskGroupDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<TaskGroupDBHandler>("TaskGroupDBHandler");
        if (taskGroupDBHandler != null)
        {
            int taskGroupId = taskGroupDBHandler.getId(); // Chiamata al metodo getId per ottenere l'ID del TaskGroup

            // Se l'ID ottenuto è valido, inizializza CurrentTaskGroup con quello specifico ID (questo presupporrebbe che TaskGroup abbia un costruttore o un metodo per impostare l'ID)
            if (taskGroupId > 0)
            {
                // Qui si assume che esista un modo per inizializzare TaskGroup con un ID specifico.
                // Modifica la seguente linea in base alla logica effettiva di TaskGroup per utilizzare l'ID.
                CurrentTaskGroup = new TaskGroup("NomeTaskGroupUnico", taskGroupId); // L'ID e il nome sono esempi, sostituire con logica appropriata
            }
            else
            {
                // Gestire il caso in cui non si ottenga un ID valido
                UnityEngine.Debug.LogError("Failed to get a valid TaskGroup ID from the database.");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Unable to obtain TaskGroupDBHandler from DatabaseManager");
        }
    }


    public void HandleJsonData(string jsonData, int repetitions)
    {
        string convertedJson = JsonConverter.ConvertMongoToJsonStandard(jsonData);
        List<HandData> handDataList = JsonConvert.DeserializeObject<List<HandData>>(convertedJson);
        // Creazione di una nuova Activity senza salvataggio immediato
        Activity newActivity = new Activity(0, DataManager.CurrentTaskGroup.ID, repetitions, handDataList);
        // Ottieni l'ID corrente da ActivityDBHandler
        ActivityDBHandler activityDBHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<ActivityDBHandler>("ActivityDBHandler");

        bool saveSuccess = activityDBHandler.Save(newActivity);
        if (!saveSuccess)
        {
            UnityEngine.Debug.LogError("Failed to save the new activity in the database.");
        }

        int newActivityId = activityDBHandler.getId();
        UnityEngine.Debug.Log(newActivityId);

        // Imposta l'ID sull'Activity appena creata
        newActivity.Id = newActivityId;
        // Aggiungi la nuova Activity al TaskGroup corrente nel DataManager
        DataManager.CurrentTaskGroup.Activities.Add(newActivity);

    }

    public List<string> ProcessJsonFilesInDirectory(string directoryPath)
    {
        List<string> filePaths = new List<string>();
        string[] jsonFilePaths = Directory.GetFiles(directoryPath, "*.json");
        foreach (var filePath in jsonFilePaths)
        {
            filePaths.Add(filePath);
        }
        return filePaths;
    }

    // Metodo aggiornato
    public Person GetPersonFromDataManager()
    {
        // Restituisce direttamente l'oggetto Person da DataManager
        return DataManager.CurrentPerson;
    }

    public void CreateAndStoreTaskGroup(string filePath, int repetitions)
    { 

        string jsonData = File.ReadAllText(filePath);
        HandleJsonData(jsonData, repetitions); // Passa anche le ripetizioni
    }

}
