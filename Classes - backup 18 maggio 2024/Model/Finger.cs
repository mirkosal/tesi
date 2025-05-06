using System;
using UnityEngine;

[Serializable]
public class Finger
{
    // Assicurati che bones sia accessibile per la deserializzazione
    public Bone[] bones { get; set; }
    public int Type { get; set; }
    public int Id { get; set; }
    public int HandId { get; set; }
    public Vector3 TipPosition { get; set; }
    public Vector3 Direction { get; set; }
    public float Width { get; set; }
    public float Length { get; set; }
    public bool IsExtended { get; set; }
    public double TimeVisible { get; set; }

    // Costruttore senza parametri
    public Finger()
    {
    }

    // Metodo ToString per visualizzare le informazioni dell'oggetto
    public override string ToString()
    {
        string bonesInfo = "";
        if (bones != null)
        {
            foreach (var bone in bones)
            {
                bonesInfo += bone.ToString() + "\n";
            }
        }

        return $"Type: {Type}, Id: {Id}, HandId: {HandId}, TipPosition: {TipPosition}, Direction: {Direction}, Width: {Width}, Length: {Length}, IsExtended: {IsExtended}, TimeVisible: {TimeVisible}, Bones: \n{bonesInfo}";
    }
}
