using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks {
    // 싱글톤 구현 
    public static NetworkManager Instance;

    public Text StatusText;
    public InputField roomInput, joinRoomInput ,NickNameInput;
    public string playerName;

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
    }

    // 방 만들기 ( MaxPlayers = 최대인원 수 ) 
    public void CreateRoom() => PhotonNetwork.CreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 4 });

    // 방 입장 ( 방 번호에 맞는 방을 입장할 수 있음 ) 
    public void JoinRoom() => PhotonNetwork.JoinRoom(joinRoomInput.text);

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
        ChangeScene("03.MainGameScene"); // 게임 씬으로 변경
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

    public void ChangeScene( string sceneName ) {
        SceneManager.LoadScene(sceneName);
    }
}