using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NextSceneManager : MonoBehaviourPunCallbacks {
    public static NextSceneManager Instance;

    private Coroutine currentCoroutine1;
    private Coroutine currentCoroutine2;

    public int deadPlayerCount;             // 죽은 플레이어 수

    public bool isQuest1 = false;
    public bool isQuest2 = false;
    public bool isQuest3 = false;

    public bool isItemInfoSaved = false;
    public bool isSceneChange = false;

    public GameObject endLoading;
    public GameObject mapGimmick;

    List<GameObject> playersInTrigger = new List<GameObject>();
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        isItemInfoSaved = false;
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(GetItemData());
    }

    void Update() {
        if (isQuest2 && mapGimmick.activeSelf) {
            photonView.RPC("GimickRPC", RpcTarget.All, false);
        }
    }

    [PunRPC]
    public void GimickRPC(bool isCheck ) {
        mapGimmick.SetActive(isCheck);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "02.LobbyScene") {
            AudioManager.Instance.PlayBgm(true, ScenesManagerment.Instance.stageCount);
            //photonView.RPC("AudioBgm", RpcTarget.All, ScenesManagerment.Instance.stageCount);
            isItemInfoSaved = false;
        }
    }

    [PunRPC]
    public void AudioBgm(int bgmCount) {
        AudioManager.Instance.PlayBgm(true, bgmCount);
    }

    IEnumerator GetItemData()
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", PhotonNetwork.NickName); // 사용자 ID 전송

        using (UnityWebRequest www = UnityWebRequest.Post(URLs.ItemInputURL, form)) {
            yield return www.SendWebRequest(); // 요청 보내기 및 응답 대기
            ParseItemData(www.downloadHandler.text); // 응답 데이터 파싱
        }
    }

    void ParseItemData(string jsonData)
    {
        string[] items = jsonData.Split('\n'); // 각 줄별로 아이템 데이터 분리
        foreach (string item in items) {
            string[] details = item.Split('-'); // 아이템 이름과 개수 분리
            if (details.Length == 2) {
                string itemName = details[0].Trim(); // Trim() 제거
                int itemCount = int.Parse(details[1].Trim()); // Trim() 제거 및 int 변환도 제거
                Debug.Log("Item Name: " + itemName + ", Item Count: " + itemCount);

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject player in players) {
                    PhotonView pv = player.GetComponent<PhotonView>();
                    if (pv != null && pv.IsMine) {
                        // 해당 플레이어에게 아이템 인스턴스화 및 전달
                        Vector3 spawnPosition = player.transform.position + new Vector3(1, 0, 0); // 플레이어 옆에 아이템 생성
                        GameObject itemInstance = Pooling.instance.GetObject(itemName, spawnPosition);
                        itemInstance.GetComponent<ItemPickUp>().itemCount = itemCount;
                        // 아이템을 플레이어가 주워 인벤토리에 추가
                        player.GetComponent<Player>().ItemPickUp(itemInstance);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) {
            ScenesManagerment.Instance.playerCount += 1;
            playersInTrigger.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) {
            ScenesManagerment.Instance.playerCount -= 1;
            isItemInfoSaved = false;  // 플레이어가 나가면 플래그 리셋
            isSceneChange = false;
            //isNextScene = false;
            if (ScenesManagerment.Instance.playerCount != (PhotonNetwork.CurrentRoom.PlayerCount - deadPlayerCount)) {
                endLoading.SetActive(false);
            }

            if (ScenesManagerment.Instance.stageCount == 0) {
                if (currentCoroutine1 != null)
                    StopCoroutine(currentCoroutine1);
            }
            else if (ScenesManagerment.Instance.stageCount == 1) {
                if (currentCoroutine2 != null)
                    StopCoroutine(currentCoroutine2);
            }
        }
    }

    private void OnTriggerStay(Collider other) 
    {
        if (other.CompareTag("Player") && (isQuest1 || isQuest2 || isQuest3)) {
            Debug.Log(ScenesManagerment.Instance.playerCount);
            Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
            Debug.Log(deadPlayerCount);

            if (ScenesManagerment.Instance.playerCount == (PhotonNetwork.CurrentRoom.PlayerCount - deadPlayerCount)) {
                PhotonView photonView = other.GetComponent<PhotonView>();
                if (photonView != null && photonView.IsMine && photonView.Owner.NickName == PhotonNetwork.NickName) {
                    if (!isItemInfoSaved) {
                        isItemInfoSaved = true;
                        endLoading.SetActive(true);
                        StartCoroutine(DeleteItemData(PhotonNetwork.NickName, other.gameObject));
                    }
                }
            }
            // 방장 플레이어가 nextStageZone에 들어왔을 때 씬을 로드합니다.
            if (PhotonNetwork.IsMasterClient) {
                if (ScenesManagerment.Instance.playerCount == (PhotonNetwork.CurrentRoom.PlayerCount - deadPlayerCount)) {
                    if (ScenesManagerment.Instance.stageCount == 0 && isQuest1) {
                        if (!isSceneChange) {
                            currentCoroutine1 = StartCoroutine(SenecChange1());
                        }
                    }
                    else if (ScenesManagerment.Instance.stageCount == 1 && isQuest2) {
                        if (!isSceneChange) {
                            currentCoroutine2 = StartCoroutine(SenecChange2());
                        }
                    }
                    else if (ScenesManagerment.Instance.stageCount == 2 && isQuest3) {
                        if (!isSceneChange)
                            StartCoroutine(SenecChange3());
                    }
                }
            }
        }
    }

    IEnumerator SenecChange1()
    {
        isSceneChange = true;
        yield return new WaitForSeconds(4.5f);
        photonView.RPC("ResetCount", RpcTarget.All, ScenesManagerment.Instance.stageCount);
        yield return new WaitForSeconds(0.5f);
        PhotonNetwork.LoadLevel("03.MainGameScene_1");
    }

    IEnumerator SenecChange2()
    {
        isSceneChange = true;
        yield return new WaitForSeconds(4.5f);
        photonView.RPC("ResetCount", RpcTarget.All, ScenesManagerment.Instance.stageCount);
        yield return new WaitForSeconds(0.5f);
        PhotonNetwork.LoadLevel("03.MainGameScene_2");
    }

    IEnumerator SenecChange3() {
        isSceneChange = true;
        yield return new WaitForSeconds(4.5f);     
        photonView.RPC("ResetCount", RpcTarget.All , ScenesManagerment.Instance.stageCount);
        yield return new WaitForSeconds(0.5f);
        PhotonNetwork.LoadLevel("04.Ending");
    }

    [PunRPC]
    public void ResetCount(int stageCount) {
        AudioManager.Instance.PlayBgm(false, stageCount);
        ScenesManagerment.Instance.stageCount += 1;
        ScenesManagerment.Instance.playerCount = 0;
        deadPlayerCount = 0;
    }

    IEnumerator DeleteItemData(string userID, GameObject other)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", userID);

        using (UnityWebRequest www = UnityWebRequest.Post(URLs.ItemDeleteURL, form)) {
            yield return www.SendWebRequest();
        }

        Inventory inventory = other.gameObject.GetComponentInChildren<Inventory>();
        inventory.AllItemInfo();
    }
}