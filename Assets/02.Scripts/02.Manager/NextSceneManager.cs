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
            isItemInfoSaved = false;  // 플레이어가 나가면 플래그 리셋
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (PhotonNetwork.IsMasterClient) {
            // 모든 플레이어가 nextStageZone에 들어왔을 때 씬을 로드합니다.
            if (ScenesManagerment.Instance.playerCount == PhotonNetwork.CurrentRoom.PlayerCount) {
                if (ScenesManagerment.Instance.stageCount == 0 && isQuest1) {
                    if (!isItemInfoSaved) {
                        isItemInfoSaved = true; // 데이터를 한 번만 저장하도록 플래그 설정
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
                        isItemInfoSaved = true; // 데이터를 한 번만 저장하도록 플래그 설정
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
                else if (ScenesManagerment.Instance.stageCount == 2 && isQuest3) {      // 가장 마지막 보스가 죽으면 isQUest3 을 true 바꿔주고 
                    // 엔딩 씬 마지막에 애니메이션으로 한번 쫙 보여주고 
                    // 가장 마지막에 버튼 하나 
                    // 방나가기 버튼으로 만들어서 로비로 이동하도록 만듦
                    if (!isItemInfoSaved) {
                        isItemInfoSaved = true; // 데이터를 한 번만 저장하도록 플래그 설정
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