using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Linq;

public class HandAnimator : MonoBehaviour
{
    public GameObject[] spheres;  // Array di GameObject per ogni sfera corrispondente a una dita
    public GameObject palm;  // Aggiungi un GameObject per il palmo
    public GameObject[] boneSpheres;  // Array di GameObject per ogni sfera rappresentante un osso

    private Vector3[] initialPositions;  // Array per memorizzare le posizioni iniziali delle sfere
    private Vector3 initialPalmPosition;  // Posizione iniziale del palmo
    private Quaternion initialPalmRotation;  // Rotazione iniziale del palmo
    private Vector3[] initialBonePositions;  // Posizioni iniziali delle sfere degli ossi

    void Awake()
    {
        // Inizializza e memorizza le posizioni iniziali delle sfere
        initialPositions = new Vector3[spheres.Length];
        for (int i = 0; i < spheres.Length; i++)
        {
            initialPositions[i] = spheres[i].transform.position;
        }

        // Memorizza la posizione e la rotazione iniziale del palmo
        if (palm != null)
        {
            initialPalmPosition = palm.transform.position;
            initialPalmRotation = palm.transform.rotation;
        }

        // Inizializza e memorizza le posizioni iniziali delle sfere degli ossi
        initialBonePositions = new Vector3[boneSpheres.Length];
        for (int i = 0; i < boneSpheres.Length; i++)
        {
            initialBonePositions[i] = boneSpheres[i].transform.position;
        }
    }

    public void Animate(Hand hand)
    {
        UnityEngine.Debug.Log($"Inizio animazione per la mano con ID: {hand.Id}");  // Assumi che ogni mano abbia un identificativo unico

        if (hand.Fingers.Count != spheres.Length)
        {
            UnityEngine.Debug.Log("Il numero di sfere non corrisponde al numero di dita");
            return;
        }

        for (int i = 0; i < hand.Fingers.Count; i++)
        {
            spheres[i].transform.position = hand.Fingers[i].TipPosition;
            UnityEngine.Debug.Log($"Sfera {i} animata a nuova posizione: {spheres[i].transform.position}");
        }

        // Anima il palmo
        AnimatePalm(hand);

        // Anima le ossa
        AnimateBones(hand);
    }

    public void AnimatePalm(Hand hand)
    {
        if (palm != null)
        {
            palm.transform.position = hand.PalmPosition;
            palm.transform.rotation = hand.Rotation;
            UnityEngine.Debug.Log($"Palmo animato a nuova posizione: {palm.transform.position}, nuova rotazione: {palm.transform.rotation}");
        }
        else
        {
            UnityEngine.Debug.Log("L'oggetto Palm non è stato assegnato.");
        }
    }

    public void AnimateBones(Hand hand)
    {
        int boneIndex = 0;
        foreach (var finger in hand.Fingers)
        {
            foreach (var bone in finger.bones)
            {
                if (boneIndex >= boneSpheres.Length)
                {
                    UnityEngine.Debug.Log("Il numero di sfere degli ossi non è sufficiente per rappresentare tutte le ossa.");
                    return;
                }

                Vector3 bonePosition = (bone.PrevJoint + bone.NextJoint) / 2;
                Quaternion boneRotation = bone.Rotation;

                // Debugging information
                UnityEngine.Debug.Log($"Osso {boneIndex}: PrevJoint: {bone.PrevJoint}, NextJoint: {bone.NextJoint}, Calculated Position: {bonePosition}, Rotation: {boneRotation}");

                boneSpheres[boneIndex].transform.position = bonePosition;
                boneSpheres[boneIndex].transform.rotation = boneRotation;

                boneIndex++;
            }
        }
    }

    public void ResetPositions()
    {
        for (int i = 0; i < spheres.Length; i++)
        {
            spheres[i].transform.position = initialPositions[i];
            // Opzionalmente, puoi anche loggare il reset delle posizioni
            UnityEngine.Debug.Log($"Sfera {i} resettata alla posizione iniziale: {initialPositions[i]}");
        }

        if (palm != null)
        {
            palm.transform.position = initialPalmPosition;
            palm.transform.rotation = initialPalmRotation;
            UnityEngine.Debug.Log($"Palmo resettato alla posizione iniziale: {initialPalmPosition}, rotazione iniziale: {initialPalmRotation}");
        }

        for (int i = 0; i < boneSpheres.Length; i++)
        {
            boneSpheres[i].transform.position = initialBonePositions[i];
            UnityEngine.Debug.Log($"Osso {i} resettato alla posizione iniziale: {initialBonePositions[i]}");
        }
    }
}



/*
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Linq;

public class HandAnimator : MonoBehaviour
{
    public GameObject[] spheres;  // Array di GameObject per ogni sfera corrispondente a una dita
    public GameObject palm;  // Aggiungi un GameObject per il palmo
    private Vector3[] initialPositions;  // Array per memorizzare le posizioni iniziali delle sfere
    private Vector3 initialPalmPosition;  // Posizione iniziale del palmo
    private Quaternion initialPalmRotation;  // Rotazione iniziale del palmo

    void Awake()
    {
        // Inizializza e memorizza le posizioni iniziali delle sfere
        initialPositions = new Vector3[spheres.Length];
        for (int i = 0; i < spheres.Length; i++)
        {
            initialPositions[i] = spheres[i].transform.position;
        }

        // Memorizza la posizione e la rotazione iniziale del palmo
        if (palm != null)
        {
            initialPalmPosition = palm.transform.position;
            initialPalmRotation = palm.transform.rotation;
        }
    }

    public void Animate(Hand hand)
    {
        UnityEngine.Debug.Log($"Inizio animazione per la mano con ID: {hand.Id}");  // Assumi che ogni mano abbia un identificativo unico

        if (hand.Fingers.Count != spheres.Length)
        {
            UnityEngine.Debug.Log("Il numero di sfere non corrisponde al numero di dita");
            return;
        }

        for (int i = 0; i < hand.Fingers.Count; i++)
        {
            spheres[i].transform.position = hand.Fingers[i].TipPosition;
            UnityEngine.Debug.Log($"Sfera {i} animata a nuova posizione: {spheres[i].transform.position}");
        }

        // Anima il palmo
        AnimatePalm(hand);
    }

    public void AnimatePalm(Hand hand)
    {
        if (palm != null)
        {
            palm.transform.position = hand.PalmPosition;
            palm.transform.rotation = hand.Rotation;
            UnityEngine.Debug.Log($"Palmo animato a nuova posizione: {palm.transform.position}, nuova rotazione: {palm.transform.rotation}");
        }
        else
        {
            UnityEngine.Debug.Log("L'oggetto Palm non è stato assegnato.");
        }
    }

    public void ResetPositions()
    {
        for (int i = 0; i < spheres.Length; i++)
        {
            spheres[i].transform.position = initialPositions[i];
            // Opzionalmente, puoi anche loggare il reset delle posizioni
            UnityEngine.Debug.Log($"Sfera {i} resettata alla posizione iniziale: {initialPositions[i]}");
        }

        if (palm != null)
        {
            palm.transform.position = initialPalmPosition;
            palm.transform.rotation = initialPalmRotation;
            UnityEngine.Debug.Log($"Palmo resettato alla posizione iniziale: {initialPalmPosition}, rotazione iniziale: {initialPalmRotation}");
        }
    }
}
*/