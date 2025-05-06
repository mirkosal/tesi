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
    private Vector3[] initialPositions;  // Array per memorizzare le posizioni iniziali delle sfere

    void Awake()
    {
        // Inizializza e memorizza le posizioni iniziali delle sfere
        initialPositions = new Vector3[spheres.Length];
        for (int i = 0; i < spheres.Length; i++)
        {
            initialPositions[i] = spheres[i].transform.position;
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
    }

    public void ResetPositions()
    {
        for (int i = 0; i < spheres.Length; i++)
        {
            spheres[i].transform.position = initialPositions[i];
            // Opzionalmente, puoi anche loggare il reset delle posizioni
            UnityEngine.Debug.Log($"Sfera {i} resettata alla posizione iniziale: {initialPositions[i]}");
        }
    }
}

