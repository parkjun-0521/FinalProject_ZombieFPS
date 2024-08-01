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
            // ��� �÷��̾ nextStageZone�� ������ �� ���� �ε��մϴ�.
            Debug.Log(ScenesManagerment.Instance.playerCount);
            Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
            Debug.Log(ScenesManagerment.Instance.stageCount);
            if (ScenesManagerment.Instance.playerCount == PhotonNetwork.CurrentRoom.PlayerCount) {
                if (ScenesManagerment.Instance.stageCount == 0 && isQuest1) {
                    Debug.Log("�� �ȵ���?");
                    AudioManager.Instance.PlayBgm(false, ScenesManagerment.Instance.stageCount);
                    PhotonNetwork.LoadLevel("03.MainGameScene_1");
                    ScenesManagerment.Instance.stageCount += 1;
                    ScenesManagerment.Instance.playerCount = 0;
                    AudioManager.Instance.PlayBgm(true, ScenesManagerment.Instance.stageCount);
                }
                else if (ScenesManagerment.Instance.stageCount == 1 && isQuest2) {
                    AudioManager.Instance.PlayBgm(false, ScenesManagerment.Instance.stageCount);
                    PhotonNetwork.LoadLevel("03.MainGameScene_2");
                    ScenesManagerment.Instance.stageCount += 1;
                    ScenesManagerment.Instance.playerCount = 0;
                    AudioManager.Instance.PlayBgm(true, ScenesManagerment.Instance.stageCount);
                }
                else if (ScenesManagerment.Instance.stageCount == 2 && isQuest3) {      // ���� ������ ������ ������ isQUest3 �� true �ٲ��ְ� 
                    // ���� �� �������� �ִϸ��̼����� �ѹ� �� �����ְ� 
                    // ���� �������� ��ư �ϳ� 
                    // �泪���� ��ư���� ���� �κ�� �̵��ϵ��� ����
                    AudioManager.Instance.PlayBgm(false, ScenesManagerment.Instance.stageCount);
                    PhotonNetwork.LoadLevel("04.Ending");
                    ScenesManagerment.Instance.playerCount = 0;
                }
            }
        }
    }
}

