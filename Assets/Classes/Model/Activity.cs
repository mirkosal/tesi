using System.Collections.Generic;
using System.Text;

public class Activity
{
    public int Id { get; set; }
    public int TaskGroupID { get;  set; } // Aggiunto attributo TaskGroupID
    public int Repetitions { get; set; }
    public List<HandData> HandData { get; set; }

    // Aggiornato il costruttore per includere il parametro taskGroupId
    public Activity(int id, int taskGroupId, int repetitions, List<HandData> handData)
    {
        Id = id;
        TaskGroupID = taskGroupId; // Inizializzazione di TaskGroupID
        Repetitions = repetitions;
        HandData = handData;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"ID: {Id}");
        sb.AppendLine($"TaskGroupID: {TaskGroupID}"); // Aggiunto TaskGroupID nell'output
        sb.AppendLine($"Repetitions: {Repetitions}");
        sb.AppendLine("HandData:");
        foreach (var hand in HandData)
        {
            sb.AppendLine($"  {hand.ToString()}");
        }
        return sb.ToString().TrimEnd();
    }
}
