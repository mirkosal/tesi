using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;


public class PersonView : MonoBehaviour
{
    [SerializeField] private GameObject formObject; // Riferimento all'oggetto form
    [SerializeField] private Button confirmButton; // Pulsante per confermare la selezione della persona
    [SerializeField] private TMP_InputField myInputField;
    [SerializeField] private ToggleGroup myToggleGroup;
    [SerializeField] private GameObject personButtonPrefab; // Prefab del pulsante TextMeshPro
    [SerializeField] private Transform scrollViewContent; // Contenitore della ScrollView per le persone
    [SerializeField] private Transform scvResults; // Contenitore della ScrollView per i TaskGroups
    [SerializeField] private Button createNewTaskGroup;
    [SerializeField] private GameObject listItemPrefab; // Riferimento al prefab degli elementi della lista
    [SerializeField] private TextMeshProUGUI listItemTextPrefab; // Riferimento al prefab del testo
    [SerializeField] private Button renameTaskButton; // Pulsante per rinominare il TaskGroup
    [SerializeField] private Button deleteTaskButton; // Pulsante per cancellare il TaskGroup
    [SerializeField] private GameObject renameFormObject; // Form per la rinomina del TaskGroup
    [SerializeField] private TMP_InputField renameInputField; // InputField per il nuovo nome
    [SerializeField] private Button confirmRenameButton; // Pulsante per confermare la rinomina
    [SerializeField] private Button SimulationButton;
    [SerializeField] private Button referenceButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private PersonInfoDisplayer personInfoDisplayer; // Aggiungi questo riferimento
    private Person currentSelectedPerson; // Persona selezionata corrente
    private TaskGroup currentSelectedTaskGroup; // TaskGroup selezionato corrente
    private ListBoxManager personListBoxManager; // Riferimento al ListBoxManager per le persone
    private ListBoxManager taskGroupListBoxManager; // Riferimento al ListBoxManager per i TaskGroups

    void Start()
    {
        if (personButtonPrefab == null || scrollViewContent == null || scvResults == null || listItemPrefab == null || listItemTextPrefab == null)
        {
            Debug.LogError("PersonView: Uno o più riferimenti non sono assegnati nell'Inspector!");
            return;
        }

        createNewTaskGroup.onClick.AddListener(ConfirmSelection); // Ascoltatore per il pulsante di conferma
        var Handler = DatabaseManager.Instance.GetDatabaseObjectInstance<ActivityDBHandler>("ActivityDBHandler");
        if (renameTaskButton == null || deleteTaskButton == null || renameFormObject == null || renameInputField == null || confirmRenameButton == null)
        {
            Debug.LogError("PersonView: Uno o più riferimenti non sono assegnati nell'Inspector!");
            return;
        }
        // Inizializza il ButtonPropertyManager e aggiorna tutti i bottoni
        ButtonPropertyManager buttonPropertyManager = gameObject.AddComponent<ButtonPropertyManager>();
        buttonPropertyManager.referenceButton = referenceButton;
        buttonPropertyManager.UpdateAllButtons();
        // Aggiungi listener al pulsante di rinomina
        renameTaskButton.onClick.AddListener(ToggleRenameForm);
        confirmRenameButton.onClick.AddListener(HandleRenameTaskGroup);
        deleteTaskButton.onClick.AddListener(HandleDeleteTaskGroup); // Aggiungi listener al pulsante di cancellazione
        SimulationButton.onClick.AddListener(HandleSimulationButtonPress);
        optionButton.onClick.AddListener(HandleOptionButtonPress); // Aggiungi listener per il pulsante di opzioni
        // Nascondi il form di rinomina all'avvio
        renameFormObject.SetActive(false);
        // Nascondi il formObject all'avvio
        formObject.SetActive(false);
        // Disabilita i pulsanti di rinomina e cancellazione all'avvio
        renameTaskButton.interactable = false;
        deleteTaskButton.interactable = false;
        SimulationButton.interactable = false;
        // Inizializzazione dei ListBoxManager
        personListBoxManager = new ListBoxManager(personButtonPrefab, scrollViewContent, listItemTextPrefab);
        taskGroupListBoxManager = new ListBoxManager(listItemPrefab, scvResults, listItemTextPrefab);
    }

    private void ToggleRenameForm()
    {
        renameFormObject.SetActive(!renameFormObject.activeSelf);
    }

    private void HandleRenameTaskGroup()
    {
        if (currentSelectedPerson != null && currentSelectedTaskGroup != null)
        {
            string newName = renameInputField.text;
            if (!string.IsNullOrWhiteSpace(newName))
            {
                // Aggiorna il nome del TaskGroup
                currentSelectedTaskGroup.Name = newName;

                // Salva il cambiamento nel database
                TaskGroupDBHandler taskGroupDbHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<TaskGroupDBHandler>("TaskGroupDBHandler");
                if (taskGroupDbHandler != null)
                {
                    bool success = taskGroupDbHandler.Edit(currentSelectedTaskGroup.ID, currentSelectedTaskGroup);
                    if (success)
                    {
                        Debug.Log("TaskGroup rinominato con successo.");
                    }
                    else
                    {
                        Debug.LogError("Errore durante la rinomina del TaskGroup.");
                    }
                }

                // Nascondi il form di rinomina dopo il salvataggio
                renameFormObject.SetActive(false);
            }
        }
    }

