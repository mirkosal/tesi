using System;

using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class CustomLocaleManager
{
    private Locale currentLocale;

    public void SetLanguage(string language)
    {
        ChangeSystemLanguage(language);
    }

    private void ChangeSystemLanguage(string language)
    {
        if (language == "English")
        {
            // Cambia la lingua del sistema in inglese
            Debug.Log("Lingua cambiata in Inglese");
            SetLocale("en");
        }
        else if (language == "Italian")
        {
            // Cambia la lingua del sistema in italiano
            Debug.Log("Lingua cambiata in Italiano");
            SetLocale("it");
        }
        else
        {
            Debug.LogWarning("Lingua non supportata: " + language);
        }
    }

    private void SetLocale(string localeCode)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        foreach (var locale in locales)
        {
            if (locale.Identifier.Code == localeCode)
            {
                LocalizationSettings.SelectedLocale = locale;
                currentLocale = locale; // Aggiorna la variabile currentLocale
                break;
            }
        }
    }

    public string GetCurrentLanguage()
    {
        return currentLocale?.Identifier.Code;
    }
}
