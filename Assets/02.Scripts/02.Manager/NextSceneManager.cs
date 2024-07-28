using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneManager : MonoBehaviour
{
    public int stageCount = 0;

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
            if (ScenesManagerment.Instance.playerCount == PhotonNetwork.CurrentRoom.PlayerCount) {
                if (stageCount == 0) {
                    stageCount += 1;
                    PhotonNetwork.LoadLevel("03.MainGameScene_1");
                }
                else if (stageCount == 1) {
                    stageCount += 1;
                    //PhotonNetwork.LoadLevel("03.MainGameScene_2");
                }
                else if (stageCount == 2) {
                    //PhotonNetwork.LoadLevel("04.Ending");
                }
            }
        }
    }
}
