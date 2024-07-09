using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class NetworkManager : MonoBehaviourPunCallbacks {
    // �̱��� ���� 
    public static NetworkManager Instance;

    public Text StatusText;
    public InputField roomInput ,NickNameInput;
    public string playerName;

    public Button[] CellBtn;        // �� ��ư
    public Button PreviousBtn;      // ���� ��ư 
    public Button NextBtn;          // ���� ��ư
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
        myList.Clear();
    }

    // �� ����� ( MaxPlayers = �ִ��ο� �� ) 
    public void CreateRoom() => PhotonNetwork.CreateRoom(roomInput.text == "" ? "Room" + Random.Range(0, 100) : roomInput.text, new RoomOptions { MaxPlayers = 4 });

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
        PhotonNetwork.LoadLevel("03.MainGameScene");
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

    // �� ���� 
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
        // �ִ�������
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // ����, ������ư
        PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // �������� �´� ����Ʈ ����
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