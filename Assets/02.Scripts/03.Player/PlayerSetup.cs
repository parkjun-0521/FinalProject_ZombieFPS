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
            // ���� �÷��̾��� ���
            firstPersonModel.SetActive(true);
            thirdPersonModel.SetActive(false);
            firstPersonCamera.enabled = true;
        }
        else {
            // ���� �÷��̾��� ���
            firstPersonModel.SetActive(false);
            thirdPersonModel.SetActive(true);
            firstPersonCamera.enabled = false;
        }
    }
}