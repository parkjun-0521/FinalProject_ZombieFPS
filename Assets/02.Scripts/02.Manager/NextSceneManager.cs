using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneManager : MonoBehaviourPunCallbacks {
    public static NextSceneManager Instance;
    public bool isQuest1 = false;
    public bool isQuest2 = false;
    public bool isQuest3 = false;

    void Awake() {
        Instance = this;
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
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (PhotonNetwork.IsMasterClient) {
            // 모든 플레이어가 nextStageZone에 들어왔을 때 씬을 로드합니다.
            if (ScenesManagerment.Instance.playerCount == PhotonNetwork.CurrentRoom.PlayerCount) {
                if (ScenesManagerment.Instance.stageCount == 0 && isQuest1) {
                    PhotonNetwork.LoadLevel("03.MainGameScene_1");
                    ScenesManagerment.Instance.stageCount += 1;
                    ScenesManagerment.Instance.playerCount = 0;
                }
                else if (ScenesManagerment.Instance.stageCount == 1 && isQuest2) {
                    PhotonNetwork.LoadLevel("03.MainGameScene_2");
                    ScenesManagerment.Instance.stageCount += 1;
                    ScenesManagerment.Instance.playerCount = 0;
                }
                else if (ScenesManagerment.Instance.stageCount == 2 && isQuest3) {
                    //PhotonNetwork.LoadLevel("04.Ending");
                    ScenesManagerment.Instance.playerCount = 0;
                }
            }
        }
    }
}
