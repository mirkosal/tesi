using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Collections;
using System.IO;
using System.Linq;

public class FileSelectorUI : MonoBehaviour
{
    public Button selectFolderButton;

    private void Start()
    {
        // Configura il file browser
        FileBrowser.SetFilters(true, new FileBrowser.Filter("JSON Files", ".json"));
        FileBrowser.SetDefaultFilter(".json");

        // Aggiungi un listener al bottone
        selectFolderButton.onClick.AddListener(OpenFolderBrowser);
    }

    private void OpenFolderBrowser()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    private IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, null, null, "Seleziona Cartella", "Seleziona");

        if (FileBrowser.Success)
        {
            // Ottieni il percorso della cartella selezionata
            string folderPath = FileBrowser.Result[0];

            // Cerca tutti i file JSON nella cartella
            string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

            foreach (string file in jsonFiles)
            {
                // Usa la classe JsonDataExtractor per caricare i dati da ogni file JSON
                HandData data = JsonDataExtractor.LoadHandData(file);

                // Gestisci i dati come necessario
                // Ad esempio, puoi aggiornare l'interfaccia utente o inviare i dati a un'altra parte del tuo gioco
            }
        }
    }
}
