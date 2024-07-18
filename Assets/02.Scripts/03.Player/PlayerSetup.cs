using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPunCallbacks {

    public GameObject firstPersonModel;
    public GameObject thirdPersonModel;
    public Camera firstPersonCamera;

    void Start()
    {
        if (photonView.IsMine) {
            // ���� �÷��̾��� ���
            SetLayerRecursively(firstPersonModel, LayerMask.NameToLayer("RemotePlayer"));
            SetLayerRecursively(thirdPersonModel, LayerMask.NameToLayer("LocalPlayer"));

            firstPersonCamera.enabled = true;
            // ī�޶� "RemotePlayer" ���̾ ���������� �ʵ��� ����
            firstPersonCamera.cullingMask = ~(1 << LayerMask.NameToLayer("LocalPlayer"));
        }
        else {
            // ���� �÷��̾��� ���
            SetLayerRecursively(firstPersonModel, LayerMask.NameToLayer("LocalPlayer"));
            SetLayerRecursively(thirdPersonModel, LayerMask.NameToLayer("RemotePlayer"));
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

}