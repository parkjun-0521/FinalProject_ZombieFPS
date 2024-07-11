using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPunCallbacks {
    public GameObject firstPersonModel;
    public GameObject thirdPersonModel;
    public Camera firstPersonCamera;

    void Start() {
        if (photonView.IsMine) {
            // 로컬 플레이어인 경우
            firstPersonModel.SetActive(true);
            thirdPersonModel.SetActive(false);
            firstPersonCamera.enabled = true;
        }
        else {
            // 원격 플레이어인 경우
            firstPersonModel.SetActive(false);
            thirdPersonModel.SetActive(true);
            firstPersonCamera.enabled = false;
        }
    }
}