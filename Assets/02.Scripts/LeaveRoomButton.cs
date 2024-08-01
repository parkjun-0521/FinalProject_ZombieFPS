using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaveRoomButton : MonoBehaviour
{
    public Button leaveButton;

    void Start() {
        leaveButton.onClick.AddListener(OnLeaveRoom);
    }

    void OnLeaveRoom() {
        AudioManager.Instance.PlayBgm(false, ScenesManagerment.Instance.stageCount);
        NetworkManager.Instance.LeaveRoom();
    }
}
