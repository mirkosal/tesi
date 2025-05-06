using UnityEngine;
using UnityEngine.UI;

public class RepetitionDialog : MonoBehaviour
{
    public GameObject dialogPanel;
    public InputField repetitionInputField;
    public Button confirmButton;
    private Activity currentTask; // Sostituito Exercise con Task

    void Start()
    {
        dialogPanel.SetActive(false); // Nascondi il pannello all'avvio
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    public void ShowDialog(Activity task)
    {
        currentTask = task;
        dialogPanel.SetActive(true);
    }

    void OnConfirmButtonClicked()
    {
        if (int.TryParse(repetitionInputField.text, out int repetitions))
        {
            // Trova l'indice del currentTask
            int taskIndex = TaskManager.Instance.GetTaskIndex(currentTask);

            // Ottieni il conteggio totale dei task dal TaskManager
            int totalTasks = TaskManager.Instance.GetTaskCount();

            if (taskIndex != -1)
            {
                // Aggiungi il currentTask se non è già presente
                if (taskIndex == totalTasks)
                {
                    TaskManager.Instance.AddTask(currentTask);
                    taskIndex = totalTasks; // Aggiorna l'indice poiché il task è stato appena aggiunto
                }

                // Imposta le ripetizioni per il task corrente
                TaskManager.Instance.SetRepetition(taskIndex, repetitions);
            }
            else
            {
                // Gestisci il caso in cui il task non è trovato
            }

            dialogPanel.SetActive(false);
        }
        else
        {
            // Gestisci l'input non valido
        }
    }
}
