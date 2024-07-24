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

    // ���� UI 
    public GameObject SettingUI;
    public bool isCountSetting;

    // ����/������ UI 
    public Image[] weaponItem;

    // ������ ����
    public Text CurBulletCount;
    public Text totalBulletCount;
    public Text totalGranedeCount;
    public Text totalHealCount;

    // ���̹� ��ư 
    public Button[] aimingImage;

    // ü�¹� 
    public Text nickName1;
    public Text nickName2;
    public Text nickName3;
    public Text nickName4;
    public Slider hpBar; 
    public Slider hpBar1; 
    public Slider hpBar2; 
    public Slider hpBar3;

    //���� �̹���
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
        Setting();          // ���� UI Ȱ��ȭ ��Ȱ��ȭ 
    }

    // ����â Ȱ��ȭ/��Ȱ��ȭ
    public void Setting() {
        Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
        Debug.Log("����â " + player.PV.ViewID);
        if (player != null) {
            if (player.inventory.activeSelf && Input.GetKeyDown(InputKeyManager.instance.GetKeyCode(KeyCodeTypes.Setting))) {
                return;
            }
            else if (!isCountSetting && Input.GetKeyDown(InputKeyManager.instance.GetKeyCode(KeyCodeTypes.Setting))) {
                Debug.Log("����â1 " + player.PV.ViewID);
                SettingUI.SetActive(true);
                isCountSetting = true;
                player.cursorLocked = true;
                player.ToggleCursor();
            }
            else if (isCountSetting && Input.GetKeyDown(InputKeyManager.instance.GetKeyCode(KeyCodeTypes.Setting))) {
                Debug.Log("����â2 " + player.PV.ViewID);
                SettingUI.SetActive(false);
                isCountSetting = false;
                player.cursorLocked = false;
                player.ToggleCursor();
            }
        }

    }

    // ����â ������ ��ư
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
            player.aiming.sprite = images[1].sprite; // ù ��° �ڽ��� �̹����� ���
        }
    }

    public void UpdatePlayerHealthBar(string nickName, int viewID ,float healthPercent)
    {
        if (PhotonNetwork.LocalPlayer.NickName == nickName) {
            // �� ü�¹� ������Ʈ
            nickName1.text = nickName;
            hpBar.value = healthPercent;
        }
        else {
            // �ٸ� �÷��̾��� ü�¹� ������Ʈ
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
