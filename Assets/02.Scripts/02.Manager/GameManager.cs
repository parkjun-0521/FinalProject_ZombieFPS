using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks {

    public Text[] chatText;
    public InputField chatInput;
    public Button chatButton;

    void Start() {
        if (PhotonNetwork.IsConnectedAndReady) {
            bool playerExists = false;

            // ���� ���� �÷��̾ �̹� �����ϴ��� Ȯ��
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player")) {
                if (obj.GetComponent<PhotonView>() != null && obj.GetComponent<PhotonView>().IsMine) {
                    playerExists = true;
                    break;
                }
            }

            // �÷��̾ �������� �ʴ� ��쿡�� ����
            if (!playerExists) {
                PhotonNetwork.Instantiate("PlayerPrefab", Vector3.zero, Quaternion.identity);
            }
        }

        NetworkManager.Instance.chatText = chatText;
        NetworkManager.Instance.chatInput = chatInput;
        chatButton.onClick.AddListener(() => NetworkManager.Instance.Send());
    }
}
