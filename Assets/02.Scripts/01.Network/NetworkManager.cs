using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviourPunCallbacks {
    // 싱글톤 구현 
    public static NetworkManager Instance;

    [Header("ETC")]
    public Text statusText;
    public PhotonView PV;
    public InputField roomInput ,NickNameInput, roomID;
    public string playerName;

    public GameObject inputRoomFail;                // 방입장 실패 UI
    public GameObject resignRoom;

    public Button[] cellBtn;                        // 방 버튼
    public Button previousBtn;                      // 이전 버튼 
    public Button nextBtn;                          // 다음 버튼
    List<RoomInfo> myList = new List<RoomInfo>();   // 방 리스트 
    int currentPage = 1, maxPage, multiple = 0;     // 페이지, 한페이지의 방 번호

    public Text[] chatText;                         // 채팅 배열 
    public InputField chatInput;                    // 채팅 입력 input


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
        StartCoroutine(ResignMessage());
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    IEnumerator ResignMessage() {
        yield return new WaitForSeconds(0.5f);
        if (ScenesManagerment.Instance.isResign) {
            resignRoom.GetComponent<Animator>().SetBool("isFail", true);
            StartCoroutine(UIExit());
            ScenesManagerment.Instance.isResign = false;
        }
    }

    // 방 만들기 ( MaxPlayers = 최대인원 수 ) 
    public void CreateRoom() => PhotonNetwork.CreateRoom(roomInput.text == "" ? "Room" + Random.Range(0, 100) : roomInput.text, new RoomOptions { MaxPlayers = 4 });

    public void JoinRoom() => PhotonNetwork.JoinRoom(roomID.text);

    // 방을 만들면서 입장
    // CreateRoom : CreateRoom은 매번 새로운 방을 만듦. 방이 이미 존재하는 경우에 방만들기 실패 
    // JoinOrCreateRoom : 해당 방의 이름이 존재하면 방입장. 방이 없으면 방을 만든다. ( 무조건 방을 생성할 수 있음 )
    public void JoinOrCreateRoom() => PhotonNetwork.JoinOrCreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 4 }, null);

    // 랜덤방 입장 ( 이 기능은 사용하지 않을 예정 ) 
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    // 방 떠나기 
    public void LeaveRoom() {
        string playerName = PhotonNetwork.NickName;

        PV.RPC("HPBarDelete", RpcTarget.AllBuffered, playerName);

        PhotonNetwork.LeaveRoom();
    }

    // 체력바 제거
    [PunRPC]
    public void HPBarDelete(string playerName) {
        for (int i = 0; i < UIManager.Instance.nickName.Length; i++) {
            if (playerName == UIManager.Instance.nickName[i].text) {
                UIManager.Instance.nickName[i].text = "";
                UIManager.Instance.hpBar[i].value = 0;
            }
        }
    }
    // 방이 생성되었을 때 동작할 부분 
    public override void OnCreatedRoom() {
        Debug.Log("방만들기 완료");
    }

    // 방에 입장했을 때 동작하는 부분 
    public override void OnJoinedRoom() {
        Debug.Log("방참가완료");
        PhotonNetwork.LoadLevel("03.MainGameScene");
        StartCoroutine(DeleteItemData(PhotonNetwork.NickName));
        AudioManager.Instance.PlayBgm(true, 0);
    }

    IEnumerator DeleteItemData(string userID)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", userID);

        using (UnityWebRequest www = UnityWebRequest.Post(URLs.ItemDeleteURL, form)) {
            yield return www.SendWebRequest();
        }
    }

    public override void OnLeftRoom() {
        Debug.Log("방 떠나기 완료");
        ScenesManagerment.Instance.playerCount = 0;
        ChangeScene("02.LobbyScene"); // 로비 씬으로 변경
    }

    // 예외 처리 
    public override void OnCreateRoomFailed( short returnCode, string message ) => print("방만들기실패");
    public override void OnJoinRoomFailed(short returnCode, string message) {
        inputRoomFail.GetComponent<Animator>().SetBool("isFail", true);
        print("방참가실패");
        StartCoroutine(UIExit());
    }
    IEnumerator UIExit()
    {
        yield return new WaitForSeconds(0.1f);
        inputRoomFail.GetComponent<Animator>().SetBool("isFail", false);
    }
    public override void OnJoinRandomFailed( short returnCode, string message ) => print("방랜덤참가실패");

    private void OnSceneLoaded( Scene scene, LoadSceneMode mode ) {
        // 씬 로드 후 플레이어 생성

        if (PhotonNetwork.InRoom) {
            CreatePlayer();

            if (scene.name == "03.MainGameScene") {
                GameObject gameReadyorStartObj = GameObject.FindGameObjectWithTag("ReadyUI");
                if (gameReadyorStartObj != null) {
                    PhotonView pv = gameReadyorStartObj.GetComponent<PhotonView>();
                    if (pv != null) {
                        pv.RPC("UpdateUserNameRPC", RpcTarget.AllBuffered);
                    }
                }
            }
        }

    }
    public void CreatePlayer() {
        GameObject player = PhotonNetwork.Instantiate("PlayerPrefab", transform.position, Quaternion.identity);
        if (ScenesManagerment.Instance.stageCount < 3) {
            GameObject[] playerSpawnPoint = GameObject.FindGameObjectsWithTag("PlayerSpawnPoint");
            int index = ((int)player.GetComponent<PhotonView>().ViewID / 1000) % 4;
            switch (index) {
                case 0:
                    player.transform.position = playerSpawnPoint[0].transform.position;
                    player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;
                case 1:
                    player.transform.position = playerSpawnPoint[1].transform.position;
                    player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;
                case 2:
                    player.transform.position = playerSpawnPoint[2].transform.position;
                    player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;
                case 3:
                    player.transform.position = playerSpawnPoint[3].transform.position;
                    player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;
            }
        }
    }

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

    // 방 입장 ( 로비 리스트 구현 부분 ) 
    public void MyListClick( int num ) {
        if (num == -2) --currentPage;                   // 이전 버튼을 눌렀을 때 
        else if (num == -1) ++currentPage;              // 다음 버튼을 눌렀을 때
        else {
            int index = multiple + num;
            PhotonNetwork.JoinRoom(myList[index].Name); // 방 리스트 클릭시 입장 
        }
        MyListRenewal();
    }
    void MyListRenewal() {
        // 최대페이지
        maxPage = (myList.Count % cellBtn.Length == 0) ? myList.Count / cellBtn.Length : myList.Count / cellBtn.Length + 1;

        // 이전, 다음버튼
        previousBtn.interactable = (currentPage <= 1) ? false : true;
        nextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
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


    // 채팅 구현 부분 
    public void Send() {
        // RPC 동기화 
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + chatInput.text);
        // 채팅을 친 후 input창 초기화 
        chatInput.text = "";
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC( string msg ) {
        bool isInput = false;
        for (int i = 0; i < chatText.Length; i++) {
            if (chatText[i].text == "") {
                isInput = true;
                chatText[i].text = msg;
                break;
            }
        }

        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < chatText.Length; i++) 
                chatText[i - 1].text = chatText[i].text;
            chatText[chatText.Length - 1].text = msg;
        }
    }

    public void ChangeScene( string sceneName ) {
        PhotonNetwork.LoadLevel(sceneName);
    }




    void OnApplicationQuit()
    {

        Debug.Log("애플리케이션이 종료됩니다.");

        // 여기에서 필요한 정리 작업을 수행합니다.
        LeaveRoom();
    }
}