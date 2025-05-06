using System.Collections.Generic;
using System.Linq;

public static class DataManager
{
    public static TaskGroup CurrentTaskGroup { get; set; }
    public static List<Activity> Activities { get; set; } = new List<Activity>();
    public static List<HandData> AllHandData { get; set; } = new List<HandData>();
    public static List<Hand> AllHands { get; set; } = new List<Hand>();
    public static List<Finger> AllFingers { get; set; } = new List<Finger>();
    public static List<Bone> AllBones { get; set; } = new List<Bone>();
    public static Person CurrentPerson { get; set; }

    public static void LoadTaskGroup(TaskGroup taskGroup)
    {
        CurrentTaskGroup = taskGroup;

        // Pre-carica tutte le Activities collegate a CurrentTaskGroup
        Activities = taskGroup.Activities;

        // Pre-carica tutti i HandData associati alle Activities
        AllHandData = new List<HandData>();
        foreach (var activity in Activities)
        {
            // Qui si assume che i dati di HandData siano già stati collegati ad ogni Activity
            AllHandData.AddRange(activity.HandData);
        }

        // A questo punto, abbiamo raccolto tutte le Activities e tutti i HandData
        // Ora aggregiamo gli Hands, Fingers, e Bones dai HandData
        AllHands = AllHandData.SelectMany(hd => hd.Hands).ToList();
        AllFingers = AllHands.SelectMany(hand => hand.Fingers).ToList();
        AllBones = AllFingers.SelectMany(finger => finger.bones).ToList();
    }

    public static string ToString()
    {
        if (CurrentTaskGroup == null)
        {
            return "Nessun TaskGroup caricato.";
        }

        var stringBuilder = new System.Text.StringBuilder();
        stringBuilder.AppendLine($"TaskGroup: Nome: {CurrentTaskGroup.Name}, ID: {CurrentTaskGroup.ID}, PersonID: {CurrentTaskGroup.PersonID}");

        if (CurrentPerson != null)
        {
            stringBuilder.AppendLine($"Persona: {CurrentPerson.ToString()}");
        }

        foreach (var activity in CurrentTaskGroup.Activities)
        {
            stringBuilder.AppendLine($"  Activity: {activity.ToString()}");
            foreach (var handData in AllHandData.Where(hd => hd.ActivityID == activity.Id))
            {
                stringBuilder.AppendLine($"    HandData: {handData.ToString()}");
                foreach (var hand in handData.Hands)
                {
                    stringBuilder.AppendLine($"      Hand: {hand.ToString()}");
                    foreach (var finger in hand.Fingers)
                    {
                        stringBuilder.AppendLine($"        Finger: {finger.ToString()}");
                        foreach (var bone in finger.bones)
                        {
                            stringBuilder.AppendLine($"          Bone: {bone.ToString()}");
                        }
                    }
                }
            }
        }

        return stringBuilder.ToString();
    }

    // Metodo per impostare CurrentPerson
    public static void SetCurrentPerson(Person person)
    {
        CurrentPerson = person;
    }

    // Metodo per resettare il valore di CurrentPerson
    public static void ResetCurrentPerson()
    {
        CurrentPerson = null;
    }

}
