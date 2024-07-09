using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public InputField roomInput;
    public InputField nicknameInput;
    public Text statusText;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button exitButton;

    public Button[] CellBtn;        // 방 버튼
    public Button PreviousBtn;      // 이전 버튼 
    public Button NextBtn;          // 이후 버튼

    void Start() {
        NetworkManager.Instance.StatusText = statusText;
        NetworkManager.Instance.roomInput = roomInput;
        NetworkManager.Instance.NickNameInput = nicknameInput;

        NetworkManager.Instance.CellBtn = CellBtn;
        NetworkManager.Instance.PreviousBtn = PreviousBtn;
        NetworkManager.Instance.NextBtn = NextBtn;

        createRoomButton.onClick.AddListener(() => NetworkManager.Instance.CreateRoom());
        exitButton.onClick.AddListener(() => NetworkManager.Instance.Disconnect());
        for (int i = 0; i < CellBtn.Length; i++) {
            int index  = i;
            CellBtn[i].onClick.AddListener(() => NetworkManager.Instance.MyListClick(index));
        }
        PreviousBtn.onClick.AddListener(() => NetworkManager.Instance.MyListClick(-2));
        NextBtn.onClick.AddListener(() => NetworkManager.Instance.MyListClick(-1));
        

        // 씬 전환 후 UI 요소 초기화
        nicknameInput.text = PhotonNetwork.LocalPlayer.NickName;
    }
}
