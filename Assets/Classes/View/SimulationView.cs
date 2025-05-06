using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;
using System.Collections;

public class SimulationView : MonoBehaviour
{
    public HandAnimator handAnimator;
    public TextMeshProUGUI textMeshPro; // Assicurati di collegare il TextMeshPro dal tuo inspector

    public void Start()
    {
        UnityEngine.Debug.Log(DataManager.ToString());
        // Start the coroutine to display the string as soon as the scene loads
        StartCoroutine(DisplayLocalizedString());
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

    IEnumerator DisplayLocalizedString()
    {
        textMeshPro.gameObject.SetActive(true);

        // Wait for 5 seconds
        yield return new WaitForSeconds(5);

        // Hide the string
        textMeshPro.gameObject.SetActive(false);
    }
}
