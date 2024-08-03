using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NextSceneManager : MonoBehaviourPunCallbacks {
    public static NextSceneManager Instance;
    public bool isQuest1 = false;
    public bool isQuest2 = false;
    public bool isQuest3 = false;

    bool isItemInfoSaved = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        isItemInfoSaved = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) {
            ScenesManagerment.Instance.playerCount += 1;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) {
            ScenesManagerment.Instance.playerCount -= 1;
            isItemInfoSaved = false;  // �÷��̾ ������ �÷��� ����
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (PhotonNetwork.IsMasterClient) {
            // ��� �÷��̾ nextStageZone�� ������ �� ���� �ε��մϴ�.
            if (ScenesManagerment.Instance.playerCount == PhotonNetwork.CurrentRoom.PlayerCount) {
                if (ScenesManagerment.Instance.stageCount == 0 && isQuest1) {
                    if (!isItemInfoSaved) {
                        isItemInfoSaved = true; // �����͸� �� ���� �����ϵ��� �÷��� ����
                        AudioManager.Instance.PlayBgm(false, ScenesManagerment.Instance.stageCount);
                        if (other.CompareTag("Player")) {
                            Debug.Log("????");
                            Inventory inventory = other.gameObject.GetComponentInChildren<Inventory>();
                            inventory.AllItemInfo();
                        }

                        PhotonNetwork.LoadLevel("03.MainGameScene_1");
                        ScenesManagerment.Instance.stageCount += 1;
                        ScenesManagerment.Instance.playerCount = 0;

                        AudioManager.Instance.PlayBgm(true, ScenesManagerment.Instance.stageCount);
                    }
                }
                else if (ScenesManagerment.Instance.stageCount == 1 && isQuest2) {
                    if (!isItemInfoSaved) {
                        isItemInfoSaved = true; // �����͸� �� ���� �����ϵ��� �÷��� ����
                        AudioManager.Instance.PlayBgm(false, ScenesManagerment.Instance.stageCount);
                        if (other.CompareTag("Player")) {
                            Inventory inventory = other.gameObject.GetComponentInChildren<Inventory>();
                            inventory.AllItemInfo();
                        }

                        PhotonNetwork.LoadLevel("03.MainGameScene_2");
                        ScenesManagerment.Instance.stageCount += 1;
                        ScenesManagerment.Instance.playerCount = 0;

                        AudioManager.Instance.PlayBgm(true, ScenesManagerment.Instance.stageCount);
                    }
                }
                else if (ScenesManagerment.Instance.stageCount == 2 && isQuest3) {      // ���� ������ ������ ������ isQUest3 �� true �ٲ��ְ� 
                    // ���� �� �������� �ִϸ��̼����� �ѹ� �� �����ְ� 
                    // ���� �������� ��ư �ϳ� 
                    // �泪���� ��ư���� ���� �κ�� �̵��ϵ��� ����
                    if (!isItemInfoSaved) {
                        isItemInfoSaved = true; // �����͸� �� ���� �����ϵ��� �÷��� ����
                        AudioManager.Instance.PlayBgm(false, ScenesManagerment.Instance.stageCount);
                        PhotonNetwork.LoadLevel("04.Ending");
                        ScenesManagerment.Instance.playerCount = 0;
                    }
                }
            }
        }
        else {
            if (!isItemInfoSaved) {
                isItemInfoSaved = true;
                if (ScenesManagerment.Instance.playerCount == PhotonNetwork.CurrentRoom.PlayerCount) {
                    AudioManager.Instance.PlayBgm(false, ScenesManagerment.Instance.stageCount);
                    if (other.CompareTag("Player")) {
                        Debug.Log("????");
                        Inventory inventory = other.gameObject.GetComponentInChildren<Inventory>();
                        inventory.AllItemInfo();
                    }
                    AudioManager.Instance.PlayBgm(true, ScenesManagerment.Instance.stageCount);
                }
            }
        }
    }

    IEnumerator DeleteItemData(string userID)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", userID);

        using (UnityWebRequest www = UnityWebRequest.Post(URLs.ItemDeleteURL, form)) {
            yield return www.SendWebRequest();
        }
    }
}