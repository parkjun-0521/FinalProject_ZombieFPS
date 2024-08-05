using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NextSceneManager : MonoBehaviourPunCallbacks {
    public static NextSceneManager Instance;
    public bool isQuest1 = false;
    public bool isQuest2 = false;
    public bool isQuest3 = false;

    public bool isItemInfoSaved = false;
    public bool isSceneChange = false;

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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "02.LobbyScene") {
            AudioManager.Instance.PlayBgm(true, ScenesManagerment.Instance.stageCount);
            isItemInfoSaved = false;
        }
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
            if (ScenesManagerment.Instance.stageCount == 0) {
                StopCoroutine(SenecChange1());
            }
            else if (ScenesManagerment.Instance.stageCount == 1) {
                StopCoroutine(SenecChange2());
            }
            playersInTrigger.Remove(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isItemInfoSaved) {
            foreach (GameObject player in playersInTrigger) {
                PhotonView photonView = player.GetComponent<PhotonView>();
                if (photonView != null && photonView.IsMine && other.CompareTag("Player")) {
                    isItemInfoSaved = true;          
                    StartCoroutine(DeleteItemData(PhotonNetwork.NickName, player));
                }
            }

        }
        if (PhotonNetwork.IsMasterClient) {
            // ��� �÷��̾ nextStageZone�� ������ �� ���� �ε��մϴ�.
            if (ScenesManagerment.Instance.playerCount == PhotonNetwork.CurrentRoom.PlayerCount) {
                if (ScenesManagerment.Instance.stageCount == 0 && isQuest1) {
                    AudioManager.Instance.PlayBgm(false, ScenesManagerment.Instance.stageCount);
                    if (!isSceneChange) {
                        StartCoroutine(SenecChange1());
                    }
                }
                else if (ScenesManagerment.Instance.stageCount == 1 && isQuest2) {
                    AudioManager.Instance.PlayBgm(false, ScenesManagerment.Instance.stageCount);
                    if (!isSceneChange) {
                        StartCoroutine(SenecChange2());
                    }
                }
                else if (ScenesManagerment.Instance.stageCount == 2 && isQuest3) {      // ���� ������ ������ ������ isQUest3 �� true �ٲ��ְ� 
                                                                                        // ���� �� �������� �ִϸ��̼����� �ѹ� �� �����ְ� 
                                                                                        // ���� �������� ��ư �ϳ� 
                                                                                        // �泪���� ��ư���� ���� �κ�� �̵��ϵ��� ����
                    AudioManager.Instance.PlayBgm(false, ScenesManagerment.Instance.stageCount);
                    PhotonNetwork.LoadLevel("04.Ending");
                    ScenesManagerment.Instance.stageCount += 1;
                    ScenesManagerment.Instance.playerCount = 0;
                }
            }
        }
    }

    IEnumerator SenecChange1()
    {
        isSceneChange = true;
        yield return new WaitForSeconds(5f);
        PhotonNetwork.LoadLevel("03.MainGameScene_1");
        ScenesManagerment.Instance.stageCount += 1;
        ScenesManagerment.Instance.playerCount = 0;
    }

    IEnumerator SenecChange2()
    {
        isSceneChange = true;
        yield return new WaitForSeconds(5f);
        PhotonNetwork.LoadLevel("03.MainGameScene_2");
        ScenesManagerment.Instance.stageCount += 1;
        ScenesManagerment.Instance.playerCount = 0;
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