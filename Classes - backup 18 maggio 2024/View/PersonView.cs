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
    [SerializeField] private GameObject scrollViewContent; // Contenitore della ScrollView per le persone
    [SerializeField] private GameObject scvResults; // Contenitore della ScrollView per i TaskGroups
    [SerializeField] private TextMeshProUGUI personInfoText; // Riferimento al TextMeshProUGUI per visualizzare le informazioni
    [SerializeField] private Button createNewTaskGroup; 

    private Person currentSelectedPerson; // Persona selezionata corrente

    void Start()
    {
        createNewTaskGroup.onClick.AddListener(ConfirmSelection); // Ascoltatore per il pulsante di conferma
        var Handler = DatabaseManager.Instance.GetDatabaseObjectInstance<ActivityDBHandler>("ActivityDBHandler");
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //
        //UnityEngine.Debug.Log(Handler.getId());
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
        foreach (Toggle toggle in myToggleGroup.ActiveToggles())
        {
            if (toggle.isOn)
            {
                var label = toggle.GetComponentInChildren<UnityEngine.UI.Text>();
                if (label != null)
                {
                    switch (label.text)
                    {
                        case "Name":
                            return "FirstName";
                        case "Last Name":
                            return "LastName";
                        case "Year":
                            return "DateOfBirth";
                        default:
                            return null;
                    }
                }
            }
        }
        return null;
    }

    private void ShowForm(Person person, List<TaskGroup> taskGroups)
    {
        currentSelectedPerson = person; // Salva la persona selezionata
        formObject.SetActive(true);
        personInfoText.text = person.ToString();

        // Pulisci e popola la ScrollView con i TaskGroups
        foreach (Transform child in scvResults.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (TaskGroup taskGroup in taskGroups)
        {
            GameObject newButton = Instantiate(personButtonPrefab, scvResults.transform);
            TextMeshProUGUI textComponent = newButton.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = taskGroup.Name;
            // Qui potresti aggiungere listener se necessario
        }
    }

    private void UpdateScrollView(List<(Person, List<TaskGroup>)> searchResults)
    {
        // Pulisci il contenuto esistente della ScrollView per le persone
        foreach (Transform child in scrollViewContent.transform)
        {
            Destroy(child.gameObject);
        }

        // Popola la ScrollView con i nuovi risultati di persone
        foreach (var result in searchResults)
        {
            Person person = result.Item1;
            List<TaskGroup> taskGroups = result.Item2;

            GameObject newButton = Instantiate(personButtonPrefab, scrollViewContent.transform);
            TextMeshProUGUI textComponent = newButton.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = person.FirstName + " " + person.LastName;
            newButton.GetComponent<Button>().onClick.AddListener(() => ShowForm(person, taskGroups));
        }
    }
}
