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

    // �Ҹ� ���� 
    public Slider bgmSlider;
    public Slider sfxSlider;

    // ���� ���� 
    public Slider xSensitivity;
    public Slider ySensitivity;

    // ü�¹� 
    public Text[] nickName;
    public Slider[] hpBar; 

    //���� �̹���
    public Image reloadImage;

    GameObject[] players;
    Player player;
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
        players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log(players.Length);
        FindLocalPlayer();
        for (int i = 0; i < weaponItem.Length; i++) {
            weaponItem[i].color = new Color(1, 1, 1, 0.2f);
        }
        UIManager.Instance.CurBulletCount.text = "0";
        UIManager.Instance.totalBulletCount.text = "0";
        UIManager.Instance.totalGranedeCount.text = "0";
        UIManager.Instance.totalHealCount.text = "0";

        bgmSlider.value = AudioManager.Instance.bgmVolume;
        sfxSlider.value = AudioManager.Instance.sfxVolume;
        bgmSlider.onValueChanged.AddListener((value) => { AudioManager.Instance.SetBgmVolume(value); });
        sfxSlider.onValueChanged.AddListener((value) => { AudioManager.Instance.SetSfxVolume(value); });

    }

    void FindLocalPlayer() {
        foreach (GameObject obj in players) {
            PhotonView pv = obj.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine) {
                player = obj.GetComponent<Player>();
                Debug.Log(player.name + pv.ViewID);
                break;
            }
        }
    }

    void Update(){
        Setting();          // ���� UI Ȱ��ȭ ��Ȱ��ȭ 
    }

    // ����â Ȱ��ȭ/��Ȱ��ȭ
    public void Setting() {
        if (player != null) {
            if (player.inventory.activeSelf && Input.GetKeyDown(InputKeyManager.instance.GetKeyCode(KeyCodeTypes.Setting))) {
                return;
            }
            else if (!isCountSetting && Input.GetKeyDown(InputKeyManager.instance.GetKeyCode(KeyCodeTypes.Setting))) {
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
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        foreach (var player in players) {
            Debug.Log(player.NickName);
        }

        for (int i = 0; i < players.Length; i++) {
            if (players[i].NickName == PhotonNetwork.LocalPlayer.NickName) {
                this.nickName[0].text = players[i].NickName;
                hpBar[0].value = (this.player.Hp / this.player.maxHp) * 100;
                break;
            }
        }

        int otherIndex = 1;
        for (int i = 0; i < players.Length; i++) {
            if (players[i].NickName != PhotonNetwork.LocalPlayer.NickName) {
                if (otherIndex < 4) // �ִ� 3���� �÷��̾������ ó�� (1, 2, 3��° ĭ)
                {
                    this.nickName[otherIndex].text = players[i].NickName;
                    hpBar[otherIndex].value =players[i].NickName ==  nickName ? healthPercent : hpBar[otherIndex].value;
                    otherIndex++;
                }
            }
        }
    }

    public void OnBgmVolumeChanged()
    {
        float volume = bgmSlider.value;
        AudioManager.Instance.SetBgmVolume(volume);
    }

    public void OnSfxVolumeChanged()
    {
        float volume = sfxSlider.value;
        AudioManager.Instance.SetSfxVolume(volume);
    }

    public void OnSensitivityX()
    {
        float x = xSensitivity.value;
        RotateToMouse player = GameObject.FindWithTag("Player").GetComponent<RotateToMouse>();
        player.SetSensitivityX(x);
    }
    public void OnSensitivityY()
    {
        float y = ySensitivity.value;
        RotateToMouse player = GameObject.FindWithTag("Player").GetComponent<RotateToMouse>();
        player.SetSensitivityY(y);
    }
}
