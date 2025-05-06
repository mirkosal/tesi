using System.Collections.Generic;
using System.Text;

[System.Serializable]
public class HandData
{
    public int DeviceID;
    public int Id;
    public long Timestamp;
    public float CurrentFramesPerSecond;
    public List<Hand> Hands;
    public int ActivityID;
    public string id0;
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"DeviceID: {DeviceID}");
        sb.AppendLine($"Id: {Id}");
        sb.AppendLine($"Timestamp: {Timestamp}");
        sb.AppendLine($"CurrentFramesPerSecond: {CurrentFramesPerSecond}");

        sb.AppendLine("Hands:");
        foreach (var hand in Hands)
        {
            sb.AppendLine($"  {hand.ToString()}");
        }

        return sb.ToString();
    }
}