    private void HandleDeleteTaskGroup()
    {
        if (currentSelectedTaskGroup != null)
        {
            // Elimina il TaskGroup dal database
            TaskGroupDBHandler taskGroupDbHandler = DatabaseManager.Instance.GetDatabaseObjectInstance<TaskGroupDBHandler>("TaskGroupDBHandler");
            if (taskGroupDbHandler != null)
            {
                bool success = taskGroupDbHandler.Delete(currentSelectedTaskGroup.ID);
                if (success)
                {
                    Debug.Log("TaskGroup eliminato con successo.");
                    // Aggiorna la vista rimuovendo il task eliminato
                    taskGroupListBoxManager.RemoveItem(currentSelectedTaskGroup.Name);
                    currentSelectedTaskGroup = null;
                    // Disabilita i pulsanti di rinomina e cancellazione
                    renameTaskButton.interactable = false;
                    deleteTaskButton.interactable = false;
                }
                else
                {
                    Debug.LogError("Errore durante l'eliminazione del TaskGroup.");
                }
            }
        }
    }

    private void ConfirmSelection()
    {
        DataManager.SetCurrentPerson(currentSelectedPerson);
        PersonController personController = ControllerFactory.Instance.CreateController<PersonController>("person");
        personController.HandleConfirmButton();
        SceneManager.LoadScene("creareTask");
    }

    public void ReceivePersonInput()
    {
        string inputText = myInputField.text;
        string selectedToggleLabel = GetSelectedToggleLabel();

        // Crea un'istanza del PersonController
        PersonController personController = ControllerFactory.Instance.CreateController<PersonController>("person");

        // Otteni i risultati dalla ricerca, includendo TaskGroups
        var searchResults = personController.SearchForPersonsAndTaskGroups(selectedToggleLabel, inputText);

        // Aggiorna la ScrollView con i risultati
        UpdateScrollView(searchResults);
    }

    private string GetSelectedToggleLabel()
    {
        int index = 0;
        foreach (Toggle toggle in myToggleGroup.GetComponentsInChildren<Toggle>())
        {
            if (toggle.isOn)
            {
                switch (index)
                {
                    case 0:
                        return "FirstName";
                    case 1:
                        return "LastName";
                    case 2:
                        return "DateOfBirth";
                    default:
                        return null;
                }
            }
            index++;
        }
        return null;
    }


    private void ShowForm(Person person, List<TaskGroup> taskGroups)
    {
        currentSelectedPerson = person; // Salva la persona selezionata
        formObject.SetActive(true); // Attiva il form
        personInfoDisplayer.DisplayPersonInfo(person); // Visualizza le informazioni della persona

        // Aggiorna la ListBox dei TaskGroups con i nuovi dati
        taskGroupListBoxManager.ClearItems();
        foreach (TaskGroup taskGroup in taskGroups)
        {
            GameObject newButton = taskGroupListBoxManager.AddItem(taskGroup.Name);
            if (newButton != null)
            {
                Button buttonComponent = newButton.GetComponent<Button>();
                if (buttonComponent != null)
                {
                    buttonComponent.onClick.AddListener(() => OnTaskGroupButtonClicked(newButton, taskGroup));
                }
                else
                {
                    Debug.LogError("PersonView: Button component non trovato nel nuovo item!");
                }
            }
            else
            {
                Debug.LogError("PersonView: newButton è null!");
            }
        }
    }

    private void UpdateScrollView(List<(Person, List<TaskGroup>)> searchResults)
    {
        // Aggiorna la ListBox delle persone con i nuovi risultati
        personListBoxManager.ClearItems();
        foreach (var result in searchResults)
        {
            Person person = result.Item1;
            string personName = person.FirstName + " " + person.LastName;
            GameObject newButton = personListBoxManager.AddItem(personName);
            if (newButton != null)
            {
                Button buttonComponent = newButton.GetComponent<Button>();
                if (buttonComponent != null)
                {
                    buttonComponent.onClick.AddListener(() => ShowForm(person, result.Item2)); // Aggiungi il listener per mostrare il form
                }
                else
                {
                    Debug.LogError("PersonView: Button component non trovato nel nuovo item!");
                }
            }
            else
            {
                Debug.LogError("PersonView: newButton è null!");
            }
        }
    }

    public void DisplayTaskGroup(TaskGroup taskGroup)
    {
        // Aggiungi logica per aggiornare la vista con i dettagli del TaskGroup, se necessario
        Debug.Log("TaskGroup caricato: " + taskGroup.Name);
        // Aggiungi codice per aggiornare la UI con i dettagli del TaskGroup
    }

    private void OnTaskGroupButtonClicked(GameObject selectedItem, TaskGroup taskGroup)
    {
        // Deseleziona l'elemento precedente (il ListBoxManager gestisce la selezione ora)
        taskGroupListBoxManager.OnItemClicked(selectedItem);
        currentSelectedTaskGroup = taskGroup; // Salva il task selezionato
        PersonController personController = ControllerFactory.Instance.CreateController<PersonController>("person");
        // Chiamata al controller per caricare il TaskGroup selezionato
        personController.LoadAndDisplayTaskGroup(taskGroup);
        // Abilita i pulsanti di rinomina e cancellazione
        renameTaskButton.interactable = true;
        deleteTaskButton.interactable = true;
        SimulationButton.interactable = true;
    }

    private void HandleSimulationButtonPress()
    {
        if (DataManager.CurrentTaskGroup != null)
        {
            SceneManager.LoadScene("SimulationScene");
            string StringCurrentTaskGroup = DataManager.CurrentTaskGroup.ToString();
            UnityEngine.Debug.Log(StringCurrentTaskGroup);
        }
        else
        {
            UnityEngine.Debug.Log("L'istanza di taskgroup non esiste.");
        }
    }

    private void HandleOptionButtonPress()
    {
        SceneManager.LoadScene("OptionScene");
    }
}
