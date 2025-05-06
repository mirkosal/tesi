using UnityEngine;
using System.Collections.Generic;
using System; // Per definire eventi

public class HandController : MonoBehaviour
{
    public TaskGroup taskGroup; // Riferimento al Model
    public HandView handView; // Riferimento alla View

    // Definizione dell'evento
    public event Action<List<Finger>> OnFingerDataChanged;

    void Start()
    {
        // Sottoscrizione della View all'evento
        OnFingerDataChanged += handView.OnFingerDataUpdated;
    }

    void Update()
    {
        // Controlla se il tasto "Space" viene premuto
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Simula l'aggiornamento dei dati delle dita e lancia l'evento
            SimulateFingerDataUpdate();
        }
    }

    private void SimulateFingerDataUpdate()
    {
        // Qui dovresti recuperare o generare i nuovi dati delle dita
        // Per ora, simuleremo con dati fittizi o un metodo che li genera
        List<Finger> updatedFingers = GetUpdatedFingersData(); // Metodo fittizio, da implementare

        // Lancia l'evento con i nuovi dati delle dita
        OnFingerDataChanged?.Invoke(updatedFingers);
    }

    private List<Finger> GetUpdatedFingersData()
    {
        // Genera o recupera i nuovi dati delle dita
        // Questo è un placeholder, dovresti implementare la logica specifica qui
        return new List<Finger>();
    }

    void OnDestroy()
    {
        // Pulizia dell'evento per evitare memory leaks
        OnFingerDataChanged -= handView.OnFingerDataUpdated;
    }
}
