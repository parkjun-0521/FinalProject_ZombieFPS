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

    public Button[] CellBtn;        // �� ��ư
    public Button PreviousBtn;      // ���� ��ư 
    public Button NextBtn;          // ���� ��ư

    void Start() {
        NetworkManager.Instance.statusText = statusText;
        NetworkManager.Instance.roomInput = roomInput;
        NetworkManager.Instance.NickNameInput = nicknameInput;

        NetworkManager.Instance.cellBtn = CellBtn;
        NetworkManager.Instance.previousBtn = PreviousBtn;
        NetworkManager.Instance.nextBtn = NextBtn;

        createRoomButton.onClick.AddListener(() => NetworkManager.Instance.CreateRoom());
        exitButton.onClick.AddListener(() => NetworkManager.Instance.Disconnect());
        for (int i = 0; i < CellBtn.Length; i++) {
            int index  = i;
            CellBtn[i].onClick.AddListener(() => NetworkManager.Instance.MyListClick(index));
        }
        PreviousBtn.onClick.AddListener(() => NetworkManager.Instance.MyListClick(-2));
        NextBtn.onClick.AddListener(() => NetworkManager.Instance.MyListClick(-1));
        

        // �� ��ȯ �� UI ��� �ʱ�ȭ
        nicknameInput.text = PhotonNetwork.LocalPlayer.NickName;
    }
}
