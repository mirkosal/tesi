using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;


public class ListBoxManager
{
    private GameObject itemPrefab; // Prefab per gli elementi della lista
    private Transform contentPanel; // Pannello di contenuto del ScrollView
    private TextMeshProUGUI textPrefab; // Prefab del testo
    private List<string> items; // Lista di stringhe da visualizzare
    private GameObject currentSelectedItem; // Elemento selezionato corrente

    public ListBoxManager(GameObject prefab, Transform panel, TextMeshProUGUI textPrefab)
    {
        itemPrefab = prefab;
        contentPanel = panel;
        this.textPrefab = textPrefab;
        items = new List<string>();
    }

    // Aggiungi un nuovo elemento alla lista e restituisci l'oggetto creato
    public GameObject AddItem(string item)
    {
        if (itemPrefab == null || contentPanel == null || textPrefab == null)
        {
            Debug.LogError("ListBoxManager: itemPrefab, contentPanel o textPrefab non sono inizializzati!");
            return null;
        }

        items.Add(item);
        GameObject newItem = GameObject.Instantiate(itemPrefab, contentPanel);
        if (newItem == null)
        {
            Debug.LogError("ListBoxManager: Fallimento nell'instanziazione del nuovo item!");
            return null;
        }

        TextMeshProUGUI textComponent = newItem.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = item;
        }
        else
        {
            Debug.LogError("ListBoxManager: TextMeshProUGUI component non trovato nel nuovo item!");
        }

        Button buttonComponent = newItem.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(() => OnItemClicked(newItem));
        }

        return newItem;
    }

    // Metodo chiamato quando un elemento viene cliccato
    public void OnItemClicked(GameObject selectedItem)
    {
        if (currentSelectedItem != null)
        {
            DeselectItem(currentSelectedItem);
        }

        currentSelectedItem = selectedItem;
        SelectItem(currentSelectedItem);
    }

    // Seleziona un elemento
    private void SelectItem(GameObject item)
    {
        var textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.color = Color.gray; // Cambia il colore del testo per indicare la selezione
        }
        else
        {
            Debug.LogError("ListBoxManager: TextMeshProUGUI component non trovato nell'elemento selezionato!");
        }
    }

    // Deseleziona un elemento
    private void DeselectItem(GameObject item)
    {
        var textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.color = Color.black; // Ripristina il colore originale del testo
        }
        else
        {
            Debug.LogError("ListBoxManager: TextMeshProUGUI component non trovato nell'elemento deselezionato!");
        }
    }

    // Rimuove un elemento dalla lista
    public void RemoveItem(string item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);

            foreach (Transform child in contentPanel)
            {
                TextMeshProUGUI textComponent = child.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null && textComponent.text == item)
                {
                    GameObject.Destroy(child.gameObject);
                    break;
                }
            }

            // Deseleziona se l'elemento rimosso è quello selezionato
            if (currentSelectedItem != null)
            {
                TextMeshProUGUI selectedTextComponent = currentSelectedItem.GetComponentInChildren<TextMeshProUGUI>();
                if (selectedTextComponent != null && selectedTextComponent.text == item)
                {
                    currentSelectedItem = null;
                }
            }
        }
        else
        {
            Debug.LogError("ListBoxManager: l'elemento da rimuovere non è presente nella lista!");
        }
    }

    // Pulisci la lista degli elementi
    public void ClearItems()
    {
        items.Clear();
        foreach (Transform child in contentPanel)
        {
            GameObject.Destroy(child.gameObject);
        }
        currentSelectedItem = null;
    }

    // Aggiorna la visualizzazione della lista
    public void UpdateDisplay()
    {
        ClearItems();
        foreach (string item in items)
        {
            AddItem(item);
        }
    }

    // Ottieni l'elemento selezionato corrente
    public string GetCurrentSelectedItem()
    {
        if (currentSelectedItem != null)
        {
            var textComponent = currentSelectedItem.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                return textComponent.text;
            }
        }
        return null;
    }
}
