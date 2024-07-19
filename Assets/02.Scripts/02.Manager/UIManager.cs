using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InputKeyManager;

public class UIManager : MonoBehaviourPun
{
    public GameObject SettingUI;
    public bool isCountSetting;

    void Update(){
        if (photonView.IsMine) {
            Setting();
        }
    }

    public void Setting()
    {
        if (photonView.IsMine) {
            Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
            if (!isCountSetting && Input.GetKeyDown(InputKeyManager.instance.GetKeyCode(KeyCodeTypes.Setting))) {
                SettingUI.SetActive(true);
                isCountSetting = true;
                player.cursorLocked = true;
                player.ToggleCursor();
            }
            else if (isCountSetting && Input.GetKeyDown(InputKeyManager.instance.GetKeyCode(KeyCodeTypes.Setting))) {
                SettingUI.SetActive(false);
                isCountSetting = false;
                player.cursorLocked = false;
                player.ToggleCursor();
            }
        }
    }

    public void SettingExit()
    {
        Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
        SettingUI.SetActive(false);
        isCountSetting = false;
        player.cursorLocked = false;
        player.ToggleCursor();
    }
}
