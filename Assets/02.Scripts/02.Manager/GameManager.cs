using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks {
    void Start() {
        if (PhotonNetwork.IsConnectedAndReady) {
            PhotonNetwork.Instantiate("PlayerPrefab", Vector3.zero, Quaternion.identity);
        }
    }
}
