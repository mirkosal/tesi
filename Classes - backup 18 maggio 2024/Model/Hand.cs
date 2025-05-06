using System;
using System.Collections.Generic;
using UnityEngine;
using MongoDB.Bson; // Assicurati di includere questo using


[Serializable]
public class Hand
{
    public int  Id { get; set; }
    public int FrameId { get;  set; }
    public int DeviceID { get;  set; }
    public ObjectId ID { get;  set; }
    public List<Finger> Fingers { get;  set; }
    public Vector3 PalmPosition { get;  set; }
    public Vector3 PalmVelocity { get;  set; }
    public Vector3 PalmNormal { get;  set; }
    public   Vector3 Direction { get;  set; }
    public Quaternion Rotation { get;  set; }
    public float GrabStrength { get;  set; }
    public float PinchStrength { get;  set; }
    public float PinchDistance { get;  set; }
    public bool IsExtended { get;  set; }
    public double TimeVisible { get;  set; }

    public Hand() { }

    /*
    public Hand(int frameId, string Id, List<Finger> fingers, Vector3 palmPosition, Vector3 palmVelocity, Vector3 palmNormal, Vector3 direction, Quaternion rotation, float grabStrength, float pinchStrength, float pinchDistance, bool isExtended, double timeVisible)
    {
        FrameId = frameId;
        Id = Id;
        Fingers = fingers ?? new List<Finger>();
        PalmPosition = palmPosition;
        PalmVelocity = palmVelocity;
        PalmNormal = palmNormal;
        Direction = direction;
        Rotation = rotation;
        GrabStrength = grabStrength;
        PinchStrength = pinchStrength;
        PinchDistance = pinchDistance;
        IsExtended = isExtended;
        TimeVisible = timeVisible;
    }
    */

    public override string ToString()
    {
        string fingersInfo = "";
        foreach (var finger in Fingers)
        {
            fingersInfo += finger.ToString() + "\n";
        }

        return $"FrameId: {FrameId}, Id: {Id}, Fingers: \n{fingersInfo}, PalmPosition: {PalmPosition}, PalmVelocity: {PalmVelocity}, PalmNormal: {PalmNormal}, Direction: {Direction}, Rotation: {Rotation}, GrabStrength: {GrabStrength}, PinchStrength: {PinchStrength}, PinchDistance: {PinchDistance}, IsExtended: {IsExtended}, TimeVisible: {TimeVisible}";
    }

}
