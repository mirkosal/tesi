using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class OptionView : MonoBehaviour
{
    public Button addLoginButton;
    public Button deleteLoginButton;
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;
    public Transform loginContentPanel;
    public GameObject loginItemPrefab;
    public Button addPersonButton;
    public Button deletePersonButton;
    public TMP_InputField firstNameInputField;
    public TMP_InputField lastNameInputField;
    public TMP_InputField diagnosisInputField;
    public TMP_InputField dateOfBirthInputField;
    public TMP_InputField taxCodeInputField;
    public Transform personContentPanel;
    public GameObject personItemPrefab;
    public Button backButton; // Aggiungi questo riferimento
    private ListBoxManager loginListBoxManager;
    private ListBoxManager personListBoxManager;

    [SerializeField] private ToggleGroup languageToggleGroup; // ToggleGroup per la selezione della lingua

    private void Start()
    {
        // Inizializzazione per Login
        loginListBoxManager = new ListBoxManager(loginItemPrefab, loginContentPanel, loginItemPrefab.GetComponentInChildren<TextMeshProUGUI>());
        addLoginButton.onClick.AddListener(OnAddLoginClicked);
        deleteLoginButton.onClick.AddListener(OnDeleteLoginClicked);
        UpdateLoginList();

        // Inizializzazione per Person
        personListBoxManager = new ListBoxManager(personItemPrefab, personContentPanel, personItemPrefab.GetComponentInChildren<TextMeshProUGUI>());
        addPersonButton.onClick.AddListener(OnAddPersonClicked);
        deletePersonButton.onClick.AddListener(OnDeletePersonClicked);
        UpdatePersonList();

        // Inizializzazione per il pulsante "Back"
        backButton.onClick.AddListener(OnBackButtonClicked);

        // Aggiorna il ToggleGroup con la lingua corrente
        UpdateLanguageToggle();

        foreach (Toggle toggle in languageToggleGroup.GetComponentsInChildren<Toggle>())
        {
            toggle.onValueChanged.AddListener(delegate {
                OnLanguageToggleChanged();
            });
        }
    }

    private void UpdateLanguageToggle()
    {
        string currentLanguage = OptionController.Instance.GetCurrentLanguage();
        Toggle[] toggles = languageToggleGroup.GetComponentsInChildren<Toggle>();
        foreach (Toggle toggle in toggles)
        {
            if (toggle.name == currentLanguage)
            {
                toggle.isOn = true;
                break;
            }
        }
    }

    private string GetSelectedLanguage()
    {
        Toggle[] toggles = languageToggleGroup.GetComponentsInChildren<Toggle>();
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn)
            {
                switch (i)
                {
                    case 0:
                        return "English";
                    case 1:
                        return "Italian";
                    default:
                        return "English"; // Lingua predefinita
                }
            }
        }
        return "English"; // Lingua predefinita
    }

    // Metodi esistenti per Login
    private void OnAddLoginClicked()
    {
        OptionController.Instance.AddLogin(usernameInputField.text, passwordInputField.text);
        UpdateLoginList();
    }

    private void OnDeleteLoginClicked()
    {
        string selectedLogin = loginListBoxManager.GetCurrentSelectedItem();
        if (selectedLogin != null)
        {
            OptionController.Instance.DeleteLogin(selectedLogin);
            UpdateLoginList();
        }
    }

    private void OnLanguageToggleChanged()
    {
        // Ottieni la lingua selezionata
        string selectedLanguage = GetSelectedLanguage();

        // Imposta la lingua nel controller
        OptionController.Instance.SetLanguage(selectedLanguage);
    }

    private void UpdateLoginList()
    {
        loginListBoxManager.ClearItems();
        List<string> logins = OptionController.Instance.GetAllLogins();
        foreach (var login in logins)
        {
            loginListBoxManager.AddItem(login);
        }
    }

    // Nuovi metodi per Person
    private void OnAddPersonClicked()
    {
        Person newPerson = new Person
        {
            FirstName = firstNameInputField.text,
            LastName = lastNameInputField.text,
            Diagnosis = diagnosisInputField.text,
            DateOfBirth = DateTime.Parse(dateOfBirthInputField.text),
            TaxCode = taxCodeInputField.text
        };
        OptionController.Instance.AddPerson(newPerson);
        UpdatePersonList();
    }

    private void OnDeletePersonClicked()
    {
        string selectedPerson = personListBoxManager.GetCurrentSelectedItem();
        if (selectedPerson != null)
        {
            OptionController.Instance.DeletePersonByName(selectedPerson);
            UpdatePersonList();
        }
    }

    private void UpdatePersonList()
    {
        personListBoxManager.ClearItems();
        List<Person> persons = OptionController.Instance.GetAllPersons();
        foreach (var person in persons)
        {
            personListBoxManager.AddItem($"{person.FirstName} {person.LastName}");
        }
    }

    private void OnBackButtonClicked()
    {
        SceneManager.LoadScene("menuPaziente");
    }
}
