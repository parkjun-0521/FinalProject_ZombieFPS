using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public InputField idInput;
    public InputField passwordInput;
    public Button loginButton;
    public Text StatusText;

    void Start() {
        NetworkManager.Instance.NickNameInput = idInput;
        NetworkManager.Instance.statusText = StatusText;
        loginButton.onClick.AddListener(OnLogin);
    }

    void OnLogin() {
        if (idInput.text.Length > 0 && passwordInput.text.Length > 0) {          
            NetworkManager.Instance.Connect();
        }
    }
}
