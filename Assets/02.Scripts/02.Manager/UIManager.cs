using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static InputKeyManager;

public class UIManager : MonoBehaviourPun
{
    public static UIManager Instance;

    // 설정 UI 
    public GameObject SettingUI;
    public bool isCountSetting;

    // 무기/아이템 UI 
    public Image[] weaponItem;

    // 아이템 개수
    public Text CurBulletCount;
    public Text totalBulletCount;
    public Text totalGranedeCount;
    public Text totalHealCount;

    // 에이밍 버튼 
    public Button[] aimingImage;

    // 체력바 
    public Text nickName1;
    public Text nickName2;
    public Text nickName3;
    public Text nickName4;
    public Slider hpBar; 
    public Slider hpBar1; 
    public Slider hpBar2; 
    public Slider hpBar3;

    //장전 이미지
    public Image reloadImage;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (photonView.IsMine) {
            for (int i = 0; i < weaponItem.Length; i++) {
                weaponItem[i].color = new Color(1, 1, 1, 0.2f);
            }
            UIManager.Instance.CurBulletCount.text = "0";
            UIManager.Instance.totalBulletCount.text = "0";
            UIManager.Instance.totalGranedeCount.text = "0";
            UIManager.Instance.totalHealCount.text = "0";
        }
    }

    void Update(){
        Setting();          // 설정 UI 활성화 비활성화 
    }

    // 설정창 활성화/비활성화
    public void Setting() {
        Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
        Debug.Log("설정창 " + player.PV.ViewID);
        if (player != null) {
            if (player.inventory.activeSelf && Input.GetKeyDown(InputKeyManager.instance.GetKeyCode(KeyCodeTypes.Setting))) {
                return;
            }
            else if (!isCountSetting && Input.GetKeyDown(InputKeyManager.instance.GetKeyCode(KeyCodeTypes.Setting))) {
                Debug.Log("설정창1 " + player.PV.ViewID);
                SettingUI.SetActive(true);
                isCountSetting = true;
                player.cursorLocked = true;
                player.ToggleCursor();
            }
            else if (isCountSetting && Input.GetKeyDown(InputKeyManager.instance.GetKeyCode(KeyCodeTypes.Setting))) {
                Debug.Log("설정창2 " + player.PV.ViewID);
                SettingUI.SetActive(false);
                isCountSetting = false;
                player.cursorLocked = false;
                player.ToggleCursor();
            }
        }

    }

    // 설정창 나가기 버튼
    public void SettingExit() {
        Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
        SettingUI.SetActive(false);
        isCountSetting = false;
        player.cursorLocked = false;
        player.ToggleCursor();

    }

    public void UpdateTotalBulletCount(int newBulletCount)
    {
        totalBulletCount.text = newBulletCount.ToString();
    }

    public void UpdateTotalGrenadeCount(int newGrenadeCount)
    {
        totalGranedeCount.text = newGrenadeCount.ToString();
    }

    public void UpdateTotalHealCount(int newHealCount)
    {
        totalHealCount.text = newHealCount.ToString();
    }

    public void AimingChange(Button buttonId)
    {
        Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
        Image[] images = buttonId.GetComponentsInChildren<Image>();

        if (images.Length > 1 && player.aiming != null) {
            player.aiming.sprite = images[1].sprite; // 첫 번째 자식의 이미지를 사용
        }
    }

    public void UpdatePlayerHealthBar(string nickName, int viewID ,float healthPercent)
    {
        if (PhotonNetwork.LocalPlayer.NickName == nickName) {
            // 내 체력바 업데이트
            nickName1.text = nickName;
            hpBar.value = healthPercent;
        }
        else {
            // 다른 플레이어의 체력바 업데이트
            var otherPlayers = PhotonNetwork.PlayerListOthers;

            for (int i = 0; i < otherPlayers.Length; i++) {
                if (otherPlayers[i].NickName == nickName) {
                    switch (i) {
                        case 0:
                            nickName2.text = nickName;
                            hpBar1.value = healthPercent;
                            break;
                        case 1:
                            nickName3.text = nickName;
                            hpBar2.value = healthPercent;
                            break;
                        case 2:
                            nickName4.text = nickName;
                            hpBar3.value = healthPercent;
                            break;
                    }
                    break;
                }
            }
        }
    }
}
