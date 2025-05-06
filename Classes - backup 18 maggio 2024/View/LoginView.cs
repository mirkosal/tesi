//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginView : MonoBehaviour
{
    [SerializeField] private TMP_InputField txtUser;
    [SerializeField] private TMP_InputField txtPassword;
    private string userName;
    private string password;
    public void ShowLoginForm()
    {
        // Implementazione del metodo
    }

    public void ReceiveUserInput()
    {
        Login login;
        var logincontroller = (IController<Login>)ControllerFactory.Instance.CreateController<LoginController>("login");
        userName = txtUser.text;
        password = txtPassword.text;

        login = logincontroller.ExecuteTask(userName, password);

        if (login != null)
        {
            // Logica post-login: Carica la scena MenuPaziente
            SceneManager.LoadScene("MenuPaziente");
        }
        else
        {
            // Gestire il fallimento del login
            Debug.Log("login non eseguito");
        }
    }

}

