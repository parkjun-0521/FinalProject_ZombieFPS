using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    public AudioClip bgmClip;       // ����� Ŭ�� 
    public float bgmVolume;         // ����� ���� 
    AudioSource bgmPlayer;          // 

    [Header("SFX")]
    public AudioClip[] sfxClips;     // ȿ���� Ŭ�� 
    public float sfxVolume;         // ȿ���� ���� 
    public int channels;            // ä�� �� 
    AudioSource[] sfxPlayers;
    int channelIndex;               //  ä�� ���ؽ� 

    // Sfx Ŭ���� ���� �ְ� �� ���� ������� enum �ۼ� 
    public enum Sfx { 
        MediKit,                // ���� ���
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
        Zombie_dead1,           // ���� �״� �Ҹ� 
        Zombie_explosion = 25,  // 24, 25 ���� ������ �Ҹ� ���� 
        Zombie_hurt = 27,       // 26, 27 ���� �ǰ� �Ҹ� ���� 
        Zombie_run,             // ���� �޸��� 
        Zombie_walk,            // ���� �ȱ� 
        Player_reload3          // �÷��̾� ����
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
        bgmPlayer.clip = bgmClip;

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

    public void PlayBgm(bool isPlay)
    {
        // ���� AudioManager.Instance.PlayerBgm(true); // ������ false
        if (isPlay) {
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

            channelIndex = loopIndex;

            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx + randIndex];
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
}
