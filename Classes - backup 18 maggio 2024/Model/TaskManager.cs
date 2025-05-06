using System.Collections.Generic;

public class TaskManager
{
    private static TaskManager instance;
    private List<Activity> tasks = new List<Activity>();
    private List<int> Repetition = new List<int>();

    // Costruttore privato per prevenire l'istanziazione esterna
    private TaskManager() { }

    // Metodo pubblico statico per accedere all'istanza
    public static TaskManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new TaskManager();
            }
            return instance;
        }
    }

    // Metodo per aggiungere un task alla lista
    public void AddTask(Activity task)
    {
        tasks.Add(task);
        Repetition.Add(0); // Aggiungi un valore di default per le ripetizioni
    }

    // Metodo per impostare il valore di Repetition in un dato indice
    public void SetRepetition(int index, int repetitionValue)
    {
        // Verifica se l'indice è valido per la lista tasks
        if (index < 0 || index >= tasks.Count)
        {
            // Puoi scegliere di gestire l'errore in un modo che si adatta meglio al tuo progetto
            throw new System.IndexOutOfRangeException("Indice non valido per la lista tasks.");
            return;
        }

        // Se l'indice è maggiore della lunghezza della lista Repetition, estendi la lista
        if (index >= Repetition.Count)
        {
            for (int i = Repetition.Count; i <= index; i++)
            {
                Repetition.Add(0); // Aggiungi valori di default fino a raggiungere l'indice richiesto
            }
        }

        // Imposta il valore di ripetizione
        Repetition[index] = repetitionValue;
    }

    public int GetTaskIndex(Activity task)
    {
        return tasks.IndexOf(task);
    }

    public int GetTaskCount()
    {
        return tasks.Count;
    }
}

