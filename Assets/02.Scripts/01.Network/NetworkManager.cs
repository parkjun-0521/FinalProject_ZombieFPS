using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class NetworkManager : MonoBehaviourPunCallbacks {
    // 싱글톤 구현 
    public static NetworkManager Instance;

    public Text StatusText;
    public InputField roomInput ,NickNameInput;
    public string playerName;

    public Button[] CellBtn;        // 방 버튼
    public Button PreviousBtn;      // 이전 버튼 
    public Button NextBtn;          // 이후 버튼
    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple = 0;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
            return;
        }
        Screen.SetResolution(1920, 1080, false);
    }

    void Start() {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Update() {
        if (StatusText != null) {
            StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        }
    }

    // 서버 연결 확인 
    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    public override void OnConnectedToMaster() {
        print("서버접속완료");
        // 닉네임 넣어주는 거 ( 포톤 내장 메소드 ) 
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        JoinLobby();
    }

    // 서버 해제 확인 
    public void Disconnect() => PhotonNetwork.Disconnect();
    public override void OnDisconnected( DisconnectCause cause ) {
        Debug.Log("연결 끊김");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }
    // 로비 연결 확인 
    public void JoinLobby() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby() {
        Debug.Log("로비접속 완료");
        ChangeScene("02.LobbyScene");
        myList.Clear();
    }

    // 방 만들기 ( MaxPlayers = 최대인원 수 ) 
    public void CreateRoom() => PhotonNetwork.CreateRoom(roomInput.text == "" ? "Room" + Random.Range(0, 100) : roomInput.text, new RoomOptions { MaxPlayers = 4 });

    // 방을 만들면서 입장
    // CreateRoom : CreateRoom은 매번 새로운 방을 만듦. 방이 이미 존재하는 경우에 방만들기 실패 
    // JoinOrCreateRoom : 해당 방의 이름이 존재하면 방입장. 방이 없으면 방을 만든다. ( 무조건 방을 생성할 수 있음 )
    public void JoinOrCreateRoom() => PhotonNetwork.JoinOrCreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 4 }, null);

    // 랜덤방 입장 ( 이 기능은 사용하지 않을 예정 ) 
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    // 방 떠나기 
    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
    }

    // 방이 생성되었을 때 동작할 부분 
    public override void OnCreatedRoom() {
        Debug.Log("방만들기 완료");
    }

    // 방에 입장했을 때 동작하는 부분 
    public override void OnJoinedRoom() {
        Debug.Log("방참가완료");
        PhotonNetwork.LoadLevel("03.MainGameScene");
    }

    public override void OnLeftRoom() {
        Debug.Log("방 떠나기 완료");
        ChangeScene("02.LobbyScene"); // 로비 씬으로 변경
    }

    // 예외 처리 
    public override void OnCreateRoomFailed( short returnCode, string message ) => print("방만들기실패");
    public override void OnJoinRoomFailed( short returnCode, string message ) => print("방참가실패");
    public override void OnJoinRandomFailed( short returnCode, string message ) => print("방랜덤참가실패");


    [ContextMenu("정보")]
    void Info() {
        if (PhotonNetwork.InRoom) {
            print("현재 방 이름 : " + PhotonNetwork.CurrentRoom.Name);
            print("현재 방 인원수 : " + PhotonNetwork.CurrentRoom.PlayerCount);
            print("현재 방 최대인원수 : " + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerStr = "방에 있는 플레이어 목록 : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";
            print(playerStr);
        }
        else {
            print("접속한 인원 수 : " + PhotonNetwork.CountOfPlayers);
            print("방 개수 : " + PhotonNetwork.CountOfRooms);
            print("모든 방에 있는 인원 수 : " + PhotonNetwork.CountOfPlayersInRooms);
            print("로비에 있는지? : " + PhotonNetwork.InLobby);
            print("연결됐는지? : " + PhotonNetwork.IsConnected);
        }
    }

    // 방 입장 
    public void MyListClick( int num ) {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else {
            int index = multiple + num;
            if (index >= 0 && index < myList.Count) {
                PhotonNetwork.JoinRoom(myList[index].Name);
            }
            else {
                Debug.LogWarning("Invalid room index: " + index);
                Debug.LogWarning("Invalid room index: " + num);
            }
        }
        MyListRenewal();
    }
    void MyListRenewal() {
        // 최대페이지
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // 이전, 다음버튼
        PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * CellBtn.Length;
        for (int i = 0; i < CellBtn.Length; i++) {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
    }
    public override void OnRoomListUpdate( List<RoomInfo> roomList ) {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++) {
            if (!roomList[i].RemovedFromList) {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }

    public void ChangeScene( string sceneName ) {
        PhotonNetwork.LoadLevel(sceneName);
    }
}