using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using SimpleFileBrowser;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class CreateTaskView : MonoBehaviour
{
    public GameObject buttonPrefab; // Prefab per i bottoni
    public GameObject textMeshPrefab; // Prefab per il testo
    public Transform scrollView1; // ScrollView per i bottoni
    public Transform scrollView2; // ScrollView per i testi
    public GameObject form;
    public TMP_InputField inpRepetition;
    public Button selectDirectoryButton;
    public Button BtnSimulation;
    private CreateTaskController createTaskController;
    public TextMeshProUGUI personNameText;
    public Button BtnConfirm;
    public Button BtnSave;
    private string currentFileName;
    private ListBoxManager buttonListBoxManager; // Gestione dei bottoni
    private ListBoxManager textListBoxManager; // Gestione dei testi

    void Awake()
    {
        createTaskController = ControllerFactory.Instance.CreateController<CreateTaskController>("createTask");
    }

    void Start()
    {
        selectDirectoryButton.onClick.AddListener(OpenFolderBrowser);
        BtnSimulation.onClick.AddListener(HandleSimulationButtonPress);
        BtnSave.onClick.AddListener(HandleSaveButtonPress);
        UpdatePersonName();

        // Inizializzare ListBoxManager per bottoni e testi
        buttonListBoxManager = new ListBoxManager(buttonPrefab, scrollView1, textMeshPrefab.GetComponent<TextMeshProUGUI>());
        textListBoxManager = new ListBoxManager(textMeshPrefab, scrollView2, textMeshPrefab.GetComponent<TextMeshProUGUI>());
    }

    void HandleSaveButtonPress()
    {
        if (DataManager.CurrentTaskGroup != null)
        {
            UnityEngine.Debug.Log(DataManager.CurrentTaskGroup);
            createTaskController.FillTaskGroupData(DataManager.CurrentTaskGroup); // Metodo per riempire i dati
            createTaskController.UpdateTaskGroupNameWithCurrentDateTime();
            StartCoroutine(createTaskController.SaveTaskGroupCoroutine(DataManager.CurrentTaskGroup, (success) =>
            {
                if (success)
                {
                    UnityEngine.Debug.Log("TaskGroup salvato con successo nel database.");
                }
                else
                {
                    UnityEngine.Debug.LogError("Errore nel salvataggio del TaskGroup nel database.");
                }
            }));
        }
        else
        {
            UnityEngine.Debug.LogError("Nessun TaskGroup corrente disponibile per il salvataggio.");
        }
    }

   

    void UpdateAndDisplayActivities(Action<Activity> displayMethod)
    {
        foreach (Transform child in scrollView2)
        {
            Destroy(child.gameObject);
        }

        foreach (var activity in DataManager.CurrentTaskGroup.Activities)
        {
            displayMethod(activity);
        }
    }

    void OpenFolderBrowser()
    {
        StartCoroutine(ShowFolderBrowser());
    }

    IEnumerator ShowFolderBrowser()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, null, null, "Seleziona Cartella", "Seleziona");
        if (FileBrowser.Success)
        {
            string folderPath = FileBrowser.Result[0];
            List<string> filePaths = createTaskController.ProcessJsonFilesInDirectory(folderPath);
            CreateButtonsWithFileNames(filePaths);
        }
    }

    public void CreateButtonsWithFileNames(List<string> filePaths)
    {
        buttonListBoxManager.ClearItems();
        foreach (string filePath in filePaths)
        {
            CreateButton(filePath);
        }
    }

    void CreateButton(string filePath)
    {
        // Estrai il nome del file senza estensione dal percorso
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

        GameObject buttonObj = buttonListBoxManager.AddItem(fileNameWithoutExtension);
        if (buttonObj != null)
        {
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => ProcessJsonFile(filePath));
            }
        }
    }

    string GetRelativePath(string filesPath, string referencePath)
    {
        var fileUri = new Uri(filesPath);
        var referenceUri = new Uri(referencePath);
        return Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).ToString().Replace('/', Path.DirectorySeparatorChar));
    }

    void ProcessJsonFile(string filePath)
    {
        UnityEngine.Debug.Log(filePath);
        int repetitions = 1; // Assumi un valore predefinito o modifica questa logica come necessario
        createTaskController.HandleJsonData(filePath, repetitions);
        UpdateScrollView(Path.GetFileNameWithoutExtension(filePath));
    }

    void OpenFormAndHandleInput(string filePath)
    {
        form.SetActive(true); // Attiva il Form
        inpRepetition.text = ""; // Resetta l'InputField per le ripetizioni
        inpRepetition.onEndEdit.RemoveAllListeners();
        inpRepetition.onEndEdit.AddListener((input) => HandleRepetitionInput(input, filePath)); // Imposta il listener per gestire l'input delle ripetizioni
    }

    void HandleRepetitionInput(string input, string filePath)
    {
        if (int.TryParse(input, out int repetitions) && repetitions > 0)
        {
            form.SetActive(false); // Nasconde il Form dopo la conferma dell'input

            // Lettura dei dati JSON dal file
            string jsonData = File.ReadAllText(filePath);
            currentFileName = Path.GetFileNameWithoutExtension(filePath); // Aggiorna currentFileName quando l'input viene confermato

            // Processa i dati con il numero di ripetizioni appropriato
            createTaskController.HandleJsonData(jsonData, repetitions); // Nota: ora non passiamo il nome del file

            // Aggiorna la ScrollView dopo l'aggiunta dell'activity, passando anche il nome del file
            UpdateScrollView(currentFileName); // Passa il nome del file aggiornato
        }
        else
        {
            UnityEngine.Debug.LogError("Inserisci un numero valido di ripetizioni.");
        }
    }

    void UpdateScrollView(string fileName)
    {
        if (DataManager.CurrentTaskGroup == null || DataManager.CurrentTaskGroup.Activities.Count == 0)
        {
            return;
        }

        
        DisplayActivityInScrollView(fileName);
        
    }

    public void DisplayActivityInScrollView(string fileName)
    {
        string displayText = $"Activity File: {fileName}";
        textListBoxManager.AddItem(displayText);
    }

    private bool CheckActivityInstance()
    {
        return DataManager.CurrentTaskGroup != null && DataManager.CurrentTaskGroup.Activities.Count > 0;
    }

    void HandleSimulationButtonPress()
    {
        if (CheckActivityInstance())
        {
            SceneManager.LoadScene("SimulationScene");
            string StringCurrentTaskGroup = DataManager.CurrentTaskGroup.ToString();
            UnityEngine.Debug.Log(StringCurrentTaskGroup);
        }
        else
        {
            UnityEngine.Debug.Log("L'istanza di Activity non esiste.");
        }
    }

    void AddActivityAndUpdateUI(string jsonData, int repetitions)
    {
        createTaskController.HandleJsonData(jsonData, repetitions); // Gestione dei dati JSON e aggiunta dell'attivi
    }

    void UpdatePersonName()
    {
        Person currentPerson = DataManager.CurrentPerson;
        if (currentPerson != null && personNameText != null)
        {
            personNameText.text = currentPerson.TaxCode;
        }
    }
}
