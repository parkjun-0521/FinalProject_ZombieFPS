using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public InputField roomInput;
    public InputField nicknameInput;
    public InputField roomID;
    public Text statusText;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button exitButton;

    public Button[] CellBtn;        // �� ��ư
    public Button PreviousBtn;      // ���� ��ư 
    public Button NextBtn;          // ���� ��ư

    public GameObject inputRoomFail;
    public GameObject ResignRoom;

    void Start() {
        NetworkManager.Instance.statusText = statusText;
        NetworkManager.Instance.roomInput = roomInput;
        NetworkManager.Instance.NickNameInput = nicknameInput;
        NetworkManager.Instance.roomID = roomID;

        NetworkManager.Instance.inputRoomFail = inputRoomFail;

        NetworkManager.Instance.cellBtn = CellBtn;
        NetworkManager.Instance.previousBtn = PreviousBtn;
        NetworkManager.Instance.nextBtn = NextBtn;

        createRoomButton.onClick.AddListener(() => NetworkManager.Instance.CreateRoom());
        joinRoomButton.onClick.AddListener(TryJoinRoom);
        exitButton.onClick.AddListener(() => NetworkManager.Instance.Disconnect());
        for (int i = 0; i < CellBtn.Length; i++) {
            int index  = i;
            CellBtn[i].onClick.AddListener(() => NetworkManager.Instance.MyListClick(index));
        }
        PreviousBtn.onClick.AddListener(() => NetworkManager.Instance.MyListClick(-2));
        NextBtn.onClick.AddListener(() => NetworkManager.Instance.MyListClick(-1));
        
        // �� ��ȯ �� UI ��� �ʱ�ȭ
        nicknameInput.text = PhotonNetwork.LocalPlayer.NickName;

        if (ScenesManagerment.Instance.isResign) {
            ResignRoom.GetComponent<Animator>().SetBool("isFail", true);
            StartCoroutine(UIExit());
            ScenesManagerment.Instance.isResign = false;
        }
    }
    void TryJoinRoom()
    {
        string roomName = roomID.text.Trim();

        if (string.IsNullOrEmpty(roomName)) {
            // �� �̸��� ��� �ְų� ������ ���
            inputRoomFail.GetComponent<Animator>().SetBool("isFail", true);
            StartCoroutine(UIExit());
            return;  // �޼��� ����
        }

        // �� �̸��� ������ ���, �濡 ���� �õ�
        NetworkManager.Instance.JoinRoom();
    }
    
    IEnumerator UIExit()
    {
        yield return new WaitForSeconds(0.1f);
        inputRoomFail.GetComponent<Animator>().SetBool("isFail", false);
    }
}
