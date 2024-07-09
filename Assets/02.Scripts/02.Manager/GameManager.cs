using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks {
    void Start() {
        if (PhotonNetwork.IsConnectedAndReady) {
            bool playerExists = false;

            // 현재 씬에 플레이어가 이미 존재하는지 확인
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player")) {
                if (obj.GetComponent<PhotonView>() != null && obj.GetComponent<PhotonView>().IsMine) {
                    playerExists = true;
                    break;
                }
            }

            // 플레이어가 존재하지 않는 경우에만 생성
            if (!playerExists) {
                PhotonNetwork.Instantiate("PlayerPrefab", Vector3.zero, Quaternion.identity);
            }
        }
    }
}
