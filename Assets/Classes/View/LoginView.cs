using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoginView : MonoBehaviour
{
    [SerializeField] private TMP_InputField txtUser;
    [SerializeField] private TMP_InputField txtPassword;
    [SerializeField] private ToggleGroup languageToggleGroup;     private string userName;
    private string password;
    private LoginController loginController;
    private void Start()
    {
        loginController = new LoginController();
        foreach (Toggle toggle in languageToggleGroup.GetComponentsInChildren<Toggle>())
        {
            toggle.onValueChanged.AddListener(delegate {
                OnLanguageToggleChanged();
            });
        }
    }
    public void ReceiveUserInput()
    {
        userName = txtUser.text;
        password = txtPassword.text;
        string selectedLanguage = GetSelectedLanguage();
        loginController.SetLanguage(selectedLanguage);
        Login login = loginController.ExecuteTask(userName, password);

        if (login != null)
        {
            SceneManager.LoadScene("MenuPaziente");
        }
        else
        {
            Debug.Log("login non eseguito");
        }
    }
    private void OnLanguageToggleChanged()
    {
        string selectedLanguage = GetSelectedLanguage();
        loginController.SetLanguage(selectedLanguage);
    }
    private string GetSelectedLanguage()
    {
        Toggle[] toggles = languageToggleGroup.GetComponentsInChildren<Toggle>();
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn)
            {
                switch (i)
                {
                    case 0:
                        return "English";
                    case 1:
                        return "Italian";
                    default:
                        return "English"; 
                }
            }
        }
        return "English";
    }
}
