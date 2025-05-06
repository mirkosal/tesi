using System.Collections.Generic;
using System.Text;

public class TaskGroup
{
    public string Name { get;  set; }
    public List<Activity> Activities { get; set; }
    public int ID { get; set; }
    public int PersonID { get; set; }

    
    public TaskGroup(string name, int personId)
    {
        Name = name;
        PersonID = personId; 
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
        sb.AppendLine($"PersonID: {PersonID}");
        foreach (var activity in Activities)
        {
            sb.AppendLine(activity.ToString());
        }
        return sb.ToString().TrimEnd();
    }

    public bool HasValidData()
    {
        return Activities.Count != 0 && PersonID > 0;
    }
}
