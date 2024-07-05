using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPunCallbacks {
    public GameObject localPlayerBody; // ���� �÷��̾��� �ٵ� ������Ʈ

    void Start() {
        if (photonView.IsMine) {
            SetLayerRecursively(localPlayerBody, LayerMask.NameToLayer("LocalPlayer"));
            SetupLocalPlayer();
        }
        else {
            // ���� �÷��̾��� ī�޶� ��Ȱ��ȭ
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
        // ���� �÷��̾� ī�޶� ����
        Camera localCamera = GetComponentInChildren<Camera>();
        if (localCamera != null) {
            localCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("LocalPlayer"));
        }
    }
}