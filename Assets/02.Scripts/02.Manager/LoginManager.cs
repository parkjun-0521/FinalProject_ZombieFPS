using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public InputField idInput;
    public InputField passwordInput;
    public Button loginButton;
    public Text StatusText;

    string LoginURL = "http://localhost/Zombie_Login.php";
    //string LoginURL = "http://223.131.75.181:1356//Zombie_Login.php";

    void Start() {
        NetworkManager.Instance.NickNameInput = idInput;
        NetworkManager.Instance.statusText = StatusText;
        loginButton.onClick.AddListener(OnLogin);
    }

    void OnLogin() {
        if (idInput.text.Length > 0 && passwordInput.text.Length > 0) {
            StartCoroutine(LoginToDB(idInput.text, passwordInput.text));
        }
        else {
            Debug.Log("아이디 또는 패스워드를 입력해주세요!");
        }
    }

    IEnumerator LoginToDB(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", username);
        form.AddField("UserPassword", password);

        using (UnityWebRequest www = UnityWebRequest.Post(LoginURL, form)) {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
                StatusText.text = "Error connecting to server: " + www.error;
                Debug.LogError(www.error);
            }
            else {
                if (www.downloadHandler.text == "1") // 로그인 성공
                {
                    NetworkManager.Instance.Connect();
                }
                else // 로그인 실패
                {
                    Debug.Log("아이디 또는 비밀번호가 틀렸습니다");
                }
            }
        }
    }
}
