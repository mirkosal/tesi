using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Finger
{
    // Inizializza bones per evitare NullReferenceException
    public List<Bone> bones { get; set; } = new List<Bone>();
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
