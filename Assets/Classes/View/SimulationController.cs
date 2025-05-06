using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Linq;

public class SimulationController : MonoBehaviour
{
    public SimulationView simulationView;

    void Update()
    {
        // Verifica se il tasto SPAZIO è stato premuto
        if (Input.GetKeyDown(KeyCode.Space))
        {
            simulationView.TriggerFingerMovement();  // Avvia l'animazione delle sfere
        }
    }
}
