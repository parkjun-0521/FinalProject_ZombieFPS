using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LeaveRoomButton : MonoBehaviour
{
    public Button leaveButton;

    void Start() {
        leaveButton.onClick.AddListener(OnLeaveRoom);
    }

    void OnLeaveRoom() {
        StartCoroutine(DeleteItemData(PhotonNetwork.NickName));
        AudioManager.Instance.PlayBgm(false, ScenesManagerment.Instance.stageCount);
        ScenesManagerment.Instance.stageCount = 0;
        ScenesManagerment.Instance.playerCount = 0;
        NetworkManager.Instance.LeaveRoom();
    }

    IEnumerator DeleteItemData(string userID)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", userID);  

        using (UnityWebRequest www = UnityWebRequest.Post(URLs.ItemDeleteURL, form)) {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.LogError("Error while sending delete item request: " + www.error);
            }
            else {
                Debug.Log("Delete item request sent successfully!");
            }
        }
    }
}
