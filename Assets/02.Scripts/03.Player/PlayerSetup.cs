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
            // 로컬 플레이어인 경우
            SetLayerRecursively(firstPersonModel, LayerMask.NameToLayer("RemotePlayer"));
            SetLayerRecursively(thirdPersonModel, LayerMask.NameToLayer("LocalPlayer"));

            firstPersonCamera.enabled = true;
            // 카메라가 "RemotePlayer" 레이어를 렌더링하지 않도록 설정
            firstPersonCamera.cullingMask = ~(1 << LayerMask.NameToLayer("LocalPlayer"));
        }
        else {
            // 원격 플레이어인 경우
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