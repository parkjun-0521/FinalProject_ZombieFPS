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

    public int deadPlayerCount;             // ���� �÷��̾� ��

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
        form.AddField("UserID", PhotonNetwork.NickName); // ����� ID ����

        using (UnityWebRequest www = UnityWebRequest.Post(URLs.ItemInputURL, form)) {
            yield return www.SendWebRequest(); // ��û ������ �� ���� ���
            ParseItemData(www.downloadHandler.text); // ���� ������ �Ľ�
        }
    }

    void ParseItemData(string jsonData)
    {
        string[] items = jsonData.Split('\n'); // �� �ٺ��� ������ ������ �и�
        foreach (string item in items) {
            string[] details = item.Split('-'); // ������ �̸��� ���� �и�
            if (details.Length == 2) {
                string itemName = details[0].Trim(); // Trim() ����
                int itemCount = int.Parse(details[1].Trim()); // Trim() ���� �� int ��ȯ�� ����
                Debug.Log("Item Name: " + itemName + ", Item Count: " + itemCount);

                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject player in players) {
                    PhotonView pv = player.GetComponent<PhotonView>();
                    if (pv != null && pv.IsMine) {
                        // �ش� �÷��̾�� ������ �ν��Ͻ�ȭ �� ����
                        Vector3 spawnPosition = player.transform.position + new Vector3(1, 0, 0); // �÷��̾� ���� ������ ����
                        GameObject itemInstance = Pooling.instance.GetObject(itemName, spawnPosition);
                        itemInstance.GetComponent<ItemPickUp>().itemCount = itemCount;
                        // �������� �÷��̾ �ֿ� �κ��丮�� �߰�
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
            isItemInfoSaved = false;  // �÷��̾ ������ �÷��� ����
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
            // ���� �÷��̾ nextStageZone�� ������ �� ���� �ε��մϴ�.
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