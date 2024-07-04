using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public InputField roomInput;
    public InputField joinRoomInput;
    public InputField nicknameInput;
    public Text statusText;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button exitButton;

    void Start() {
        NetworkManager.Instance.StatusText = statusText;
        NetworkManager.Instance.roomInput = roomInput;
        NetworkManager.Instance.joinRoomInput = joinRoomInput;
        NetworkManager.Instance.NickNameInput = nicknameInput;

        createRoomButton.onClick.AddListener(() => NetworkManager.Instance.CreateRoom());
        joinRoomButton.onClick.AddListener(() => NetworkManager.Instance.JoinRoom());
        exitButton.onClick.AddListener(() => NetworkManager.Instance.Disconnect());
        

        // 씬 전환 후 UI 요소 초기화
        nicknameInput.text = PhotonNetwork.LocalPlayer.NickName;
    }
}
