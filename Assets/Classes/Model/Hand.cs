using System;
using System.Collections.Generic;
using UnityEngine;
using MongoDB.Bson;

[Serializable]
public class Hand
{
    public int Id { get; set; } // Cambiamo questo in caso MongoDB.ObjectId fosse necessario
    public int FrameId { get; set; }
    public int DeviceID { get; set; }
    public ObjectId MongoObjectId { get; set; } // Rinominato per evitare conflitti
    public int HandDataID { get; set; } // Aggiunto il campo HandDataID
    public List<Finger> Fingers { get; set; } = new List<Finger>(); // Inizializza per evitare NullReferenceException
    public Vector3 PalmPosition { get; set; }
    public Vector3 PalmVelocity { get; set; }
    public Vector3 PalmNormal { get; set; }
    public Vector3 Direction { get; set; }
    public Quaternion Rotation { get; set; }
    public float GrabStrength { get; set; }
    public float PinchStrength { get; set; }
    public float PinchDistance { get; set; }
    public bool IsExtended { get; set; }
    public double TimeVisible { get; set; }

    public Hand() { }

    public override string ToString()
    {
        string fingersInfo = "";
        foreach (var finger in Fingers)
        {
            fingersInfo += finger.ToString() + "\n";
        }

        return $"FrameId: {FrameId}, Id: {Id}, HandDataID: {HandDataID}, Fingers: \n{fingersInfo}, PalmPosition: {PalmPosition}, PalmVelocity: {PalmVelocity}, PalmNormal: {PalmNormal}, Direction: {Direction}, Rotation: {Rotation}, GrabStrength: {GrabStrength}, PinchStrength: {PinchStrength}, PinchDistance: {PinchDistance}, IsExtended: {IsExtended}, TimeVisible: {TimeVisible}";
    }
}



















/*using System;
using System.Collections.Generic;
using UnityEngine;
using MongoDB.Bson; // Assicurati di includere questo using

[Serializable]
public class Hand
{
    public int Id { get; set; }
    public int FrameId { get; set; }
    public int DeviceID { get; set; }
    public ObjectId ID { get; set; }
    public int HandDataID { get; set; } // Aggiunto il campo HandDataID
    public List<Finger> Fingers { get; set; } = new List<Finger>(); // Inizializza per evitare NullReferenceException
    public Vector3 PalmPosition { get; set; }
    public Vector3 PalmVelocity { get; set; }
    public Vector3 PalmNormal { get; set; }
    public Vector3 Direction { get; set; }
    public Quaternion Rotation { get; set; }
    public float GrabStrength { get; set; }
    public float PinchStrength { get; set; }
    public float PinchDistance { get; set; }
    public bool IsExtended { get; set; }
    public double TimeVisible { get; set; }

    public Hand() { }

    public override string ToString()
    {
        string fingersInfo = "";
        foreach (var finger in Fingers)
        {
            fingersInfo += finger.ToString() + "\n";
        }

        return $"FrameId: {FrameId}, Id: {Id}, HandDataID: {HandDataID}, Fingers: \n{fingersInfo}, PalmPosition: {PalmPosition}, PalmVelocity: {PalmVelocity}, PalmNormal: {PalmNormal}, Direction: {Direction}, Rotation: {Rotation}, GrabStrength: {GrabStrength}, PinchStrength: {PinchStrength}, PinchDistance: {PinchDistance}, IsExtended: {IsExtended}, TimeVisible: {TimeVisible}";
    }
}
*/