using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks {
    // �̱��� ���� 
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

    // ���� ���� Ȯ�� 
    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    public override void OnConnectedToMaster() {
        print("�������ӿϷ�");
        // �г��� �־��ִ� �� ( ���� ���� �޼ҵ� ) 
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        JoinLobby();
    }

    // ���� ���� Ȯ�� 
    public void Disconnect() => PhotonNetwork.Disconnect();
    public override void OnDisconnected( DisconnectCause cause ) {
        Debug.Log("���� ����");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // ���ø����̼� ����
#endif
    }
    // �κ� ���� Ȯ�� 
    public void JoinLobby() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby() {
        Debug.Log("�κ����� �Ϸ�");
        ChangeScene("02.LobbyScene");
    }

    // �� ����� ( MaxPlayers = �ִ��ο� �� ) 
    public void CreateRoom() => PhotonNetwork.CreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 4 });

    // �� ���� ( �� ��ȣ�� �´� ���� ������ �� ���� ) 
    public void JoinRoom() => PhotonNetwork.JoinRoom(joinRoomInput.text);

    // ���� ����鼭 ����
    // CreateRoom : CreateRoom�� �Ź� ���ο� ���� ����. ���� �̹� �����ϴ� ��쿡 �游��� ���� 
    // JoinOrCreateRoom : �ش� ���� �̸��� �����ϸ� ������. ���� ������ ���� �����. ( ������ ���� ������ �� ���� )
    public void JoinOrCreateRoom() => PhotonNetwork.JoinOrCreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 4 }, null);

    // ������ ���� ( �� ����� ������� ���� ���� ) 
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    // �� ������ 
    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
    }

    // ���� �����Ǿ��� �� ������ �κ� 
    public override void OnCreatedRoom() {
        Debug.Log("�游��� �Ϸ�");
    }

    // �濡 �������� �� �����ϴ� �κ� 
    public override void OnJoinedRoom() {
        Debug.Log("�������Ϸ�");
        ChangeScene("03.MainGameScene"); // ���� ������ ����
    }

    public override void OnLeftRoom() {
        Debug.Log("�� ������ �Ϸ�");
        ChangeScene("02.LobbyScene"); // �κ� ������ ����
    }

    // ���� ó�� 
    public override void OnCreateRoomFailed( short returnCode, string message ) => print("�游������");
    public override void OnJoinRoomFailed( short returnCode, string message ) => print("����������");
    public override void OnJoinRandomFailed( short returnCode, string message ) => print("�淣����������");


    [ContextMenu("����")]
    void Info() {
        if (PhotonNetwork.InRoom) {
            print("���� �� �̸� : " + PhotonNetwork.CurrentRoom.Name);
            print("���� �� �ο��� : " + PhotonNetwork.CurrentRoom.PlayerCount);
            print("���� �� �ִ��ο��� : " + PhotonNetwork.CurrentRoom.MaxPlayers);

            string playerStr = "�濡 �ִ� �÷��̾� ��� : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";
            print(playerStr);
        }
        else {
            print("������ �ο� �� : " + PhotonNetwork.CountOfPlayers);
            print("�� ���� : " + PhotonNetwork.CountOfRooms);
            print("��� �濡 �ִ� �ο� �� : " + PhotonNetwork.CountOfPlayersInRooms);
            print("�κ� �ִ���? : " + PhotonNetwork.InLobby);
            print("����ƴ���? : " + PhotonNetwork.IsConnected);
        }
    }

    public void ChangeScene( string sceneName ) {
        SceneManager.LoadScene(sceneName);
    }
}