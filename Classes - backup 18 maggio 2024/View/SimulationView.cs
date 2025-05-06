using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Linq;

public class SimulationView : MonoBehaviour
{
    public HandAnimator handAnimator;

    public void Start()
    {
        UnityEngine.Debug.Log(DataManager.ToString());
    }

    public void TriggerFingerMovement()
    {
        StartCoroutine(AnimateActivities(0.03f)); // Default delay between animations set here
    }

    IEnumerator AnimateActivities(float delayBetweenActivities)
    {
        foreach (var activity in DataManager.CurrentTaskGroup.Activities)
        {
            foreach (var handData in activity.HandData)
            {
                foreach (var hand in handData.Hands)
                {
                    handAnimator.Animate(hand);  // Animate the hands using HandAnimator
                    yield return new WaitForSeconds(delayBetweenActivities); // Delay after each hand animation
                }
            }

            handAnimator.ResetPositions();  // Reset the spheres to their original position
            yield return new WaitForSeconds(delayBetweenActivities);  // Delay between different activity animations
        }

        // After all animations are complete, log the message and reset positions
        UnityEngine.Debug.Log("Animation completed");
    }
}
