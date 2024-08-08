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

    // 소리 셋팅 
    public Slider bgmSlider;
    public Slider sfxSlider;

    // 감도 셋팅 
    public Slider xSensitivity;
    public Slider ySensitivity;

    // 체력바 
    public Text[] nickName;
    public Slider[] hpBar; 

    //장전 이미지
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
        Setting();          // 설정 UI 활성화 비활성화 
    }

    // 설정창 활성화/비활성화
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
                if (otherIndex < 4) // 최대 3명의 플레이어까지만 처리 (1, 2, 3번째 칸)
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
