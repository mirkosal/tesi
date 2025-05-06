using System;
using UnityEngine;

[Serializable]
public class Bone
{
    public int BoneID { get; set; }
    public Vector3 PrevJoint { get; set; }
    public Vector3 NextJoint { get; set; }
    public Vector3 Center { get; set; }
    public Quaternion Rotation { get; set; }
    public float Length { get; set; }
    public float Width { get; set; }
    public int Type { get; set; }
    public int FingerID { get; set; } // Aggiunta nuova proprietà FingerID

    // Costruttore senza parametri
    public Bone()
    {
    }

    // Metodo ToString per visualizzare le informazioni dell'oggetto, incluso il FingerID
    public override string ToString()
    {
        return $"PrevJoint: {PrevJoint}, NextJoint: {NextJoint}, Center: {Center}, Rotation: {Rotation}, Length: {Length}, Width: {Width}, Type: {Type}, FingerID: {FingerID}";
    }
}
