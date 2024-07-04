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
        NetworkManager.Instance.LeaveRoom();
    }
}
