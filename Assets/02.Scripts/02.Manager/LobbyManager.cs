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

    public Button[] CellBtn;        // 방 버튼
    public Button PreviousBtn;      // 이전 버튼 
    public Button NextBtn;          // 이후 버튼

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
        
        // 씬 전환 후 UI 요소 초기화
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
            // 방 이름이 비어 있거나 공백인 경우
            inputRoomFail.GetComponent<Animator>().SetBool("isFail", true);
            StartCoroutine(UIExit());
            return;  // 메서드 종료
        }

        // 방 이름이 제공된 경우, 방에 접속 시도
        NetworkManager.Instance.JoinRoom();
    }
    
    IEnumerator UIExit()
    {
        yield return new WaitForSeconds(0.1f);
        inputRoomFail.GetComponent<Animator>().SetBool("isFail", false);
    }
}
