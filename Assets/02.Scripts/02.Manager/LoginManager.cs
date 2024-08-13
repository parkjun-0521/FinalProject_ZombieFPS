using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    // �α��� 
    public InputField idInput;
    public InputField passwordInput;
    public Button loginButton;

    // ȸ������
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

    // �α��� 
    void OnLogin() {
        if (idInput.text.Length > 0 && passwordInput.text.Length > 0) {
            StartCoroutine(LoginToDB(idInput.text, passwordInput.text));
        }
        else {
            Debug.Log("���̵� �Ǵ� �н����带 �Է����ּ���!");
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
                if (www.downloadHandler.text == "1") // �α��� ����
                {
                    successLoginText.GetComponent<Animator>().SetBool("isFail", true);
                    yield return new WaitForSeconds(0.1f);
                    successLoginText.GetComponent<Animator>().SetBool("isFail", false);
                    NetworkManager.Instance.Connect();
                }
                else // �α��� ����
                {
                    Debug.Log("Server Response: " + www.downloadHandler.text);
                    failLoginText.GetComponent<Animator>().SetBool("isFail", true);
                    yield return new WaitForSeconds(0.1f);
                    failLoginText.GetComponent<Animator>().SetBool("isFail", false);
                }
            }
        }
    }

    // ȸ������
    void OnCreateUser()
    {
        if (createUserIDInput.text.Length > 0 && createUserPWInput.text.Length > 0) {
            StartCoroutine(NewCreateUser(createUserIDInput.text, createUserPWInput.text));
        }
        else {
            Debug.Log("���̵� �Ǵ� �н����带 �Է����ּ���!");
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
                Debug.Log("ȸ���� ��������� ����: " + www.error);
            }
            else {
                // ���� �ؽ�Ʈ�� ���� ȸ������ ��� Ȯ��
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
                    Debug.Log("ȸ������ ����: " + response);
                }
            }
        }
    }
}
