using System.Collections.Generic;
using UnityEngine;

public class HandView : MonoBehaviour
{
    public GameObject[] fingerSpheres; // Array di 5 GameObject, uno per dito

    // Metodo per aggiornare la posizione delle sfere in base ai dati delle dita
    // Ora può essere chiamato direttamente in risposta a un evento
    public void OnFingerDataUpdated(List<Finger> fingers)
    {
        UpdateFingerPositions(fingers);
    }

    private void UpdateFingerPositions(List<Finger> fingers)
    {
        for (int i = 0; i < fingers.Count; i++)
        {
            if (i < fingerSpheres.Length)
            {
                fingerSpheres[i].transform.position = fingers[i].TipPosition;
                // Aggiungiamo anche una semplice animazione o transizione per rendere il movimento più fluido
                // StartCoroutine(MoveFinger(fingerSpheres[i].transform, fingers[i].TipPosition));
            }
        }
    }

    // Una coroutine per muovere le sfere con una transizione lineare
    /* IEnumerator MoveFinger(Transform fingerTransform, Vector3 newPosition)
    {
        float timeToMove = 0.5f; // Durata del movimento in secondi
        Vector3 startPosition = fingerTransform.position;
        float time = 0;

        while (time < 1)
        {
            time += Time.deltaTime / timeToMove;
            fingerTransform.position = Vector3.Lerp(startPosition, newPosition, time);
            yield return null;
        }
    } */
}
