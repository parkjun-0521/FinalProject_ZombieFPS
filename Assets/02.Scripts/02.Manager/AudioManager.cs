using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    public AudioClip[] bgmClips;    // ����� Ŭ�� 
    public float bgmVolume;         // ����� ���� 
    AudioSource bgmPlayer;          // 

    [Header("SFX")]
    public AudioClip[] sfxClips;    // ȿ���� Ŭ�� 
    public float sfxVolume;         // ȿ���� ���� 
    public int channels;            // ä�� �� 
    AudioSource[] sfxPlayers;
    int channelIndex;               //  ä�� ���ؽ� 

    public enum Sfx { 
        MediKit,                // ���� ���� �Ҹ�
        Player_Death,           // �÷��̾� ���� 
        Player_death_BGM,       // ���� ���� BGM 
        Player_door,            // ������ �Ҹ� 
        Player_explosion,       // ����ź ���� 
        Player_granede,         // ����ź ������ �Ҹ� 
        Player_gun2,            // �� �Ҹ�     
        Player_help,            // ���� �Ҹ� 
        Player_hurt,            // �ǰ� �Ҹ� 
        Player_item,            // ������ ���� �Ҹ� 
        Player_jump,            // ���� �Ҹ� 
        Player_knife = 12,      // 11,12 Į �Ҹ� ( ���� ) 
        Player_run2,            // �޸��� 
        Player_walk3,           // �ȱ� 
        UI_Button,              // UI ��ư 
        Zombie_attack,          // ���� ���� 0
        Zombie_attack1,         // ���� ���� 1
        Zombie_attack2,         // ���� ���� 2
        Zombie_attack3,         // ���� ���� 3
        Zombie_attack4,         // ���� ���� 4
        Zombie_attack5,         // ���� ���� 5
        Zombie_attack6,         // ���� ���� 6
        Zombie_explosion = 24,  // 24, 25 ���� ������ �Ҹ� ���� 
        Zombie_hurt = 26,       // 26, 27 ���� �ǰ� �Ҹ� ���� 
        Zombie_run,             // ���� �޸��� 
        Zombie_walk,            // ���� �ȱ� 
        Player_reload3,         // �÷��̾� ����
        Player_Door,            // �÷��̾� ��
        Bandage,                // �÷��̾� �ش� ���� �Ҹ� 
        BossDie1,               // ���� �״� �Ҹ� 1
        BossDie2,               // ���� �״� �Ҹ� 2
        BossHit = 37,           // ���� �ǰ� �Ҹ� 0~4
        BossRun1,               // ���� �߰� �Ҹ� 1
        BossRun2,               // ���� �߰� �Ҹ� 2
        BossWalk1,              // ���� �ȴ� �Ҹ� 1
        BossWalk2,              // ���� �ȴ� �Ҹ� 2
        GrenadePinRull,         // �÷��̾� ����ź ����
        Medikit2,               // �÷��̾� �ش� ���� �Ҹ� 
        Zombie_dead1 = 47,      // ���� �״� �Ҹ� 
    }

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
            return;
        }

        Init();
    }

    void Init()
    {
        // ����� 
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClips[0];

        // ȿ���� 
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for(int i = 0; i < sfxPlayers.Length; i++) {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].volume = sfxVolume;
        }
    }

    public void PlayBgm(bool isPlay, int stageCount)
    {
        // ���� AudioManager.Instance.PlayBgm(true); // ������ false
        if (isPlay) {
            if (stageCount == 0)
                bgmPlayer.clip = bgmClips[0];
            else if (stageCount == 1) {
                bgmPlayer.clip = bgmClips[1];
                SetBgmVolume(0.8f);
            }
            else if (stageCount == 2)
                bgmPlayer.clip = bgmClips[2];
            bgmPlayer.Play();
        }
        else {
            bgmPlayer.Stop();
        }
    }

    public void PlayerSfx(Sfx sfx)
    {
        for(int i = 0; i < sfxPlayers.Length; i++) {
            int loopIndex = (i + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            int randIndex = 0;
            if (sfx == Sfx.Player_knife || sfx == Sfx.Zombie_explosion || sfx == Sfx.Zombie_hurt) {
                randIndex = Random.Range(0, 2);
            }
            else if (sfx == Sfx.BossHit || sfx == Sfx.Zombie_dead1) {
                randIndex = Random.Range(0, 4);
            }

            channelIndex = loopIndex;

            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx - randIndex];
            sfxPlayers[loopIndex].Play();

            break;
        }
    }

    public bool IsPlaying( Sfx sfx ) {
        foreach (var player in sfxPlayers) {
            if (player.isPlaying && player.clip == sfxClips[(int)sfx]) {
                return true;
            }
        }
        return false;
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = volume;
        bgmPlayer.volume = bgmVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;
        foreach (AudioSource player in sfxPlayers) {
            player.volume = sfxVolume;
        }
    }
}
