using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    // 로그인 
    public InputField idInput;
    public InputField passwordInput;
    public Button loginButton;

    // 회원가입
    public InputField createUserIDInput;
    public InputField createUserPWInput;
    public Button createUserButton;

    public Text StatusText;

    //string LoginURL = URLs.LoginUR;
    //string CreateUserURL = URLs.CreateUserURL;
    string LoginURL = URLs.LoginURL;
    string CreateUserURL = URLs.CreateUserURL;


    public GameObject CreateUserUI;
    public GameObject successLoginText;
    public GameObject failLoginText;
    public GameObject CreateUserFail;
    public GameObject CreateUserSuccess;

    void Start() {
        NetworkManager.Instance.NickNameInput = idInput;
        NetworkManager.Instance.statusText = StatusText;
        loginButton.onClick.AddListener(OnLogin);
        createUserButton.onClick.AddListener(OnCreateUser);
    }

    // 로그인 
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
                NetworkManager.Instance.Connect();
            }
            else {
                if (www.downloadHandler.text == "1") // 로그인 성공
                {
                    successLoginText.GetComponent<Animator>().SetBool("isFail", true);
                    yield return new WaitForSeconds(0.1f);
                    successLoginText.GetComponent<Animator>().SetBool("isFail", false);
                    NetworkManager.Instance.Connect();
                }
                else // 로그인 실패
                {
                    Debug.Log("Server Response: " + www.downloadHandler.text);
                    failLoginText.GetComponent<Animator>().SetBool("isFail", true);
                    yield return new WaitForSeconds(0.1f);
                    failLoginText.GetComponent<Animator>().SetBool("isFail", false);
                }
            }
        }
    }

    // 회원가입
    void OnCreateUser()
    {
        if (createUserIDInput.text.Length > 0 && createUserPWInput.text.Length > 0) {
            StartCoroutine(NewCreateUser(createUserIDInput.text, createUserPWInput.text));
        }
        else {
            Debug.Log("아이디 또는 패스워드를 입력해주세요!");
        }
    }

    IEnumerator NewCreateUser(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", username);
        form.AddField("UserPassword", password);

        using (UnityWebRequest www = UnityWebRequest.Post(CreateUserURL, form)) {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
                Debug.Log("회원이 만들어지지 않음: " + www.error);
            }
            else {
                // 응답 텍스트를 통해 회원가입 결과 확인
                string response = www.downloadHandler.text;
                if (response == "This username is already taken.") {
                    CreateUserFail.GetComponent<Animator>().SetBool("isFail", true);
                    yield return new WaitForSeconds(0.1f);
                    CreateUserFail.GetComponent<Animator>().SetBool("isFail", false);
                }
                else if (response == "1") {
                    createUserIDInput.text = "";
                    createUserPWInput.text = "";
                    CreateUserSuccess.GetComponent<Animator>().SetBool("isFail", true);
                    yield return new WaitForSeconds(0.1f);
                    CreateUserUI.SetActive(false);
                    CreateUserSuccess.GetComponent<Animator>().SetBool("isFail", false);
                }
                else {
                    Debug.Log("회원가입 실패: " + response);
                }
            }
        }
    }
}
