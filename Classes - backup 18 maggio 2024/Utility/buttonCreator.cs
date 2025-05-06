using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ButtonCreator : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform contentPanel;

    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            AddButton("Pulsante " + (i + 1), i);
        }
    }

    void AddButton(string buttonText, int index)
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("Button prefab is not assigned in the Inspector!");
            return;
        }

        if (contentPanel == null)
        {
            Debug.LogError("Content panel transform is not assigned in the Inspector!");
            return;
        }

        GameObject newButton = Instantiate(buttonPrefab, contentPanel);
        if (newButton == null)
        {
            Debug.LogError("Failed to instantiate the button prefab.");
            return;
        }

        // Impostazione del testo del bottone usando il componente TextMeshProUGUI già presente nel prefab
        TextMeshProUGUI textMeshPro = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            textMeshPro.text = buttonText;
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found in the button prefab.");
        }

        // Aggiunta del listener per il clic sul pulsante
        Button buttonComponent = newButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(() => ButtonClicked(index));
        }
        else
        {
            Debug.LogError("Button component not found on the button prefab.");
        }
    }
    void ButtonClicked(int buttonIndex)
    {
        Debug.Log("Pulsante " + buttonIndex + " cliccato.");
    }
}
