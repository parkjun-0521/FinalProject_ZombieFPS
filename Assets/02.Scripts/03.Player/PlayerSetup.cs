using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPunCallbacks {
    public GameObject localPlayerBody; // 로컬 플레이어의 바디 오브젝트

    void Start() {
        if (photonView.IsMine) {
            SetLayerRecursively(localPlayerBody, LayerMask.NameToLayer("LocalPlayer"));
            SetupLocalPlayer();
        }
        else {
            // 원격 플레이어의 카메라 비활성화
            Camera localCamera = GetComponentInChildren<Camera>();
            if (localCamera != null) {
                localCamera.gameObject.SetActive(false);
            }
        }
    }

    void SetLayerRecursively( GameObject obj, int newLayer ) {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void SetupLocalPlayer() {
        // 로컬 플레이어 카메라 설정
        Camera localCamera = GetComponentInChildren<Camera>();
        if (localCamera != null) {
            localCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("LocalPlayer"));
        }
    }
}