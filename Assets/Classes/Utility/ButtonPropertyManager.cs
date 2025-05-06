using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonPropertyManager : MonoBehaviour
{
    [SerializeField] public Button referenceButton; // Bottone di riferimento per copiare le proprietà

    void Start()
    {
        if (referenceButton == null)
        {
            Debug.LogError("ButtonPropertyManager: Il bottone di riferimento non è assegnato!");
            return;
        }

        UpdateAllButtons();
    }

    public void UpdateAllButtons()
    {
        // Trova tutti i bottoni nella scena
        Button[] allButtons = FindObjectsOfType<Button>();

        foreach (Button btn in allButtons)
        {
            CopyButtonProperties(referenceButton, btn);
        }
    }

    private void CopyButtonProperties(Button referenceButton, Button targetButton)
    {
        if (referenceButton == null || targetButton == null)
        {
            Debug.LogError("ButtonPropertyManager: Reference or Target Button is null.");
            return;
        }

        // Copia le proprietà del RectTransform
/*        RectTransform referenceRect = referenceButton.GetComponent<RectTransform>();
        RectTransform targetRect = targetButton.GetComponent<RectTransform>();
        if (referenceRect != null && targetRect != null)
        {
            targetRect.sizeDelta = referenceRect.sizeDelta;
            targetRect.anchoredPosition = referenceRect.anchoredPosition;
            targetRect.localScale = referenceRect.localScale;
        }*/

        // Copia le proprietà del TextMeshPro
        TextMeshProUGUI referenceText = referenceButton.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI targetText = targetButton.GetComponentInChildren<TextMeshProUGUI>();
        if (referenceText != null && targetText != null)
        {
            targetText.font = referenceText.font;
            targetText.fontSize = referenceText.fontSize;
            targetText.color = referenceText.color;
            targetText.alignment = referenceText.alignment;
            targetText.enableAutoSizing = referenceText.enableAutoSizing;
            targetText.fontSizeMin = referenceText.fontSizeMin;
            targetText.fontSizeMax = referenceText.fontSizeMax;
            targetText.overflowMode = referenceText.overflowMode;
        }

        // Copia le proprietà dell'immagine
        Image referenceImage = referenceButton.GetComponent<Image>();
        Image targetImage = targetButton.GetComponent<Image>();
        if (referenceImage != null && targetImage != null)
        {
            targetImage.sprite = referenceImage.sprite;
            targetImage.color = referenceImage.color;
            targetImage.type = referenceImage.type;
        }
    }
}
