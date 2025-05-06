using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using SimpleFileBrowser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro; // Aggiungi questo namespace per TextMesh Pr
using UnityEngine.SceneManagement; // Per il cambio di scena
using System.Linq;



public class CreateTaskView : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform scrollViewContent;
    public GameObject form;
    public TMP_InputField inpRepetition;
    public Transform scrollView;
    public Button selectDirectoryButton;
    public Button BtnSimulation; // Assicurati di assegnare questo dal tuo editor Unity
    private CreateTaskController createTaskController;
    public TextMeshProUGUI personNameText;
    public Button BtnConfirm; // Assicurati di assegnare questo bottone dall'editor di Unity


    public GameObject textMeshPrefab;
    private string currentFileName; // Aggiungi questa variabile per tenere traccia del nome del file corrente


    void Awake()
    {
        createTaskController = ControllerFactory.Instance.CreateController<CreateTaskController>("createTask");
    }

    void Start()
    { 
    
        selectDirectoryButton.onClick.AddListener(OpenFolderBrowser);
        BtnSimulation.onClick.AddListener(HandleSimulationButtonPress);
        UpdatePersonName();
    }

   
    public void DisplayActivityInScrollView(Activity activity)
    {
        GameObject textObj = Instantiate(textMeshPrefab, scrollView);
        TextMeshProUGUI textMesh = textObj.GetComponent<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textMesh.text = $"Activity ID: {activity.Id}, Repetitions: {activity.Repetitions}";
        }
        else
        {
            UnityEngine.Debug.LogError("Componente TextMeshProUGUI non trovato nel prefab.");
        }
    }

    void UpdateAndDisplayActivities(Action<Activity> displayMethod)
    {
        foreach (Transform child in scrollView)
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
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }
        foreach (string filePath in filePaths)
        {
            CreateButton(filePath);
        }
    }

    void CreateButton(string filePath)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, scrollViewContent);

        // Calcola il percorso relativo rispetto alla directory del progetto
        string relativePath = GetRelativePath(filePath, Application.dataPath);

        buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = relativePath;

        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => ProcessJsonFile(relativePath));
        }
    }

    // Metodo per calcolare il percorso relativo
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
        UnityEngine.Debug.Log("File name:" +fileName);
        if (DataManager.CurrentTaskGroup == null || DataManager.CurrentTaskGroup.Activities.Count == 0)
        {
            return;
        }

        foreach (var activity in DataManager.CurrentTaskGroup.Activities)
        {
            DisplayActivityInScrollView(fileName, activity.Repetitions);
        }
    }

    public void DisplayActivityInScrollView(string fileName, int repetitions)
    {
        GameObject textObj = Instantiate(textMeshPrefab, scrollView);
        TextMeshProUGUI textMesh = textObj.GetComponent<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textMesh.text = $"Activity File: {fileName}, Repetitions: {repetitions}";
        }
        else
        {
            UnityEngine.Debug.LogError("Componente TextMeshProUGUI non trovato nel prefab.");
        }
    }

    void AddTextToScrollView(string text)
    {
        if (textMeshPrefab == null)
        {
            UnityEngine.Debug.LogError("TextMesh Prefab non è stato assegnato.");
            return;
        }

        // Crea una nuova istanza di TextMesh Pro usando il prefab
        GameObject textObj = Instantiate(textMeshPrefab, scrollView);
        TextMeshProUGUI textMesh = textObj.GetComponent<TextMeshProUGUI>();

        if (textMesh != null)
        {
            textMesh.text = text;
            // Puoi impostare qui altre proprietà di TextMesh Pro, come la dimensione del font, ecc.
        }
        else
        {
            UnityEngine.Debug.LogError("Il componente TextMeshProUGUI non è stato trovato nel prefab.");
        }
    }

    private bool CheckActivityInstance()
    {
        // Implementa la logica di verifica
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