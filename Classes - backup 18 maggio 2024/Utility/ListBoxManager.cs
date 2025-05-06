using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ListBoxManager : MonoBehaviour
{
    public GameObject itemPrefab; // Prefab per gli elementi della lista
    public Transform contentPanel; // Pannello di contenuto del ScrollView

    private List<string> items = new List<string>(); // Lista di stringhe da visualizzare

    // Aggiungi un nuovo elemento alla lista
    public void AddItem(string item)
    {
        items.Add(item);
        GameObject newItem = Instantiate(itemPrefab, contentPanel);
        newItem.GetComponentInChildren<Text>().text = item;
        // Aggiungi qui ulteriore logica se necessario
    }

    // Aggiorna la visualizzazione della lista
    public void UpdateDisplay()
    {
        // Elimina vecchi elementi
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Aggiungi elementi attuali
        foreach (string item in items)
        {
            AddItem(item);
        }
    }
}