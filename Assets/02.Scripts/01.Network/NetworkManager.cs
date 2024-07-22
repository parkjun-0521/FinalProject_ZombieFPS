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

    [Header("ETC")]
    public Text statusText;
    public PhotonView PV;
    public InputField roomInput ,NickNameInput, roomID;
    public string playerName;

    public GameObject inputRoomFail;                // ������ ���� UI

    public Button[] cellBtn;                        // �� ��ư
    public Button previousBtn;                      // ���� ��ư 
    public Button nextBtn;                          // ���� ��ư
    List<RoomInfo> myList = new List<RoomInfo>();   // �� ����Ʈ 
    int currentPage = 1, maxPage, multiple = 0;     // ������, ���������� �� ��ȣ

    public Text[] chatText;                         // ä�� �迭 
    public InputField chatInput;                    // ä�� �Է� input

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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Update() {
        if (statusText != null) {
            statusText.text = PhotonNetwork.NetworkClientState.ToString();
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

    public void JoinRoom() => PhotonNetwork.JoinRoom(roomID.text);

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
    public override void OnJoinRoomFailed(short returnCode, string message) {
        inputRoomFail.GetComponent<Animator>().SetBool("isFail", true);
        print("����������");
        StartCoroutine(UIExit());
    }
    IEnumerator UIExit()
    {
        yield return new WaitForSeconds(0.1f);
        inputRoomFail.GetComponent<Animator>().SetBool("isFail", false);
    }
    public override void OnJoinRandomFailed( short returnCode, string message ) => print("�淣����������");

    private void OnSceneLoaded( Scene scene, LoadSceneMode mode ) {
        // �� �ε� �� �÷��̾� ����
        if (PhotonNetwork.InRoom) {
            CreatePlayer();
        }
    }
    public void CreatePlayer() {
            GameObject player = PhotonNetwork.Instantiate("PlayerPrefab", GetRandomSpawnPosition(), Quaternion.identity);
    }
    private Vector3 GetRandomSpawnPosition() {
        // ������ ���� ��ġ�� ��ȯ (���÷� 10x10 ���� ������ ������ ��ġ�� ����)
        float x = Random.Range(-5f, 5f);
        float y = 1f; // ��� �� ��ġ��Ű�� ���� y ��ǥ�� 0���� ����
        float z = Random.Range(-5f, 5f);
        return new Vector3(x, y, z);
    }

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

    // �� ���� ( �κ� ����Ʈ ���� �κ� ) 
    public void MyListClick( int num ) {
        if (num == -2) --currentPage;                   // ���� ��ư�� ������ �� 
        else if (num == -1) ++currentPage;              // ���� ��ư�� ������ ��
        else {
            int index = multiple + num;
            PhotonNetwork.JoinRoom(myList[index].Name); // �� ����Ʈ Ŭ���� ���� 
        }
        MyListRenewal();
    }
    void MyListRenewal() {
        // �ִ�������
        maxPage = (myList.Count % cellBtn.Length == 0) ? myList.Count / cellBtn.Length : myList.Count / cellBtn.Length + 1;

        // ����, ������ư
        previousBtn.interactable = (currentPage <= 1) ? false : true;
        nextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // �������� �´� ����Ʈ ����
        multiple = (currentPage - 1) * cellBtn.Length;
        for (int i = 0; i < cellBtn.Length; i++) {
            cellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            cellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            cellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
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


    // ä�� ���� �κ� 
    public void Send() {
        // RPC ����ȭ 
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + chatInput.text);
        // ä���� ģ �� inputâ �ʱ�ȭ 
        chatInput.text = "";
    }

    [PunRPC] // RPC�� �÷��̾ �����ִ� �� ��� �ο����� �����Ѵ�
    void ChatRPC( string msg ) {
        bool isInput = false;
        for (int i = 0; i < chatText.Length; i++) {
            if (chatText[i].text == "") {
                isInput = true;
                chatText[i].text = msg;
                break;
            }
        }

        if (!isInput) // ������ ��ĭ�� ���� �ø�
        {
            for (int i = 1; i < chatText.Length; i++) 
                chatText[i - 1].text = chatText[i].text;
            chatText[chatText.Length - 1].text = msg;
        }
    }

    public void ChangeScene( string sceneName ) {
        PhotonNetwork.LoadLevel(sceneName);
    }
}