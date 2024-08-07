using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameReadyorStart : MonoBehaviourPun
{
    public Text roomName;       // 방 이름 
    public Text[] userName;     // 유저 이름 
    public Button[] resignBtn;  // 강퇴 버튼

    public Button exitBtn;
    public Button startBtn;
    public bool isReady;

    public GameObject startLading;


    void Start() {
        // 방 이름 설정 
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        ReadyButton();
    }

    [PunRPC]
    public void UpdateUserNameRPC() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int index = 1;
        foreach (GameObject player in players) {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null) {
                Photon.Realtime.Player photonPlayer = pv.Owner;
                if (photonPlayer.IsMasterClient) {
                    userName[0].text = photonPlayer.NickName;
                }
                else {
                    if (index < userName.Length) { // 배열 범위를 벗어나지 않도록
                        userName[index].text = photonPlayer.NickName;
                        index++;
                    }
                }
            }
        }
    }

    // 방 입장시 버튼 Text 변경 
    public void ReadyButton() {
        if (PhotonNetwork.IsMasterClient) {
            startBtn.GetComponentInChildren<Text>().text = "게임시작";
        }
        else {
            startBtn.GetComponentInChildren<Text>().text = "준비";
        }
    }

    // 방 나가기 이벤트 
    public void ExitRoom() {
        Cursor.visible = true;

        // 방장이 방을 나가면 전원 방 나가짐 
        if(PhotonNetwork.IsMasterClient) {
            photonView.RPC("AllLeaveRoom", RpcTarget.All);
        }

        if (!isReady) {
            // 내 이름을 배열에서 지워주고 초기화 해주야함 RPC로 
            photonView.RPC("OffRemoveName", RpcTarget.OthersBuffered, PhotonNetwork.NickName);

            PhotonNetwork.LeaveRoom();
        }
        else {
            isReady = false;
            // 내 이름을 배열에서 지워주고 초기화 해주야함 RPC로 
            photonView.RPC("OnRemoveName", RpcTarget.OthersBuffered, PhotonNetwork.NickName);

            PhotonNetwork.LeaveRoom();
        }
    }

    [PunRPC]
    public void AllLeaveRoom() {
        ScenesManagerment.Instance.readyUserCount = 0;
        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    public void OnRemoveName(string nickname) {
        ScenesManagerment.Instance.readyUserCount -= 1;
        foreach(Text name in userName) {
            if(name.text == nickname) {
                name.text = null;
                name.text = "-----";
            }
        }
    }

    [PunRPC]
    public void OffRemoveName( string nickname ) {
        foreach (Text name in userName) {
            if (name.text == nickname) {
                name.text = null;
                name.text = "-----";
            }
        }
    }

    // 게임 시작 버튼의 이벤트 
    public void OnGameStart() {
        if(PhotonNetwork.IsMasterClient) {
            if (PhotonNetwork.CurrentRoom.PlayerCount - 1 == ScenesManagerment.Instance.readyUserCount) {
                Debug.Log("게임을 시작합니다.");
                photonView.RPC("GameStart", RpcTarget.AllBuffered);
            }
            else {
                Debug.Log("방 인원 전체가 준비가 되어있지 않습니다.");
            }
        }
        else {
            if (!isReady) {
                isReady = true;               
                foreach (Text name in userName) {
                    if (PhotonNetwork.NickName == name.text) {
                        photonView.RPC("GameReady", RpcTarget.OthersBuffered, PhotonNetwork.NickName);
                        name.color = Color.green;
                    }
                }
            }
            else {
                isReady = false;
                foreach (Text name in userName) {
                    if (PhotonNetwork.NickName == name.text) {
                        photonView.RPC("GameReadyCancel", RpcTarget.OthersBuffered, PhotonNetwork.NickName);
                        name.color = Color.white;
                    }
                }
            }
        }
    }

    [PunRPC]
    public void GameStart() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        gameObject.SetActive(false);
        startLading.SetActive(true);
    }

    [PunRPC]
    public void GameReady(string nickName ) {
        ScenesManagerment.Instance.readyUserCount += 1;
        foreach (Text name in userName) {
            if (nickName == name.text) {
                name.color = Color.green;
            }
        }
    }

    [PunRPC]
    public void GameReadyCancel( string nickName ) {
        ScenesManagerment.Instance.readyUserCount -= 1;
        foreach (Text name in userName) {
            if (nickName == name.text) {
                name.color = Color.white;
            }
        }
    }

}
