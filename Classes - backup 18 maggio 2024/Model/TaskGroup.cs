using System.Collections.Generic;
using System.Text;

public class TaskGroup
{
    public string Name { get; private set; }
    public List<Activity> Activities { get; set; }
    public int ID { get; set; }
    public int PersonID { get; private set; } // Aggiunto attributo PersonID

    // Modificato il costruttore per accettare un parametro personId
    public TaskGroup(string name, int personId)
    {
        Name = name;
        PersonID = personId; // Inizializzazione di PersonID
        Activities = new List<Activity>();
    }

    public void AddActivity(Activity activity)
    {
        Activities.Add(activity);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"TaskGroup: {Name}");
        sb.AppendLine($"PersonID: {PersonID}"); // Inclusione di PersonID nel metodo ToString
        foreach (var activity in Activities)
        {
            sb.AppendLine(activity.ToString());
        }
        return sb.ToString().TrimEnd();
    }

    public bool HasValidData()
    {
        // Controllo se ci sono attività nella lista e se l'ID della persona è valido
        return Activities.Count != 0 && PersonID > 0;
    }
}
